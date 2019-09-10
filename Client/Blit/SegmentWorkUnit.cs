namespace Arvid.Client.Blit
{
    internal struct SegmentWorkUnit
    {
        public ushort[] Data;
        public int Offset;
        public int Length;
        public ushort YPos;
        public ushort Stride;
    }
}