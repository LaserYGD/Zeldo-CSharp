using System;

namespace Jitter.Dynamics
{
    [Flags]
    public enum RigidBodyFlags
    {
        AreSpeculativeContactsEnabled = 1<<0,

        IsActive = 1<<1,
        IsAffectedByGravity = 1<<2,
        IsDeactivationAllowed = 1<<3,
        IsFixedVertical = 1<<4,
        IsOnPlatform = 1<<5,

        // TODO: Consider removing (if unused).
        IsParticle = 1<<6,
        IsSpawnOrientationSet = 1<<7,
        IsSpawnPositionSet = 1<<8,

        UseShapeMassProperties = 1<<9
    }
}
