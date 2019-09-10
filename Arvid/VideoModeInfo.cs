using System.Runtime.InteropServices;

namespace Arvid
{
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct VideoModeInfo
    {
        public readonly ushort Width;
        public readonly VideoMode VideoMode;

        public VideoModeInfo(ushort width, VideoMode videoMode)
        {
            Width = width;
            VideoMode = videoMode;
        }
    }
}