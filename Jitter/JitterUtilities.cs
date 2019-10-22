using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jitter
{
    // CUSTOM: This class was created by me (for the clamp function).
    public static class JitterUtilities
    {
        public static float Clamp(float f, float min, float max)
        {
            return (f < min) ? min : (f > max) ? max : f;
        }
    }
}
