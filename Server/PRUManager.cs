using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Mono.Unix.Native;
using NetPRUSSDriver;

namespace Arvid.Server
{
    public class PruManager
    {
        private uint _ddrAddress;
        private unsafe uint* _pruMem;
        private unsafe uint* _pruSharedMem;
        private unsafe uint* _ddrMem;
        private int _ddrFd;
        
        public bool Initialized { get; private set; }

        public void Init()
        {
            if (Initialized) return;
            
            var process = Process.Start("/sbin/modprobe uio_pruss  extram_pool_sz=0x400000");
            process.WaitForExit();
            if (process.ExitCode < 0)
            {
                throw new Exception("arvid: modprobe uio_pruss failed");
            }
            
            // Wait for the module to be loaded
            Thread.Sleep(100);
            
            process = Process.Start("echo pru_arvid > /sys/devices/bone_capemgr.9/slots");
            process.WaitForExit();
            if (process.ExitCode < 0)
            {
                throw new Exception("arvid: dtbo overlay failed");
            }
            
            // Wait for the cape to be loaded
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
                throw new Exception("arvid: uio address lookup failed.\n");
            }
            
            res = NativeDriver.prussdrv_init();
            if (res != 0) {
                throw new Exception($"arvid: prussdrv_init failed: {res}\n");
            }
            
            res = NativeDriver.prussdrv_open(NativeDriver.PRU_EVTOUT_1);
            if (res != 0) {
                throw new Exception($"arvid: prussdrv_open 1 failed: {res}\n");
            }
            
            /* initialize interrupt */
            var intc = NativeDriver.tpruss_intc_initdata.PRUSS_INTC_INITDATA();
            res = NativeDriver.prussdrv_pruintc_init(ref intc);
            if (res != 0) {
                throw new Exception($"arvid: prussdrv_pruintc_init() failed: {res}\n");
            }

            Initialized = true;
        }

        public void InitMemoryMapping()
        {
            unsafe
            {
                int fd;

                //map internal pru memory 
                NativeDriver.prussdrv_map_prumem(NativeDriver.PRUSS0_PRU1_DATARAM, out var pruMem);
                _pruMem = (uint*) pruMem.ToPointer();
                NativeDriver.prussdrv_map_prumem(NativeDriver.PRUSS0_SHARED_DATARAM, out var pruSharedMem);
                _pruSharedMem = (uint*) pruSharedMem.ToPointer();
                
                //set video mode info to pru
                _setPruMem(320, 252);

                //map DDR memory

                fd = Syscall.open("/dev/mem", OpenFlags.O_RDWR);
                if (fd < 0) {
                    throw new Exception($"arvid: failed to open /dev/mem {fd}\n");
                }
                _ddrFd = fd;
                _ddrMem = (uint*)Syscall.mmap(
                    IntPtr.Zero, 
                    0x400000, 
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
        }

        private void _setPruMem(int initialFbW, int initialFbLines)
        {
        }

        private unsafe int _getUioAddress(int id) {
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
                Console.WriteLine("arvid: found ddr address at: 0x{0}\n", Convert.ToString(_ddrAddress, 16));
            }

            return 0;
        }
    }
}