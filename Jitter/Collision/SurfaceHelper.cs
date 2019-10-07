using System;
using Jitter.LinearMath;

namespace Jitter.Collision
{
    // CUSTOM: Added to support surface movement.
    // Note that the utililty functions in this class are copied from the Zeldo Utilities class, but copying is simpler
    // than creating an entire helper project for a handful of functions.
    public static class SurfaceHelper
    {
        public static JVector ProjectOntoPlane(ref JVector v, ref JVector normal)
        {
            // See https://www.maplesoft.com/support/help/Maple/view.aspx?path=MathApps%2FProjectionOfVectorOntoPlane.
            return v - Project(ref v, ref normal);
        }

        public static JVector Project(ref JVector v, ref JVector onto)
        {
            // See https://math.oregonstate.edu/home/programs/undergrad/CalculusQuestStudyGuides/vcalc/dotprod/dotprod.html.
            return JVector.Dot(onto, v) / JVector.Dot(onto, onto) * onto;
        }

        public static float Angle(ref JVector v1, ref JVector v2)
        {
            // See https://www.analyzemath.com/stepbystep_mathworksheets/vectors/vector3D_angle.html.
            return (float)Math.Acos(JVector.Dot(v1, v2) / (v1.Length() * v2.Length()));
        }
    }
}
