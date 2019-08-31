using System.Runtime.InteropServices;

namespace Arvid.Response
{
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    public ref struct VsyncResponse
    {
        [FieldOffset(0)]
        public ushort id;
        [FieldOffset(2)]
        public uint frameNumber;
        [FieldOffset(6)]
        public uint buttons;

        [FieldOffset(0)]
        public unsafe fixed byte rawData[10];
    }
}