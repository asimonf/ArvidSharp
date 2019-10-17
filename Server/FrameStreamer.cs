using System;
using System.Threading;
using static NetPRUSSDriver.NativeDriver;


namespace Arvid.Server
{
    public static class FrameStreamer
    {
        public static bool Initialized { get; private set; } = false;

        private static bool _stopThread = false;
        private static bool _paused = false;

        private static readonly Thread Thread;
        private static readonly ManualResetEvent InitializedEvent;
        private static readonly AutoResetEvent PauseEvent;

        static FrameStreamer()
        {
            Thread = new Thread(DoWork);
            InitializedEvent = new ManualResetEvent(false);
            PauseEvent = new AutoResetEvent(false);
        }

        public static void Init()
        {
            if (Initialized) return;
            
            Thread.Start();
            InitializedEvent.WaitOne();
        }

        public static void PauseAtNextFrame()
        {
            if (_paused) return;
            
            _paused = true;
            PauseEvent.WaitOne();
        }

        public static void Resume()
        {
            if (!_paused) return;
            
            _paused = false;
            PauseEvent.Set();
        }

        private static unsafe void DoWork()
        {
            var dstFb = stackalloc ushort*[2];		//pru mini frame buffer in shared pru memory
            int width = PruManager.Width;
            int height = PruManager.Lines;

            //each mini frame buffer is worth of 4 lines
            dstFb[0] = (ushort*)(&PruManager.PruSharedMem[4 + 340]);

            dstFb[1] = dstFb[0];
            dstFb[1] += width * 4;
            
            while (true)
            {
                prussdrv_pru_wait_event(PRU_EVTOUT_1);
                prussdrv_pru_clear_event(PRU_EVTOUT_1, PRU0_ARM_INTERRUPT);

                width = PruManager.Width;
                
                dstFb[1] = dstFb[0];
                dstFb[1] += width * 4;

                if (_stopThread) break;
                if (_paused)
                {
                    PauseEvent.Set();
                    PauseEvent.WaitOne();
                    continue;
                }
                
                var frameNumber = PruManager.PruMem[PruManager.PruDataFrameNumber];
                var src = PruManager.FrameBuffer;
                var i = height;
                var fbIndex = 0;

                do
                {
                    Buffer.MemoryCopy(
                        src, 
                        dstFb[fbIndex], 
                        width << 3, 
                        width << 3
                    );

                    if (i >= 4)
                    {
                        prussdrv_pru_wait_event(PRU_EVTOUT_0);
                        prussdrv_pru_clear_event(PRU_EVTOUT_0, PRU0_ARM_INTERRUPT);
                        
                        if (_stopThread) break;

                        var line = (int)PruManager.PruSharedMem[2];
                        src = PruManager.FrameBuffer + width * line;
                        fbIndex = ((line - 1) & 7) >> 2;
                    }
                    
                    i -= 4;

                    // frame numbers are out of sync -> early exit
                    if (frameNumber != PruManager.PruMem[PruManager.PruDataFrameNumber]) break;
                } while (i > 0);
            }

            _stopThread = false;
        }
    }
}