using System.Runtime.InteropServices;

namespace Arvid.Response
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe ref struct EnumVideoResponse
    {
        public ushort id;
        public int responseData;
        public fixed byte videoModePayload[120];
    }
}