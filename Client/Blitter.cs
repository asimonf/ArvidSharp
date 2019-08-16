using System;
using System.Collections.Generic;
using System.Threading;
using Arvid.Client.Blit;

namespace Arvid.Client
{
    internal delegate void SegmentReadyHandler(SegmentWorkOutput output);

    internal class Blitter: IDisposable
    {
        private const int SegmentWorkerCount = 4;
        
        private readonly Thread _sendThread;

        private readonly SegmentWorker[] _segmentWorkers;
        private readonly ManualResetEvent _startWorkEvent;
        private readonly ManualResetEvent _finishSendEvent;
        private readonly ManualResetEvent[] _finishWorkEvents;
        private readonly List<SegmentWorkOutput> _workOutputList;
        
        private readonly SegmentOutputPool _outputPool = SegmentOutputPool.Instance;

        public event SegmentReadyHandler SegmentReady;
        
        public Blitter()
        {
            _sendThread = new Thread(_doSend);
            _startWorkEvent = new ManualResetEvent(false);
            _finishSendEvent = new ManualResetEvent(true);
            
            _finishWorkEvents = new ManualResetEvent[SegmentWorkerCount];;
            _segmentWorkers = new SegmentWorker[SegmentWorkerCount];
            _workOutputList = new List<SegmentWorkOutput>(16);

            for (var i = 0; i < SegmentWorkerCount; i++)
            {
                var finishWorkEvent = new ManualResetEvent(false);
                _segmentWorkers[i] = new SegmentWorker(_startWorkEvent, finishWorkEvent);
                _finishWorkEvents[i] = finishWorkEvent;
            }
        }

        public void Start()
        {
            _sendThread.Start();

            foreach (var worker in _segmentWorkers)
                worker.Start();
        }
        
        public void Stop()
        {
            foreach (var worker in _segmentWorkers)
                worker.Stop();

            if (!_sendThread.IsAlive) return;
            
            _sendThread.Interrupt();
            _sendThread.Join();
        }

        public void Wait() => _finishSendEvent.WaitOne();

        public void Blit(ushort[] data, int height, int stride)
        {
            // Wait for any current blitting to finish
            _finishSendEvent.WaitOne();
            
            var segmentSize = height / SegmentWorkerCount;
            var workerIndex = 0;

            // Round-robin assignment
            for (var segment = 0; segment < height; segment += segmentSize)
            {
                _segmentWorkers[workerIndex].AddWork(data, segment, segmentSize, stride);
                workerIndex = (workerIndex + 1) % _segmentWorkers.Length;
            }

            // Reset the handle the waiting handle
            _finishSendEvent.Reset();

            // Reset all workers' finish event
            foreach (var workEvent in _finishWorkEvents)
                workEvent.Reset();

            // Start work
            _startWorkEvent.Set();
        }

        private void _doSend()
        {
            try
            {
                while (true)
                {
                    // Reset the starting event and wait for it
                    _startWorkEvent.Reset();
                    _startWorkEvent.WaitOne();

                    // Wait for all of them to finish
                    WaitHandle.WaitAll(_finishWorkEvents);

                    foreach (var worker in _segmentWorkers)
                        while (worker.OutputQueue.Count > 0)
                            _workOutputList.Add(worker.OutputQueue.Dequeue());

                    _workOutputList.Sort((a, b) => a.YPos - b.YPos);

                    foreach (var output in _workOutputList)
                    {
                        SegmentReady?.Invoke(output);

                        _outputPool.Return(output);
                    }

                    _workOutputList.Clear();

                    _finishSendEvent.Set();
                }
            }
            catch (ThreadInterruptedException)
            {
                
            }
        }

        public void Dispose()
        {
            Stop();
            _startWorkEvent?.Dispose();
            
            for (var i = 0; i < _segmentWorkers.Length; i++)
            {
                _segmentWorkers[i].Dispose();
                _finishWorkEvents[i].Dispose();
            }
        }
    }
}