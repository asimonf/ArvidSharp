using System.Runtime.InteropServices;

namespace Arvid.Response
{
    [StructLayout(LayoutKind.Sequential)]
    public ref struct RegularResponse
    {
        public ushort id;
        public int responseData;
    }
}