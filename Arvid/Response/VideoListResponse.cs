using System.Runtime.InteropServices;

namespace Arvid.Response
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe ref struct VideoListResponse
    {
        public ushort id;
        public int responseData;
        public fixed byte videoModePayload[120];
    }
}