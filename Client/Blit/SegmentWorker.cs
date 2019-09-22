using System;
using System.Collections.Generic;
using System.Threading;
using static Zstandard.ExternMethods;

namespace Arvid.Client.Blit
{
    internal class SegmentWorker : IDisposable
    {
        // Not Owned
        private readonly ManualResetEvent _startWorkEvent;
        private readonly ManualResetEvent _finishWorkEvent;

        // Owned
        private readonly Thread _thread;
        private readonly IntPtr _zstdContext;
        private readonly SegmentOutputPool _outputPool = SegmentOutputPool.Instance;
        private readonly SegmentWorkItemPool _unitPool = SegmentWorkItemPool.Instance;

        // Actual State
        public readonly Queue<SegmentWorkUnit> WorkQueue;
        public readonly Queue<SegmentWorkOutput> OutputQueue;

        public SegmentWorker(
            ManualResetEvent startWorkEvent,
            ManualResetEvent finishWorkEvent
        )
        {
            _thread = new Thread(_doWork);
            _startWorkEvent = startWorkEvent;
            _finishWorkEvent = finishWorkEvent;
            WorkQueue = new Queue<SegmentWorkUnit>(10);
            OutputQueue = new Queue<SegmentWorkOutput>(10);
            _zstdContext = ZSTD_createCCtx();
        }

        public void Dispose()
        {
            Stop();
            if (IntPtr.Zero != _zstdContext) ZSTD_freeCCtx(_zstdContext);
        }

        public void AddWork(ushort[] data, int yPos, int lineCount, int stride)
        {
            var segmentSize = lineCount;
            if (lineCount * stride > Helper.MaxSegmentSize) segmentSize >>= 1;

            for (var i = 0; i < lineCount; i += segmentSize)
            {
                var segmentYPos = (ushort) (i + yPos);
                var offset = segmentYPos * stride;
                var length = segmentSize * stride;
                WorkQueue.Enqueue(_unitPool.Request(data, offset, length, segmentYPos, (ushort) stride));
            }
        }

        private void _doWork()
        {
            try
            {
                while (true)
                    unsafe
                    {
                        // Reset the starting event and wait for it
                        _startWorkEvent.Reset();
                        _startWorkEvent.WaitOne();

                        while (WorkQueue.Count > 0)
                        {
                            var workUnit = WorkQueue.Dequeue();

                            var inputSize = (ushort) (workUnit.Length << 1); // In bytes
                            var outputSize = (int) ZSTD_compressBound(new UIntPtr(inputSize)).ToUInt32(); // In bytes
                            var outputUnit = _outputPool.Request(outputSize, workUnit.YPos, workUnit.Stride, inputSize);
                            var outputBuffer = outputUnit.Output;

                            fixed (ushort* inputPtr = &workUnit.Data[workUnit.Offset])
                            fixed (ushort* outputPtr = outputBuffer)
                            {
                                outputUnit.CompressedSize = (ushort) ZSTD_compressCCtx(
                                    _zstdContext,
                                    new IntPtr(outputPtr),
                                    new UIntPtr((uint) outputSize),
                                    new IntPtr(inputPtr),
                                    new UIntPtr(inputSize),
                                    1
                                ).ToUInt32();
                            }

                            OutputQueue.Enqueue(outputUnit);
                        }

                        // Signal that this worker is done
                        _finishWorkEvent.Set();
                    }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        public void Start()
        {
            _thread.Start();
        }

        public void Stop()
        {
            if (!_thread.IsAlive) return;

            _thread.Interrupt();
            _thread.Join();
        }
    }
}