using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Mono.Unix.Native;
using NetPRUSSDriver;

namespace Arvid.Server
{
    public static class PruManager
    {
        // Frame buffer address index
        private const int PruDataFbAddr = 0;

        // Frame sequential number
        private const int PruDataFrameNumber = 1;

        // line length block count
        private const int PruDataBlockCount = 2;

        // number of lines to render
        private const int PruDataTotalLines = 3;

        // gpio state - buttons
        private const int PruDataGpioState = 4;

        // X position of the first pixel - allows to shift screen horizontally
        private const int PruDataLinePosMod = 5;

        // Currently rendered line
        private const int PruDataLineNumber = 6;

        // Pass universal timing data
        private const int PruDataUniversalTimings = 7;
        
        // Enable interlaced mode
        private const int PruDataInterlacingEnabled = 9;

        // Memory size of the shared memory pool of UIO
        private const int MemSize = 0x400000;

        private static uint _ddrAddress;
        private static unsafe uint* _pruMem;
        private static unsafe uint* _pruSharedMem;
        private static unsafe uint* _ddrMem;
        
        private static int _ddrFd;
        private static uint _linePosMod;
        private static bool _interlacing;

        private static uint _height;

        public static bool Initialized { get; private set; }

        public static object Sync = new object();

        private static unsafe int _getUioAddress(int id) {
            var buffer = stackalloc byte[32];

            var device = $"/sys/class/uio/uio{id}/maps/map1/addr";
            var fd = Syscall.open(device, OpenFlags.O_RDONLY);

            if (fd < 0) {
                return -1;
            }

            var size = Syscall.read(fd, buffer, 32);

            Syscall.close(fd);
            
            if (size >  5)
            {
                _ddrAddress = uint.Parse(new ReadOnlySpan<char>(buffer, (int) size), NumberStyles.HexNumber);
                Console.WriteLine($"arvid: found ddr address at: 0x{_ddrAddress:X}");
            }

            return 0;
        }

        private static void _initUio()
        {
            Helper.RunCommand(
                "/sbin/modprobe -rw uio_pruss",
                "Couldn't remove uio_pruss"
            );
            
            Helper.RunCommand(
                $"/sbin/modprobe uio_pruss extram_pool_sz=0x{MemSize:X}",
                "Couldn't insert uio_pruss"
            );
            
            // Wait for the module to be loaded
            Thread.Sleep(100);

            int res;
            
            //try to find uio address
            _ddrAddress = 0;
            for (var i = 0; i < 8; i++) {
                res = _getUioAddress(i);
                if (res == 0 && _ddrAddress != 0) {
                    break;
                }
            }
            
            if (_ddrAddress == 0) {
                throw new Exception("uio address lookup failed.\n");
            }
        }

        private static void _initPruss()
        {
            int res;
            
            res = NativeDriver.prussdrv_init();
            if (res != 0) {
                throw new Exception($"arvid: prussdrv_init failed: {res}\n");
            }
            
            res = NativeDriver.prussdrv_open(NativeDriver.PRU_EVTOUT_1);
            if (res != 0) {
                throw new Exception($"arvid: prussdrv_open 1 failed: {res}\n");
            }
            
            res = NativeDriver.prussdrv_open(NativeDriver.PRU_EVTOUT_2);
            if (res != 0) {
                throw new Exception($"arvid: prussdrv_open 2 failed: {res}\n");
            }
            
            /* initialize interrupt */
            var intc = NativeDriver.tpruss_intc_initdata.PRUSS_INTC_INITDATA();
            res = NativeDriver.prussdrv_pruintc_init(ref intc);
            if (res != 0) {
                throw new Exception($"arvid: prussdrv_pruintc_init() failed: {res}\n");
            }
        }

        private static unsafe void _initMemory()
        {
            int fd;

            //map internal pru memory 
            NativeDriver.prussdrv_map_prumem(NativeDriver.PRUSS0_PRU1_DATARAM, out var pruMem);
            _pruMem = (uint*) pruMem.ToPointer();
            NativeDriver.prussdrv_map_prumem(NativeDriver.PRUSS0_SHARED_DATARAM, out var pruSharedMem);
            _pruSharedMem = (uint*) pruSharedMem.ToPointer();
            
            //set video mode info to pru
            _setPruMem(320, 252, 0);

            //map DDR memory
            fd = Syscall.open("/dev/mem", OpenFlags.O_RDWR);
            if (fd < 0) {
                throw new Exception($"arvid: failed to open /dev/mem {fd}\n");
            }
            _ddrFd = fd;
            _ddrMem = (uint*)Syscall.mmap(
                IntPtr.Zero, 
                MemSize, 
                MmapProts.PROT_READ | MmapProts.PROT_WRITE, 
                MmapFlags.MAP_SHARED, 
                _ddrFd, 
                _ddrAddress
            ).ToPointer();

            if (IntPtr.Zero.ToPointer() == _ddrMem) {
                throw new Exception("arvid: failed allocate DMA memory\n");
            }

            _ddrMem[0] = 0;
        }

        private static unsafe void _setPruMem(uint fbWidth, uint fbLines, int mode)
        {
            //note: pruMem[1] contains current frame number
            _pruMem[PruDataFbAddr] = _ddrAddress;

            //set BLOCK_COUNT to PRU (horizontal resolution)
            var blockCnt = fbWidth / 32;
            if ((fbWidth % 32) == 0) {
                _pruMem[PruDataBlockCount] = blockCnt; //stream 32 pixels per block (64 bytes) 
            } else {
                //stream one more block
                blockCnt++;
                //set flag to properly adjust the next line address
                //note: '<< 17' means 16 + 1. 16 to offset to high word, 1 to multiply the value * 2 
                //to get number of bytes to detract from the frame-buffer address
                _pruMem[PruDataBlockCount] = blockCnt | ((32 - (fbWidth % 32)) << 17) ; //stream 32 pixels per block (64 bytes) 
            }

            //set TOTAL_LINES to PRU (vertical resolution)
            _pruMem[PruDataTotalLines] = fbLines;
            _pruMem[PruDataLinePosMod] = _linePosMod;
	
            //Universal timing data
            var entry = RateTable.ModeCyclesTable[mode];
            
            _pruMem[PruDataUniversalTimings] = entry.PixelData();
            _pruMem[PruDataUniversalTimings + 1] = entry.TimingData();

            //Interlacing
            _pruMem[PruDataInterlacingEnabled] = _interlacing ? uint.MaxValue : 0;
        }

        public static void Init()
        {
            if (Initialized) return;
            
            if (Syscall.geteuid() != 0)
            {
                throw new Exception("arvid: error ! permission check failed. Superuser required.\n");
            }
            
            _initUio();
            _initPruss();
            _initMemory();

            Initialized = true;
        }

        public static unsafe uint GetFrameNumber()
        {
            return _pruMem[PruDataFrameNumber];
        }

        public static unsafe void WaitForVsync()
        {
            NativeDriver.prussdrv_pru_wait_event(NativeDriver.PRU_EVTOUT_2);
            NativeDriver.prussdrv_pru_clear_event(NativeDriver.PRU_EVTOUT_2, NativeDriver.PRU0_ARM_INTERRUPT);
        }

        public static unsafe uint GetButtons()
        {
            return ~_pruMem[PruDataGpioState];
        }

        public static void SetVideoMode(VideoMode mode, int lines)
        {
            
        }
    }
}