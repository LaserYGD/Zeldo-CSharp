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

using Jitter.Dynamics;
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
    public class RigidBody : IBroadphaseEntity, IDebugDrawable, IEquatable<RigidBody>, IComparable<RigidBody>
	{
		[Flags]
		public enum DampingType
		{
			None = 0x00,
			Angular = 0x01,
			Linear = 0x02
		}

		private static int instanceCount;

		private int instance;
        private readonly int hashCode;

        private RigidBodyTypes bodyType;

		private Shape shape;
		private ShapeUpdatedHandler updatedHandler;

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

        private ReadOnlyHashset<Arbiter> readOnlyArbiters;
        private ReadOnlyHashset<Constraint> readOnlyConstraints;

        public RigidBody(Shape shape, RigidBodyTypes bodyType, RigidBodyFlags flags = RigidBodyFlags.IsActive)
            : this(shape, bodyType, new Material(), flags)
        {
        }

        public RigidBody(Shape shape, RigidBodyTypes bodyType, Material material,
            RigidBodyFlags flags = RigidBodyFlags.IsActive)
        {
            this.bodyType = bodyType;
            this.flags = flags;

            Damping = DampingType.Angular | DampingType.Linear;

            readOnlyArbiters = new ReadOnlyHashset<Arbiter>(arbiters);
            readOnlyConstraints = new ReadOnlyHashset<Constraint>(constraints);

            instance = Interlocked.Increment(ref instanceCount);
            hashCode = CalculateHash(instance);

            Shape = shape;
            orientation = JMatrix.Identity;

            if (!isParticle)
            {
                updatedHandler = ShapeUpdated;
                Shape.ShapeUpdated += updatedHandler;
                SetMassProperties();
            }
            else
            {
                this.inertia = JMatrix.Zero;
                this.invInertia = this.invInertiaWorld = JMatrix.Zero;
                this.invOrientation = this.orientation = JMatrix.Identity;
                inverseMass = 1.0f;
            }

            this.material = material;

            AllowDeactivation = true;
            EnableSpeculativeContacts = false;

            this.isParticle = isParticle;

            Update();
        }

        public override int GetHashCode()
        {
            return hashCode;
        }

        public ReadOnlyHashset<Arbiter> Arbiters => readOnlyArbiters;
	    public ReadOnlyHashset<Constraint> Constraints => readOnlyConstraints;

	    /// <summary>
        /// If set to false the body will never be deactived by the
        /// world.
        /// </summary>
        public bool AllowDeactivation { get; set; }
        public bool EnableSpeculativeContacts { get; set; }
        public bool RequiresResolution { get; set; }

        // TODO: Consider removing (if unused).
        /// <summary>
        /// If true, the body as no angular movement.
        /// </summary>
        public bool IsParticle
        {
            get => (flags & RigidBodyFlags.IsParticle) > 0;
            set
            {
                var isParticle = (flags & RigidBodyFlags.IsParticle) > 0;

                if (isParticle && !value)
                {
                    updatedHandler = ShapeUpdated;
                    Shape.ShapeUpdated += updatedHandler;
                    SetMassProperties();
                    flags &= ~RigidBodyFlags.IsParticle;
                }
                else if (!isParticle && value)
                {
                    inertia = JMatrix.Zero;
                    invInertia = invInertiaWorld = JMatrix.Zero;
                    invOrientation = orientation = JMatrix.Identity;
                    inverseMass = 1.0f;

                    Shape.ShapeUpdated -= updatedHandler;
                    flags |= RigidBodyFlags.IsParticle;
                }

                Update();
            }
        }

        /// <summary>
        /// The axis aligned bounding box of the body.
        /// </summary>
        public JBBox BoundingBox => boundingBox;

        private int CalculateHash(int a)
        {
            a = (a ^ 61) ^ (a >> 16);
            a += (a << 3);
            a ^= (a >> 4);
            a *= 0x27d4eb2d;
            a ^= (a >> 15);
            
            return a;
        }

        /// <summary>
        /// Gets the current collision island the body is in.
        /// </summary>
        public CollisionIsland CollisionIsland => island;

        /// <summary>
        /// Applies an impulse on the center of the body. Changing
        /// linear velocity.
        /// </summary>
        /// <param name="impulse">Impulse direction and magnitude.</param>
        public void ApplyImpulse(JVector impulse)
        {
			Debug.Assert(bodyType != RigidBodyTypes.Static, "Can't apply an impulse to a static body.");

	        JVector.Multiply(ref impulse, inverseMass, out var temp);
            JVector.Add(ref linearVelocity, ref temp, out linearVelocity);
        }

        /// <summary>
        /// Applies an impulse on the specific position. Changing linear
        /// and angular velocity.
        /// </summary>
        /// <param name="impulse">Impulse direction and magnitude.</param>
        /// <param name="relativePosition">The position where the impulse gets applied
        /// in Body coordinate frame.</param>
        public void ApplyImpulse(JVector impulse, JVector relativePosition)
		{
			Debug.Assert(bodyType != RigidBodyTypes.Static, "Can't apply an impulse to a static body.");
			Debug.Assert(!isFixedVertical, "Can't apply an impulse to a body that's fixed vertical.");

            // Linear velocity.
			JVector.Multiply(ref impulse, inverseMass, out var temp);
            JVector.Add(ref linearVelocity, ref temp, out linearVelocity);

            // Angular velocity.
			JVector.Cross(ref relativePosition, ref impulse, out temp);
			JVector.Transform(ref temp, ref invInertiaWorld, out temp);
			JVector.Add(ref angularVelocity, ref temp, out angularVelocity);
        }

        /// <summary>
        /// Adds a force to the center of the body. The force gets applied
        /// the next time <see cref="World.Step"/> is called. The 'impact'
        /// of the force depends on the time it is applied to a body - so
        /// the timestep influences the energy added to the body.
        /// </summary>
        /// <param name="force">The force to add next <see cref="World.Step"/>.</param>
        public void AddForce(JVector force)
        {
            JVector.Add(ref force, ref this.force, out this.force);
        }

        /// <summary>
        /// Adds a force to the center of the body. The force gets applied
        /// the next time <see cref="World.Step"/> is called. The 'impact'
        /// of the force depends on the time it is applied to a body - so
        /// the timestep influences the energy added to the body.
        /// </summary>
        /// <param name="force">The force to add next <see cref="World.Step"/>.</param>
        /// <param name="pos">The position where the force is applied.</param>
        public void AddForce(JVector force, JVector pos)
        {
            Debug.Assert(!isFixedVertical, "Can't apply forces to a body that's fixed vertical.");

            // Force.
            JVector.Add(ref this.force, ref force, out this.force);
            
            // Torque.
		    JVector.Subtract(ref pos, ref position, out pos);
		    JVector.Cross(ref pos, ref force, out pos);
		    JVector.Add(ref pos, ref torque, out torque);
        }

        /// <summary>
        /// Returns the torque which acts this timestep on the body.
        /// </summary>
        public JVector Torque => torque;

		/// <summary>
        /// Returns the force which acts this timestep on the body.
        /// </summary>
        public JVector Force
        {
	        get => force;
	        set => force = value;
        }

        /// <summary>
        /// Adds torque to the body. The torque gets applied
        /// the next time <see cref="World.Step"/> is called. The 'impact'
        /// of the torque depends on the time it is applied to a body - so
        /// the timestep influences the energy added to the body.
        /// </summary>
        /// <param name="torque">The torque to add next <see cref="World.Step"/>.</param>
        public void AddTorque(JVector torque)
        {
			Debug.Assert(!isFixedVertical, "Can't apply torque to a body that's fixed vertical.");

            JVector.Add(ref torque, ref this.torque, out this.torque);
        }

        /// <summary>
        /// By calling this method the shape inertia and mass is used.
        /// </summary>
        public void SetMassProperties()
        {
            this.inertia = Shape.inertia;
            JMatrix.Inverse(ref inertia, out invInertia);
            this.inverseMass = 1.0f / Shape.mass;
            useShapeMassProperties = true;
        }

        /// <summary>
        /// The engine used the given values for inertia and mass and ignores
        /// the shape mass properties.
        /// </summary>
        /// <param name="inertia">The inertia/inverse inertia of the untransformed object.</param>
        /// <param name="mass">The mass/inverse mass of the object.</param>
        /// <param name="setAsInverseValues">Sets the InverseInertia and the InverseMass
        /// to this values.</param>
        public void SetMassProperties(JMatrix inertia, float mass, bool setAsInverseValues)
        {
            if (setAsInverseValues)
            {
                if (!isParticle)
                {
                    this.invInertia = inertia;
                    JMatrix.Inverse(ref inertia, out this.inertia);
                }
                this.inverseMass = mass;
            }
            else
            {
                if (!isParticle)
                {
                    this.inertia = inertia;
                    JMatrix.Inverse(ref inertia, out this.invInertia);
                }
                this.inverseMass = 1.0f / mass;
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
            UpdateHullData();
        }

        public object Tag { get; set; }

        /// <summary>
        /// The shape the body is using.
        /// </summary>
        public Shape Shape 
        {
            get => shape;
	        set 
            {
                // deregister update event
	            if (shape != null)
	            {
		            shape.ShapeUpdated -= updatedHandler;
	            }

                // register new event
                shape = value; 
                shape.ShapeUpdated += ShapeUpdated; 
            } 
        }

        public DampingType Damping { get; set; }

        public Material Material
        {
	        get => material;
	        set => material = value;
        }

        /// <summary>
        /// The inertia currently used for this body.
        /// </summary>
        public JMatrix Inertia => inertia;

	    /// <summary>
        /// The inverse inertia currently used for this body.
        /// </summary>
        public JMatrix InverseInertia => invInertia;

	    /// <summary>
	    /// The inverse inertia tensor in world space.
	    /// </summary>
	    public JMatrix InverseInertiaWorld => invInertiaWorld;

		/// <summary>
		/// The velocity of the body.
		/// </summary>
		public JVector LinearVelocity
        {
            get => linearVelocity;
	        set 
            {
				Debug.Assert(bodyType != RigidBodyTypes.Static, "Can't set linear velocity on a static body.");
				Debug.Assert(!IsOnPlatform, "Can't set linear velocity directly on a body that's on a platform (use " +
                    "SetTransform with a timestep instead).");

	            linearVelocity = value;
            }
        }
		
        /// <summary>
        /// The angular velocity of the body.
        /// </summary>
        public JVector AngularVelocity
        {
            get => angularVelocity;
	        set
            {
				Debug.Assert(bodyType != RigidBodyTypes.Static, "Can't set angular velocity on a static body.");
				Debug.Assert(!isFixedVertical, "Can't set angular velocity on a body that's fixed-vertical.");
				Debug.Assert(!IsOnPlatform, "Can't set angular velocity directly on a body that's on a platform (use" +
                    "SetTransform with a timestep instead).");

	            angularVelocity = value;
            }
        }

        /// <summary>
        /// The current position of the body.
        /// </summary>
        public JVector Position
        {
            get => position;
            set
            {
                Debug.Assert(bodyType != RigidBodyTypes.Static, "Can't set position on a static body." +
                    "directly on a pseudo-static body (use SetPosition instead).");
                Debug.Assert(!(IsOnPlatform && isSpawnTransformSet), "Can't set position directly on a body with " +
                    "the IgnoreVelocity flag set (use SetPosition instead).");

                if (!isSpawnTransformSet)
                {
                    oldPosition = value;
                    isSpawnTransformSet = true;
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
                // Pseudo-static bodies can have their position set once directly (on spawn), but afterwards should
                // use SetTransform instead (same logic applies to bodies on platforms).
                Debug.Assert(!(bodyType == RigidBodyTypes.PseudoStatic && isSpawnPositionSet), "Can't set position " +
                    "directly on a pseudo-static body (use SetPosition instead).");
                Debug.Assert(!(IsOnPlatform && isSpawnPositionSet), "Can't set position directly on a body with " +
                    "the IgnoreVelocity flag set (use SetPosition instead).");

                orientation = value;
		        Update();
	        }
        }

        /// <summary>
        /// If set to false, velocities are set to zero and the body gets immediately frozen.
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
                    flags |= RigidBodyFlags.IsActive;
                }
                else if (isActive && !value)
                {
                    inactiveTime = float.PositiveInfinity;
                    angularVelocity.MakeZero();
                    linearVelocity.MakeZero();
                    flags &= ~RigidBodyFlags.IsActive;
                }
            }
        }

        public bool IsStatic => (int)bodyType >= (int)RigidBodyTypes.PseudoStatic;
        public bool IsStaticOrInactive => IsStatic || !IsActive;

        public bool IsAffectedByGravity
        {
	        get => (flags & RigidBodyFlags.IsAffectedByGravity) > 0;
	        set
            {
                if (value)
                {
                    flags |= RigidBodyFlags.IsAffectedByGravity;
                }
                else
                {
                    flags &= ~RigidBodyFlags.IsAffectedByGravity;
                }
            }
        }

	    public bool IsFixedVertical
	    {
		    get => (flags & RigidBodyFlags.IsFixedVertical) > 0;
		    set
		    {
		        if (value)
                {
                    flags |= RigidBodyFlags.IsFixedVertical;
                    // If a body is fixed-vertical, it's assumed its orientation will be set manually (via control
                    // code). As such, angular velocity is disallowed.
		            angularVelocity.MakeZero();
                }
                else
                {
                    flags &= ~RigidBodyFlags.IsFixedVertical;
                }
		    }
	    }

        public bool IsOnPlatform { get; set; }

		public RigidBodyTypes BodyType
	    {
		    get => bodyType;

			// TODO: If bodies can change type on the fly, additional logic might be needed here.
			set => bodyType = value;

            // The code below was previously in the IsStatic setter.
            /*
            if (value && bodyType != RigidBodyTypes.Static)
            {
	            island?.islandManager.MakeBodyStatic(this);

	            angularVelocity.MakeZero();
                linearVelocity.MakeZero();
            }
             */
        }

        /// <summary>
        /// Setting the mass automatically scales the inertia.
        /// To set the mass indepedently from the mass use SetMassProperties.
        /// </summary>
        public float Mass
        {
            get => 1.0f / inverseMass;
	        set 
            {
				Debug.Assert(value > 0, "Mass must be positive.");

                // scale inertia
                if (!isParticle)
                {
                    JMatrix.Multiply(ref Shape.inertia, value / Shape.mass, out inertia);
                    JMatrix.Inverse(ref inertia, out invInertia);
                }

                inverseMass = 1.0f / value;
            }
        }

	    public int BroadphaseTag { get; set; }

        // TODO: Consider adding normal to this callback (to avoid having to recompute it later).
		public Func<RigidBody, JVector[], bool> ShouldGenerateContact { get; set; }

        // Mid-step occurs after contacts are generated, but before they're resolved.
        public Action<float> PreStep { get; set; }
        public Action<float> MidStep { get; set; }
        public Action<float> PostStep { get; set; }

        public void SetTransform(JVector position, JMatrix orientation)
        {
            this.position = position;
            this.orientation = orientation;

            Update();
        }

        // TODO: Compute fake angular velocity as well (and rename this to something like SetTransformWithFakeVelocities).
	    public void SetTransform(JVector position, JMatrix orientation, float step)
	    {
            Debug.Assert(bodyType == RigidBodyTypes.PseudoStatic || IsOnPlatform, "This function should only be " +
                "called for pseudo-static bodies or bodies on platforms.");

	        oldPosition = this.position;
	        this.position = position;

	        LinearVelocity = JVector.Multiply(position - oldPosition, 1 / step);
            Update();
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
            if (isParticle)
            {
                inertia = JMatrix.Zero;
                invInertia = this.invInertiaWorld = JMatrix.Zero;
                invOrientation = this.orientation = JMatrix.Identity;
                boundingBox = shape.boundingBox;
                JVector.Add(ref boundingBox.Min, ref position, out boundingBox.Min);
                JVector.Add(ref boundingBox.Max, ref position, out boundingBox.Max);

                angularVelocity.MakeZero();
            }
            else
            {
                // Given: Orientation, Inertia
                JMatrix.Transpose(ref orientation, out invOrientation);
                Shape.GetBoundingBox(ref orientation, out boundingBox);
                JVector.Add(ref boundingBox.Min, ref position, out boundingBox.Min);
                JVector.Add(ref boundingBox.Max, ref position, out boundingBox.Max);

				// CUSTOM: Modified to use body type.
                if (bodyType != RigidBodyTypes.Static)
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

        public int CompareTo(RigidBody other)
        {
	        if (other.instance < instance)
	        {
		        return -1;
	        }

            return other.instance > instance ? 1 : 0;
        }
    }
}
