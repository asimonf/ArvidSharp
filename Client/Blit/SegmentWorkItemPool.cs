using System.Buffers;
using System.Collections.Concurrent;

namespace Arvid.Client.Blit
{
    public class SegmentWorkItemPool
    {
        private readonly ConcurrentQueue<SegmentWorkUnit> _pool;

        private static SegmentWorkItemPool _reference;

        public static SegmentWorkItemPool Instance => _reference ??= new SegmentWorkItemPool();

        private SegmentWorkItemPool()
        {
            _pool = new ConcurrentQueue<SegmentWorkUnit>();
        }

        public SegmentWorkUnit Request(ushort[] data, int offset, int length, ushort yPos, ushort stride)
        {
            var res = _pool.TryDequeue(out var obj);
            
            if (!res)
                obj = new SegmentWorkUnit();

            obj.Data = data;
            obj.Length = length;
            obj.Offset = offset;
            obj.YPos = yPos;
            obj.Stride = stride;

            return obj;
        }

        public void Return(SegmentWorkUnit obj)
        {
            obj.Data = null;
            obj.Length = -1;
            obj.Offset = -1;
            obj.YPos = 0;
            obj.Stride = 0;
            _pool.Enqueue(obj);
        }
    }
}