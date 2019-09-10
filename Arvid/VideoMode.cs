namespace Arvid
{
    public enum VideoMode: ushort
    {
        Mode320   =  0,
        Mode256   =  1,
        Mode288   =  2,
        Mode384   =  3,
        Mode240   =  4,
        Mode392   =  5,
        Mode400   =  6,
        Mode292   =  7,
        Mode336   =  8,
        Mode416   =  9,
        Mode448   = 10,
        Mode512   = 11,
        Mode640   = 12,
        ModeCount = 13,
        
        Invalid = ushort.MaxValue
    }
}