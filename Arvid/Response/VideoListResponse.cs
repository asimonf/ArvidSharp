using System.Runtime.InteropServices;

namespace Arvid.Response
{
    [StructLayout(LayoutKind.Explicit)]
    public unsafe ref struct VideoListResponse
    {
        [FieldOffset(0)]
        public ushort id;
        
        [FieldOffset(2)]
        public int responseData;
        
        [FieldOffset(6)]
        public fixed byte videoModePayload[120];
        
        [FieldOffset(0)]
        public fixed byte rawData[126];
    }
}