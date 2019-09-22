namespace Arvid.Client.Blit
{
    internal struct SegmentWorkOutput
    {
        public ushort[] Output;
        public ushort CompressedSize;
        public ushort OriginalSize;
        public ushort YPos;
        public ushort Stride;
    }
}