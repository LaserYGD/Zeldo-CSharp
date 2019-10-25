/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
* 
*  This software is provided 'as-is', without any express or implied
*  warranty.  In no event will the authors be held liable for any damages
*  arising from the use of this software.
*
*  Permission is granted to anyone to use this software for any purpose,
*  including commercial applications, and to alter it and redistribute it
*  freely, subject to the following restrictions:
*
*  1. The origin of this software must not be misrepresented; you must not
*      claim that you wrote the original software. If you use this software
*      in a product, an acknowledgment in the product documentation would be
*      appreciated but is not required.
*  2. Altered source versions must be plainly marked as such, and must not be
*      misrepresented as being the original software.
*  3. This notice may not be removed or altered from any source distribution. 
*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Collision;
using Jitter.Dynamics.Constraints;
using Jitter.DataStructures;

namespace Jitter.Dynamics
{
    /// <summary>
    /// The RigidBody class.
    /// </summary>
    public class RigidBody : IBroadphaseEntity, IEquatable<RigidBody>, IComparable<RigidBody>
	{
		[Flags]
		public enum DampingType
		{
			None = 0x00,
			Angular = 0x01,
			Linear = 0x02
		}

		private static int instanceCount;

		private readonly int instance;
        private readonly int hashCode;

        private RigidBodyTypes bodyType;

		private Shape shape;
		private ShapeUpdatedHandler updatedHandler;
	    private ReadOnlyHashset<Arbiter> readOnlyArbiters;
	    private ReadOnlyHashset<Constraint> readOnlyConstraints;

        // TODO: Consider removing (if shapes never change).
        protected bool useShapeMassProperties = true;

		internal JMatrix inertia;
        internal JMatrix invInertia;
        internal JMatrix invInertiaWorld;
        internal JMatrix orientation;
        internal JMatrix oldOrientation;
        internal JMatrix invOrientation;

        internal JVector position;
	    internal JVector oldPosition;
        internal JVector linearVelocity;
        internal JVector angularVelocity;
	    internal JVector force;
	    internal JVector torque;
		internal JVector sweptDirection;

        // CUSTOM: Let's see if this works.
	    internal JVector storedLinear;
	    internal JVector storedAngular;

	    // TODO: Is material used?
        internal Material material;
        internal JBBox boundingBox;
	    internal CollisionIsland island;

		internal int marker = 0;

		internal float inactiveTime;
	    internal float inverseMass;

        internal RigidBodyFlags flags;

		internal List<RigidBody> connections = new List<RigidBody>();
        internal HashSet<Arbiter> arbiters = new HashSet<Arbiter>();
        internal HashSet<Constraint> constraints = new HashSet<Constraint>();

        public RigidBody(Shape shape, RigidBodyTypes bodyType, RigidBodyFlags flags = 0) :
            this(shape, bodyType, new Material(), flags)
        {
        }

        public RigidBody(Shape shape, RigidBodyTypes bodyType, Material material, RigidBodyFlags flags = 0)
        {
            Debug.Assert(shape != null, "Shape can't be null.");
            Debug.Assert(material != null, "Material can't be null.");

            this.bodyType = bodyType;
            this.material = material;

            // By default, all bodies start active (and with deactivation allowed).
            this.flags = flags | RigidBodyFlags.IsActive | RigidBodyFlags.IsDeactivationAllowed;

            readOnlyArbiters = new ReadOnlyHashset<Arbiter>(arbiters);
            readOnlyConstraints = new ReadOnlyHashset<Constraint>(constraints);

            instance = Interlocked.Increment(ref instanceCount);
            hashCode = CalculateHash(instance);

            Shape = shape;
            Damping = DampingType.Angular | DampingType.Linear;
            orientation = JMatrix.Identity;

            if (!IsParticle)
            {
                updatedHandler = ShapeUpdated;
                Shape.ShapeUpdated += updatedHandler;
                SetMassProperties();
            }
            else
            {
                inertia = JMatrix.Zero;
                invInertia = invInertiaWorld = JMatrix.Zero;
                invOrientation = orientation = JMatrix.Identity;
                inverseMass = 1.0f;
            }
            
            Update();
        }

        public JBBox BoundingBox => boundingBox;
	    public CollisionIsland CollisionIsland => island;
        public ReadOnlyHashset<Arbiter> Arbiters => readOnlyArbiters;
	    public ReadOnlyHashset<Constraint> Constraints => readOnlyConstraints;

	    public Material Material
	    {
	        get => material;
	        set => material = value;
	    }

        public Shape Shape 
        {
            get => shape;
	        set 
            {
	            if (shape != null)
	            {
		            shape.ShapeUpdated -= updatedHandler;
	            }

                shape = value; 
                shape.ShapeUpdated += ShapeUpdated; 
            } 
        }

	    public object Tag { get; set; }

        public DampingType Damping { get; set; }

        public JMatrix Inertia => inertia;
        public JMatrix InverseInertia => invInertia;
	    public JMatrix InverseInertiaWorld => invInertiaWorld;

	    public JVector Torque => torque;
	    public JVector Force
	    {
	        get => force;
	        set => force = value;
	    }

        /// <summary>
        /// Linear velocity of the body.
        /// </summary>
        public JVector LinearVelocity
        {
            get => linearVelocity;
	        set 
            {
				Debug.Assert(!IsStatic, "Can't set linear velocity on a static or pseudo-static body.");
				Debug.Assert(!IsOnPlatform, "Can't set linear velocity directly on a body that's on a platform (use " +
                    "SetTransform with a timestep instead).");

	            linearVelocity = value;
            }
        }
		
        /// <summary>
        /// Angular velocity of the body.
        /// </summary>
        public JVector AngularVelocity
        {
            get => angularVelocity;
	        set
            {
				Debug.Assert(!IsStatic, "Can't set angular velocity on a static or pseudo-static body.");
				Debug.Assert(!IsFixedVertical, "Can't set angular velocity on a fixed-vertical body.");
				Debug.Assert(!IsOnPlatform, "Can't set angular velocity directly on a body that's on a platform (use" +
                    "SetTransform with a timestep instead).");

	            angularVelocity = value;
            }
        }

        /// <summary>
        /// Position of the body.
        /// </summary>
        public JVector Position
        {
            get => position;
            set
            {
                bool isSpawnPositionUnset = !IsSpawnPositionSet;

                Debug.Assert(bodyType != RigidBodyTypes.Static || isSpawnPositionUnset, "Static body position can " +
                    "only be set on spawn.");
                Debug.Assert(bodyType != RigidBodyTypes.PseudoStatic || isSpawnPositionUnset, "Pseudo-static body " +
                    "position can only be set directly on spawn. After that, use SetTransform with a timestep.");
                Debug.Assert(!IsOnPlatform || isSpawnPositionUnset, "Bodies on platforms can only have position set " +
                    "directly on spawn. After that, use SetTransform with a timestep.");

                if (isSpawnPositionUnset)
                {
                    oldPosition = value;
                    IsSpawnPositionSet = true;
                }
                else
                {
                    oldPosition = position;
                }

                position = value;
                Update();
            }
        }

	    public JVector OldPosition => oldPosition;

        /// <summary>
        /// The current oriention of the body.
        /// </summary>
        public JMatrix Orientation
        {
            get => orientation;
	        set
            {
                bool isSpawnOrientationUnset = !IsSpawnOrientationSet;

                Debug.Assert(bodyType != RigidBodyTypes.Static || isSpawnOrientationUnset, "Static body orientation " +
                    "can only be set on spawn.");
                Debug.Assert(bodyType != RigidBodyTypes.PseudoStatic || isSpawnOrientationUnset, "Pseudo-static " +
                    "body orientation can only be set directly on spawn. After that, use SetTransform with a " +
                    "timestep.");
                Debug.Assert(!IsOnPlatform || isSpawnOrientationUnset, "Bodies on platforms can only have " +
                    "orientation set directly on spawn. After that, use SetTransform with a timestep.");

                if (isSpawnOrientationUnset)
                {
                    oldOrientation = value;
                    IsSpawnOrientationSet = true;
                }
                else
                {
                    oldOrientation = orientation;
                }

                orientation = value;
		        Update();
	        }
        }

	    public bool AreSpeculativeContactsEnabled
	    {
	        get => (flags & RigidBodyFlags.AreSpeculativeContactsEnabled) > 0;
	        set => ModifyFlag(RigidBodyFlags.AreSpeculativeContactsEnabled, value);
	    }

        /// <summary>
        /// If set to false, velocities are set to zero and the body is immediately frozen.
        /// </summary>
        public bool IsActive
        {
            get => (flags & RigidBodyFlags.IsActive) > 0;
            set
            {
                bool isActive = (flags & RigidBodyFlags.IsActive) > 0;

                if (!isActive && value)
                {
                    inactiveTime = 0;
                }
                else if (isActive && !value)
                {
                    inactiveTime = float.PositiveInfinity;
                    angularVelocity.MakeZero();
                    linearVelocity.MakeZero();
                }

                ModifyFlag(RigidBodyFlags.IsActive, value);
            }
        }

        public bool IsAffectedByGravity
        {
            get => (flags & RigidBodyFlags.IsAffectedByGravity) > 0;
            set
            {
                Debug.Assert(!IsStatic, "The gravity property shouldn't be called on static or pseudo-static bodies " +
                    "since they're not affected by gravity anyway).");

                ModifyFlag(RigidBodyFlags.IsAffectedByGravity, value);
            }
        }

	    public bool IsDeactivationAllowed
	    {
	        get => (flags & RigidBodyFlags.IsDeactivationAllowed) > 0;
	        set => ModifyFlag(RigidBodyFlags.IsDeactivationAllowed, value);
	    }

        public bool IsFixedVertical
	    {
		    get => (flags & RigidBodyFlags.IsFixedVertical) > 0;

            // TODO: Should this setter be removed? If a body is fixed-vertical, it can probably be assumed that it'll always stay that way.
		    set
		    {
                Debug.Assert(!IsStatic, "Marking a body as fixed-vertical causes angular velocity to not be " +
                    "applied. As such, it's redundant to set on static and pseudo-static bodies (since angular " +
                    "velocity is already not applied).");

		        if (value)
                {
                    // If a body is fixed-vertical, it's assumed its orientation will be set manually (via control
                    // code). As such, angular velocity is disallowed.
		            angularVelocity.MakeZero();
                }
                
                ModifyFlag(RigidBodyFlags.IsFixedVertical, value);
		    }
	    }

	    public bool IsOnPlatform
	    {
	        get => (flags & RigidBodyFlags.IsOnPlatform) > 0;
	        set
	        {
                Debug.Assert(bodyType == RigidBodyTypes.Kinematic, "Only kinematic bodies can be marked with " +
                    "platform handling (dynamic bodies just use regular physics).");

	            ModifyFlag(RigidBodyFlags.IsOnPlatform, value);
	        }
	    }

	    // TODO: Consider removing (if unused).
	    /// <summary>
	    /// If true, the body as no angular movement.
	    /// </summary>
	    public bool IsParticle
	    {
	        get => (flags & RigidBodyFlags.IsParticle) > 0;
	        set
	        {
                Debug.Assert(!IsStatic, "The particle property shouldn't be called on static and pseudo-static " +
                    "bodies (since they already ignore angular velocity).");

	            var isParticle = (flags & RigidBodyFlags.IsParticle) > 0;

	            if (isParticle && !value)
	            {
	                updatedHandler = ShapeUpdated;
	                Shape.ShapeUpdated += updatedHandler;
	                SetMassProperties();
	            }
	            else if (!isParticle && value)
	            {
	                inertia = JMatrix.Zero;
	                invInertia = invInertiaWorld = JMatrix.Zero;
	                invOrientation = orientation = JMatrix.Identity;
	                inverseMass = 1.0f;

	                Shape.ShapeUpdated -= updatedHandler;
	            }

	            ModifyFlag(RigidBodyFlags.IsParticle, value);
	            Update();
	        }
	    }

        public bool IsSpawnPositionSet
        {
            get => (flags & RigidBodyFlags.IsSpawnPositionSet) > 0;
            private set => ModifyFlag(RigidBodyFlags.IsSpawnPositionSet, value);
        }

        public bool IsSpawnOrientationSet
        {
            get => (flags & RigidBodyFlags.IsSpawnOrientationSet) > 0;
            private set => ModifyFlag(RigidBodyFlags.IsSpawnOrientationSet, value);
        }

        public bool IsStatic => (int)bodyType >= (int)RigidBodyTypes.PseudoStatic;
	    public bool IsStaticOrInactive => IsStatic || !IsActive;

        // TODO: If kept, this should be a flag as well.
        public bool RequiresResolution { get; set; }

        // TODO: Allow body type to be changed after creation (if necessary).
        public RigidBodyTypes BodyType => bodyType;

	    // The code below was previously in the IsStatic setter. Might need it later.
        /*
        if (value && bodyType != RigidBodyTypes.Static)
        {
	        island?.islandManager.MakeBodyStatic(this);

	        angularVelocity.MakeZero();
            linearVelocity.MakeZero();
        }
         */

        /// <summary>
        /// Setting the mass automatically scales inertia. To set mass indepedently, use SetMassProperties.
        /// </summary>
        public float Mass
        {
            get => 1.0f / inverseMass;
	        set 
            {
				Debug.Assert(value > 0, "Mass must be positive.");

                // scale inertia
                if (!IsParticle)
                {
                    JMatrix.Multiply(ref Shape.inertia, value / Shape.mass, out inertia);
                    JMatrix.Inverse(ref inertia, out invInertia);
                }

                inverseMass = 1.0f / value;
            }
        }

        // TODO: Is this used?
	    public int BroadphaseTag { get; set; }

        // TODO: Consider adding normal to this callback (to avoid having to recompute it later).
		public Func<RigidBody, JVector[], bool> ShouldGenerateContact { get; set; }

        // Mid-step occurs after contacts are generated, but before they're resolved.
        public Action<float> PreStep { get; set; }
        public Action<float> MidStep { get; set; }
        public Action<float> PostStep { get; set; }

        /// <summary>
        /// Applies an impulse to the center of the body (changes linear velocity immediately).
        /// </summary>
        public void ApplyImpulse(JVector impulse)
        {
            Debug.Assert(!IsStatic, "Can't apply an impulse to a static or pseudo-static body.");
            Debug.Assert(!IsOnPlatform, "Can't apply an impulse to a body on a platform.");

            JVector.Multiply(ref impulse, inverseMass, out var temp);
            JVector.Add(ref linearVelocity, ref temp, out linearVelocity);
        }

        /// <summary>
        /// Applies an impulse to the given position (in world space). Changes both linear and angular velocity
        /// immediately.
        /// </summary>
        public void ApplyImpulse(JVector impulse, JVector p)
        {
            Debug.Assert(!IsStatic, "Can't apply an impulse to a static or pseudo-static body.");
            Debug.Assert(!IsOnPlatform, "Can't apply an impulse to a body on a platform.");

            // Linear velocity.
            JVector.Multiply(ref impulse, inverseMass, out var temp);
            JVector.Add(ref linearVelocity, ref temp, out linearVelocity);

            // Angular velocity.
            JVector.Subtract(ref p, ref position, out p);
            JVector.Cross(ref p, ref impulse, out temp);
            JVector.Transform(ref temp, ref invInertiaWorld, out temp);
            JVector.Add(ref angularVelocity, ref temp, out angularVelocity);
        }

        /// <summary>
        /// Adds a force to the body's center of mass (to be applied on the next step).
        /// </summary>
        public void AddForce(JVector force)
        {
            Debug.Assert(!IsStatic, "Can't apply a force to a static or pseudo-static body.");
            Debug.Assert(!IsOnPlatform, "Can't apply a force to a body on a platform.");

            JVector.Add(ref force, ref this.force, out this.force);
        }

        /// <summary>
        /// Adds a force to the given position (in world space). Accumulated forces are applied on the next step.
        /// </summary>
        public void AddForce(JVector force, JVector p)
        {
            Debug.Assert(!IsStatic, "Can't apply a force to a static or pseudo-static body.");
            Debug.Assert(!IsOnPlatform, "Can't apply a force to a body on a platform.");

            // Force.
            JVector.Add(ref this.force, ref force, out this.force);

            // Torque.
            JVector.Subtract(ref p, ref position, out p);
            JVector.Cross(ref p, ref force, out p);
            JVector.Add(ref p, ref torque, out torque);
        }

        /// <summary>
        /// Adds torque to the body (at the center of mass). Torque is applied on the next step.
        /// </summary>
        public void AddTorque(JVector torque)
        {
            Debug.Assert(!IsStatic, "Can't apply torque to a static or pseudo-static body.");
            Debug.Assert(!IsFixedVertical, "Can't apply torque to a fixed-vertical body.");
            Debug.Assert(!IsOnPlatform, "Can't apply torque to a body on a platform.");

            JVector.Add(ref torque, ref this.torque, out this.torque);
        }

        // TODO: Is this used?
        /// <summary>
        /// If called, the engine uses the given values for inertia and mass and ignores the shape's mass properties.
        /// </summary>
        public void SetMassProperties(JMatrix inertia, float mass, bool setAsInverseValues)
        {
            var isParticle = IsParticle;

            if (setAsInverseValues)
            {
                if (!isParticle)
                {
                    invInertia = inertia;
                    JMatrix.Inverse(ref inertia, out this.inertia);
                }

                inverseMass = mass;
            }
            else
            {
                if (!isParticle)
                {
                    this.inertia = inertia;
                    JMatrix.Inverse(ref inertia, out invInertia);
                }

                inverseMass = 1.0f / mass;
            }

            useShapeMassProperties = false;
            Update();
        }

        private void ShapeUpdated()
        {
            if (useShapeMassProperties)
            {
                SetMassProperties();
            }

            Update();
        }

        /// <summary>
        /// By calling this method, the shape's inertia and mass are used.
        /// </summary>
        private void SetMassProperties()
        {
            inertia = Shape.inertia;
            JMatrix.Inverse(ref inertia, out invInertia);
            inverseMass = 1.0f / Shape.mass;
            useShapeMassProperties = true;
        }

        public void SetTransform(JVector position, JMatrix orientation)
        {
            if (!IsSpawnPositionSet)
            {
                oldPosition = position;
                IsSpawnPositionSet = true;
            }
            else
            {
                oldPosition = this.position;
            }

            if (!IsSpawnOrientationSet)
            {
                oldOrientation = orientation;
                IsSpawnOrientationSet = true;
            }
            else
            {
                oldOrientation = this.orientation;
            }

            this.position = position;
            this.orientation = orientation;

            Update();
        }

	    public void SetTransform(JVector position, JMatrix orientation, float step)
	    {
            Debug.Assert(bodyType == RigidBodyTypes.PseudoStatic || IsOnPlatform, "This function should only be " +
                "called for pseudo-static bodies or bodies on platforms.");

	        SetTransform(position, orientation);

	        linearVelocity = JVector.Multiply(position - oldPosition, 1 / step);

            // TODO: Consider optimizing this (maybe this? https://stackoverflow.com/a/22172503).
            // See https://stackoverflow.com/a/22167097.
            var q1 = JQuaternion.CreateFromMatrix(orientation);
            var q2 = JQuaternion.CreateFromMatrix(oldOrientation);
            var diff = q2 * JQuaternion.Conjugate(q1);

            angularVelocity = diff.ComputeEulerAngles() * (1 / step);
        }

        public void SweptExpandBoundingBox(float timestep)
        {
            sweptDirection = linearVelocity * timestep;

            if (sweptDirection.X < 0.0f)
            {
                boundingBox.Min.X += sweptDirection.X;
            }
            else
            {
                boundingBox.Max.X += sweptDirection.X;
            }

            if (sweptDirection.Y < 0.0f)
            {
                boundingBox.Min.Y += sweptDirection.Y;
            }
            else
            {
                boundingBox.Max.Y += sweptDirection.Y;
            }

            if (sweptDirection.Z < 0.0f)
            {
                boundingBox.Min.Z += sweptDirection.Z;
            }
            else
            {
                boundingBox.Max.Z += sweptDirection.Z;
            }
        }

        /// <summary>
        /// Recalculates the axis aligned bounding box and the inertia
        /// values in world space.
        /// </summary>
        public virtual void Update()
        {
            if (IsParticle)
            {
                inertia = JMatrix.Zero;
                invInertia = invInertiaWorld = JMatrix.Zero;
                invOrientation = orientation = JMatrix.Identity;
                boundingBox = shape.boundingBox;

                JVector.Add(ref boundingBox.Min, ref position, out boundingBox.Min);
                JVector.Add(ref boundingBox.Max, ref position, out boundingBox.Max);

                angularVelocity.MakeZero();
            }
            else
            {
                JMatrix.Transpose(ref orientation, out invOrientation);
                Shape.GetBoundingBox(ref orientation, out boundingBox);
                JVector.Add(ref boundingBox.Min, ref position, out boundingBox.Min);
                JVector.Add(ref boundingBox.Max, ref position, out boundingBox.Max);

                if (!IsStatic)
                {
                    JMatrix.Multiply(ref invOrientation, ref invInertia, out invInertiaWorld);
                    JMatrix.Multiply(ref invInertiaWorld, ref orientation, out invInertiaWorld);
                }
            }
        }

        public bool Equals(RigidBody other)
        {
            return other.instance == instance;
        }

	    public override int GetHashCode()
	    {
	        return hashCode;
	    }

	    private int CalculateHash(int a)
	    {
	        a = (a ^ 61) ^ (a >> 16);
	        a += a << 3;
	        a ^= a >> 4;
	        a *= 0x27d4eb2d;
	        a ^= a >> 15;

	        return a;
	    }

        public int CompareTo(RigidBody other)
        {
	        if (other.instance < instance)
	        {
		        return -1;
	        }

            return other.instance > instance ? 1 : 0;
        }

	    private void ModifyFlag(RigidBodyFlags flag, bool value)
	    {
	        if (value)
	        {
	            flags |= flag;
	        }
	        else
	        {
	            flags &= ~flag;
	        }
	    }
	}
}
