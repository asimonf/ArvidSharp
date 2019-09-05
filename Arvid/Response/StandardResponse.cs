using System.Runtime.InteropServices;

namespace Arvid.Response
{
    [StructLayout(LayoutKind.Explicit, Pack = 2)]
    public ref struct StandardResponse
    {
        [FieldOffset(0)]
        public ushort id;
        [FieldOffset(2)]
        public int responseData;

        [FieldOffset(0)]
        public unsafe fixed byte rawData[6];
    }
}