namespace Arvid
{
    public enum CommandType: ushort
    {
        Blit = 1,
        GetFrameNumber = 2,
        Vsync = 3,
        SetVideoMode = 4,
        GetVideoModeLines = 5,
        GetVideoModeFrequency = 6,
        GetWidth = 7,
        GetHeight = 8,
        EnumVideoModes = 9,
        GetVideoModeCount = 10,
        
        Init = 11,
        Close = 12,
        
        GetLineMod = 32,
        SetLineMod = 33,
        SetVirtualSync = 34,
        
        UpdateStart = 40,
        UpdatePacket = 41,
        UpdateEnd = 42,
        
        PowerOff = 50
    }
}