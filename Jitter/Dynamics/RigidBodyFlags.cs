using System;

namespace Jitter.Dynamics
{
    [Flags]
    public enum RigidBodyFlags
    {
        IsActive,
        IsAffectedByGravity,
        IsFixedVertical,
        IsOnPlatform,

        // TODO: Consider removing (if unused).
        IsParticle,
        IsSpawnTransformSet
    }
}
