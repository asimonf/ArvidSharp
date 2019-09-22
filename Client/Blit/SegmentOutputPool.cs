using System.Buffers;
using System.Collections.Concurrent;

namespace Arvid.Client.Blit
{
    internal class SegmentOutputPool
    {
        private readonly ConcurrentQueue<SegmentWorkOutput> _pool;
        private readonly ArrayPool<ushort> _arrayPool;

        private static SegmentOutputPool _reference;

        public static SegmentOutputPool Instance => _reference ??= new SegmentOutputPool();

        private SegmentOutputPool()
        {
            _arrayPool = ArrayPool<ushort>.Create();
            _pool = new ConcurrentQueue<SegmentWorkOutput>();
        }

        public SegmentWorkOutput Request(int bufferSize, ushort yPos, ushort stride, ushort originalSize)
        {
            var res = _pool.TryDequeue(out var obj);
            
            if (!res)
                obj = new SegmentWorkOutput();

            obj.Output = _arrayPool.Rent(bufferSize);
            obj.YPos = yPos;
            obj.Stride = stride;
            obj.OriginalSize = originalSize;

            return obj;
        }

        public void Return(SegmentWorkOutput obj)
        {
            _arrayPool.Return(obj.Output);
            obj.Output = null;
            obj.YPos = 0;
            obj.Stride = 0;
            obj.OriginalSize = 0;
            _pool.Enqueue(obj);
        }
    }
}