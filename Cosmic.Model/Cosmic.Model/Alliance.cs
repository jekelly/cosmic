using System;

namespace Cosmic.Model
{
    [Flags]
    public enum Alliance
    {
        Neither = 0x0,
        Offense = 0x1,
        Defense = 0x2,
    }
}
