using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Mono.Unix.Native;
using static NetPRUSSDriver.NativeDriver;

namespace Arvid.Server
{
    public static class PruManager
    {
        // Frame buffer address index
        public const int PruDataFbAddr = 0;

        // Frame sequential number
        public const int PruDataFrameNumber = 1;

        // line length block count
        public const int PruDataBlockCount = 2;

        // number of lines to render
        public const int PruDataTotalLines = 3;

        // gpio state - buttons
        public const int PruDataGpioState = 4;

        // X position of the first pixel - allows to shift screen horizontally
        public const int PruDataLinePosMod = 5;

        // Currently rendered line
        public const int PruDataLineNumber = 6;

        // Pass universal timing data
        public const int PruDataUniversalTimings = 7;

        // Enable interlaced mode
        public const int PruDataInterlacingEnabled = 9;

        // Memory size of the shared memory pool of UIO
        private const int MemSize = 0x400000;

        private static int _ddrFd;

        private static uint _ddrAddress;
        private static unsafe uint* _pruMem;
        private static unsafe uint* _pruSharedMem;
        private static unsafe uint* _ddrMem;

        // Geometry

        public static bool Interlacing { get; private set; }

        public static ushort LinePosMod { get; private set; } = 60;

        public static ushort Width { get; private set; }
        public static ushort Height { get; private set; }
        public static ushort Lines { get; private set; }

        public static uint VsyncLine { get; private set; }

        public static unsafe ushort* FrameBuffer { get; private set; }

        public static unsafe uint* PruMem => _pruMem;
        public static unsafe uint* PruSharedMem => _pruSharedMem;

        public static bool Initialized { get; private set; }

        private static unsafe int _getUioAddress(int id)
        {
            var buffer = stackalloc byte[32];

            var device = $"/sys/class/uio/uio{id}/maps/map1/addr";
            var fd = Syscall.open(device, OpenFlags.O_RDONLY);

            if (fd < 0)
            {
                return -1;
            }

            var size = Syscall.read(fd, buffer, 32);

            Syscall.close(fd);

            if (size > 5)
            {
                var str = System.Text.Encoding.ASCII.GetString(new ReadOnlySpan<byte>(buffer, (int)size)).Trim();
                _ddrAddress = Convert.ToUInt32(str, 16);
                Console.WriteLine($"arvid: found ddr address at: 0x{_ddrAddress:X} with size {size}");
            }

            return 0;
        }

        private static void _initUio()
        {
            //try to find uio address
            _ddrAddress = 0;
            for (var i = 0; i < 8; i++)
            {
                var res = _getUioAddress(i);
                if (res == 0 && _ddrAddress != 0)
                {
                    break;
                }
            }

            if (_ddrAddress == 0)
            {
                throw new Exception("uio address lookup failed.\n");
            }
        }

        private static void _initPruss()
        {
            var res = prussdrv_init();
            if (res != 0)
                throw new Exception($"arvid: prussdrv_init failed: {res}\n");
            
            res = prussdrv_open(PRU_EVTOUT_0);
            if (res != 0)
                throw new Exception($"arvid: prussdrv_open 0 failed: {res}\n");

            res = prussdrv_open(PRU_EVTOUT_1);
            if (res != 0)
                throw new Exception($"arvid: prussdrv_open 1 failed: {res}\n");

            res = prussdrv_open(PRU_EVTOUT_2);
            if (res != 0)
                throw new Exception($"arvid: prussdrv_open 2 failed: {res}\n");

            /* initialize interrupt */
            var intc = tpruss_intc_initdata.PRUSS_INTC_INITDATA();
            res = prussdrv_pruintc_init(ref intc);
            if (res != 0)
                throw new Exception($"arvid: prussdrv_pruintc_init() failed: {res}\n");
        }

        private static unsafe void _initMemory()
        {
            //map internal pru memory 
            prussdrv_map_prumem(PRUSS0_PRU1_DATARAM, out var pruMem);
            _pruMem = (uint*) pruMem.ToPointer();
            prussdrv_map_prumem(PRUSS0_SHARED_DATARAM, out var pruSharedMem);
            _pruSharedMem = (uint*) pruSharedMem.ToPointer();

            //set video mode info to pru
            _setPruMem(VideoMode.Mode320, 252);

            //map DDR memory
            var fd = Syscall.open("/dev/mem", OpenFlags.O_RDWR);
            if (fd < 0)
            {
                throw new Exception($"arvid: failed to open /dev/mem {fd}\n");
            }

            _ddrFd = fd;
            _ddrMem = (uint*) Syscall.mmap(
                IntPtr.Zero,
                MemSize,
                MmapProts.PROT_READ | MmapProts.PROT_WRITE,
                MmapFlags.MAP_SHARED,
                _ddrFd,
                _ddrAddress
            ).ToPointer();

            if (IntPtr.Zero.ToPointer() == _ddrMem)
            {
                throw new Exception("arvid: failed allocate DMA memory\n");
            }

            _ddrMem[0] = 0;

            FrameBuffer = (ushort*) &_ddrMem[16];
        }

        private static void _loadPruCode()
        {
            var res = prussdrv_exec_program(0, "./prustream.bin");
            if (res != 0)
                throw new Exception($"arvid: failed to load prustream.bin");

            res = prussdrv_exec_program(1, "./pruvideo_universal.bin");
            if (res != 0)
                throw new Exception($"arvid: failed to load pruvideo_universal.bin");
        }

        private static void _setPruMem(VideoMode mode, int lines)
        {
            _setPruMem((int) mode, lines);
        }

        private static unsafe void _setPruMem(int mode, int fbLines)
        {
            Height = 224;
            Lines = (ushort)fbLines;
            VsyncLine = Height;
            
            var fbWidth = VideoModeHelpers.ModeWidthTable[mode];
            //note: pruMem[1] contains current frame number
            _pruMem[PruDataFbAddr] = _ddrAddress;

            //set BLOCK_COUNT to PRU (horizontal resolution)
            var blockCnt = fbWidth / 32;
            if ((fbWidth % 32) == 0)
            {
                _pruMem[PruDataBlockCount] = (uint) blockCnt; //stream 32 pixels per block (64 bytes) 
            }
            else
            {
                //stream one more block
                blockCnt++;
                //set flag to properly adjust the next line address
                //note: '<< 17' means 16 + 1. 16 to offset to high word, 1 to multiply the value * 2 
                //to get number of bytes to detract from the frame-buffer address
                _pruMem[PruDataBlockCount] =
                    (uint) (blockCnt | ((32 - (fbWidth % 32)) << 17)); //stream 32 pixels per block (64 bytes) 
            }

            //set TOTAL_LINES to PRU (vertical resolution)
            _pruMem[PruDataTotalLines] = (uint) fbLines;
            _pruMem[PruDataLinePosMod] = LinePosMod;

            //set the line to trigger vsync
            _pruMem[PruDataLineNumber] = VsyncLine;

            //Universal timing data
            var entry = VideoModeHelpers.ModeCyclesTable[mode];

            _pruMem[PruDataUniversalTimings] = entry.PixelData();
            _pruMem[PruDataUniversalTimings + 1] = entry.TimingData();

            //Interlacing
            _pruMem[PruDataInterlacingEnabled] = Interlacing ? uint.MaxValue : 0;
        }

        private static void _initFrameBuffers(VideoMode mode, ushort lines, bool noFbClear)
        {
            Lines = lines;
            Width = (ushort) VideoModeHelpers.ModeWidthTable[(int) mode];

            if (noFbClear) return;

            //clean both frames with black color
            FillRect(0, 0, Width, Lines, 0xF0);
//            arvid_fill_rect(0, 0, 0, ap.fbWidth, ap.lines, 0);
//            arvid_fill_rect(1, 0, 0, ap.fbWidth, ap.lines, 0);
        }

        public static void Init()
        {
            if (Initialized) return;

            if (Syscall.geteuid() != 0)
            {
                throw new Exception("arvid: error ! permission check failed. Superuser required.\n");
            }
            
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.AboveNormal;

            _initUio();
            _initPruss();
            _initMemory();
            SetVideoMode(VideoMode.Mode320, 252);
            _loadPruCode();
            FrameStreamer.Init();

            Initialized = true;
        }

        public static unsafe uint GetFrameNumber()
        {
            return _pruMem[PruDataFrameNumber];
        }

        public static void WaitForVsync()
        {
            prussdrv_pru_wait_event(PRU_EVTOUT_2);
            prussdrv_pru_clear_event(PRU_EVTOUT_2, PRU1_PRU0_INTERRUPT);
        }

        public static unsafe uint GetButtons()
        {
            return ~_pruMem[PruDataGpioState];
        }

        public static unsafe void SetVideoMode(VideoMode mode, ushort lines)
        {
            if (lines == Lines)
            {
                _setPruMem(mode, lines);
                _ddrMem[0] = 0;
                _initFrameBuffers(mode, lines, false);
                return;
            }

//            FrameStreamer.PauseAtNextFrame();

            _setPruMem(mode, lines);
            _ddrMem[0] = 0;
            _initFrameBuffers(mode, lines, false);

            VsyncLine = Height;
            
            _loadPruCode();

//            FrameStreamer.Resume();
        }

        public static void SetVsyncLine(int line)
        {
            if (line < 0 || line > Height) VsyncLine = Height;
            else VsyncLine = (uint)line;
        }

        public static unsafe void SetInterlacing(int interlacing)
        {
            Interlacing = interlacing > 0;
            _pruMem[PruDataInterlacingEnabled] = Interlacing ? uint.MaxValue : 0;
        }

        public static void SetLinePosMod(ushort linePosMod)
        {
            if (linePosMod < 1)
            {
                linePosMod = 1;
            }
            else if (linePosMod > 111)
            {
                linePosMod = 111;
            }

            LinePosMod = linePosMod;
        }

        public static unsafe void FillRect(int x, int y, int w, int h, ushort color)
        {
            int i, j;

            var fb = FrameBuffer + y * Width + x;

            for (j = 0; j < h; j++)
            {
                for (i = 0; i < w; i++)
                {
                    fb[i] = color;
                }

                fb += Width;
            }
        }

        public static unsafe void Blit(int x, int y, int w, int h, ushort* srcRef, bool rotate)
        {
            int j, i;
            var ptr = FrameBuffer;
            var maxW = Width;
            var srcPtr = srcRef;
            var hm1 = h - 1;

            ptr += maxW * y + x;

            if (rotate)
            {
                for (j = 0; j < w; j++)
                {
                    for (i = 0; i < h; i++)
                    {
                        var srcIndex = (hm1 - i) * w + j;
                        ptr[i] = srcPtr[srcIndex];
                    }

                    ptr += maxW;
                }
            }
            else
            {
                for (j = 0; j < h; j++)
                {
                    srcPtr = srcRef + j * w;
                    for (i = 0; i < w; i++)
                    {
                        ptr[i] = *srcPtr;
                        srcPtr++;
                    }

                    ptr += maxW;
                }
            }
        }
    }
}