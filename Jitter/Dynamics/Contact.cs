﻿/* Copyright (C) <2009-2011> <Thorben Linneweber, Jitter Physics>
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
using Jitter.Collision;
using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Dynamics.Constraints;

namespace Jitter.Dynamics
{
    public class ContactSettings
    {
        public enum MaterialCoefficientMixingType { TakeMaximum, TakeMinimum, UseAverage }

        internal float maximumBias = 10.0f;
        internal float bias = 0.25f;
        internal float minVelocity = 0.001f;
        internal float allowedPenetration = 0.01f;
        internal float breakThreshold = 0.01f;

        internal MaterialCoefficientMixingType materialMode = MaterialCoefficientMixingType.UseAverage;

        public float MaximumBias
        {
            get => maximumBias;
            set => maximumBias = value;
        }

        public float BiasFactor
        {
            get => bias;
            set => bias = value;
        }

        public float MinimumVelocity
        {
            get => minVelocity;
            set => minVelocity = value;
        }

        public float AllowedPenetration
        {
            get => allowedPenetration;
            set => allowedPenetration = value;
        }

        public float BreakThreshold
        {
            get => breakThreshold;
            set => breakThreshold = value;
        }

        public MaterialCoefficientMixingType MaterialCoefficientMixing
        {
            get => materialMode;
            set => materialMode = value;
        }
    }

    /// <summary>
    /// </summary>
    public class Contact : IConstraint
	{
		private float staticFriction;
		private float dynamicFriction;
		private float restitution;
		private float friction;
		private float massNormal;
		private float massTangent;
		private float restitutionBias;
		private float lostSpeculativeBounce;
		private float speculativeVelocity;

		// CUSTOM: Added these two booleans to support kinematic bodies.
		private bool isBody1Movable;
		private bool isBody2Movable;
		private bool isNewContact;
		private bool body1IsMassPoint;
		private bool body2IsMassPoint;

		private ContactSettings settings;

		internal RigidBody body1;
		internal RigidBody body2;

		internal JVector normal;
		internal JVector tangent;
		internal JVector realRelPos1;
		internal JVector realRelPos2;
		internal JVector relativePos1;
		internal JVector relativePos2;
		internal JVector p1;
		internal JVector p2;

        // CUSTOM: Added to support wall movement.
        internal JVector[] triangle;

        internal float accumulatedNormalImpulse;
        internal float accumulatedTangentImpulse;
        internal float penetration;
        internal float initialPen;

        /// <summary>
        /// A contact resource pool.
        /// </summary>
        public static readonly ResourcePool<Contact> Pool = new ResourcePool<Contact>();

        private float lastTimeStep = float.PositiveInfinity;

        #region Properties
        public float Restitution
        {
            get => restitution;
	        set => restitution = value;
        }

        public float StaticFriction
        {
            get => staticFriction;
	        set => staticFriction = value;
        }

        public float DynamicFriction
        {
            get => dynamicFriction;
	        set => dynamicFriction = value;
        }

        /// <summary>
        /// The first body involved in the contact.
        /// </summary>
        public RigidBody Body1 => body1;

	    /// <summary>
        /// The second body involved in the contact.
        /// </summary>
        public RigidBody Body2 => body2;

	    /// <summary>
        /// The penetration of the contact.
        /// </summary>
        public float Penetration => penetration;

	    /// <summary>
        /// The collision position in world space of body1.
        /// </summary>
        public JVector Position1 => p1;

	    /// <summary>
        /// The collision position in world space of body2.
        /// </summary>
        public JVector Position2 => p2;

	    /// <summary>
        /// The contact tangent.
        /// </summary>
        public JVector Tangent => tangent;

	    /// <summary>
        /// The contact normal.
        /// </summary>
        public JVector Normal => normal;

        // This will be null for non-triangle contacts.
        public JVector[] Triangle => triangle;
	    #endregion

        /// <summary>
        /// Calculates relative velocity of body contact points on the bodies.
        /// </summary>
        /// <param name="relVel">The relative velocity of body contact points on the bodies.</param>
        public JVector CalculateRelativeVelocity()
        {
            var x = (body2.angularVelocity.Y * relativePos2.Z) - (body2.angularVelocity.Z * relativePos2.Y) + body2.linearVelocity.X;
            var y = (body2.angularVelocity.Z * relativePos2.X) - (body2.angularVelocity.X * relativePos2.Z) + body2.linearVelocity.Y;
            var z = (body2.angularVelocity.X * relativePos2.Y) - (body2.angularVelocity.Y * relativePos2.X) + body2.linearVelocity.Z;

            JVector relVel;
            relVel.X = x - (body1.angularVelocity.Y * relativePos1.Z) + (body1.angularVelocity.Z * relativePos1.Y) - body1.linearVelocity.X;
            relVel.Y = y - (body1.angularVelocity.Z * relativePos1.X) + (body1.angularVelocity.X * relativePos1.Z) - body1.linearVelocity.Y;
            relVel.Z = z - (body1.angularVelocity.X * relativePos1.Y) + (body1.angularVelocity.Y * relativePos1.X) - body1.linearVelocity.Z;

            return relVel;
        }
        
        /// <summary>
        /// Solves the contact iteratively.
        /// </summary>
        public void Iterate()
        {
			//body1.linearVelocity = JVector.Zero;
			//body2.linearVelocity = JVector.Zero;
			//return;

            // TODO: Is this needed? If neither body is movable, the contact should have never been generated.
			// CUSTOM: Updated to account for kinematic body types.
            if (!(isBody1Movable || isBody2Movable))
            {
                return;
            }

            var dvx = body2.linearVelocity.X - body1.linearVelocity.X;
            var dvy = body2.linearVelocity.Y - body1.linearVelocity.Y;
            var dvz = body2.linearVelocity.Z - body1.linearVelocity.Z;

            if (!body1IsMassPoint)
            {
                dvx = dvx - (body1.angularVelocity.Y * relativePos1.Z) + (body1.angularVelocity.Z * relativePos1.Y);
                dvy = dvy - (body1.angularVelocity.Z * relativePos1.X) + (body1.angularVelocity.X * relativePos1.Z);
                dvz = dvz - (body1.angularVelocity.X * relativePos1.Y) + (body1.angularVelocity.Y * relativePos1.X);
            }

            if (!body2IsMassPoint)
            {
                dvx = dvx + (body2.angularVelocity.Y * relativePos2.Z) - (body2.angularVelocity.Z * relativePos2.Y);
                dvy = dvy + (body2.angularVelocity.Z * relativePos2.X) - (body2.angularVelocity.X * relativePos2.Z);
                dvz = dvz + (body2.angularVelocity.X * relativePos2.Y) - (body2.angularVelocity.Y * relativePos2.X);
            }

            // This gets us some performance.
            if (dvx * dvx + dvy * dvy + dvz * dvz < settings.minVelocity * settings.minVelocity)
            {
                return;
            }

            float vn = normal.X * dvx + normal.Y * dvy + normal.Z * dvz;
            float normalImpulse = massNormal * (-vn + restitutionBias + speculativeVelocity);
            float oldNormalImpulse = accumulatedNormalImpulse;

            accumulatedNormalImpulse = oldNormalImpulse + normalImpulse;

            if (accumulatedNormalImpulse < 0)
            {
                accumulatedNormalImpulse = 0;
            }

            normalImpulse = accumulatedNormalImpulse - oldNormalImpulse;

            float vt = dvx * tangent.X + dvy * tangent.Y + dvz * tangent.Z;
            float maxTangentImpulse = friction * accumulatedNormalImpulse;
            float tangentImpulse = massTangent * -vt;
            float oldTangentImpulse = accumulatedTangentImpulse;

            accumulatedTangentImpulse = JitterUtilities.Clamp(oldTangentImpulse + tangentImpulse, -maxTangentImpulse,
                maxTangentImpulse);
            tangentImpulse = accumulatedTangentImpulse - oldTangentImpulse;

            // Apply contact impulse.
            JVector impulse;
            impulse.X = normal.X * normalImpulse + tangent.X * tangentImpulse;
            impulse.Y = normal.Y * normalImpulse + tangent.Y * tangentImpulse;
            impulse.Z = normal.Z * normalImpulse + tangent.Z * tangentImpulse;

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody1Movable)
            {
                body1.linearVelocity.X -= (impulse.X * body1.inverseMass);
                body1.linearVelocity.Y -= (impulse.Y * body1.inverseMass);
                body1.linearVelocity.Z -= (impulse.Z * body1.inverseMass);

                // Bodies that are fixed vertical are assumed to be actors, which means their movement is largely
                // controlled manually (via controllers). As such, the rotation effects of contacts can be ignored in
                // those cases.
                if (!(body1IsMassPoint || body1.IsFixedVertical || body1.IsManuallyControlled))
                {
                    var num0 = relativePos1.Y * impulse.Z - relativePos1.Z * impulse.Y;
                    var num1 = relativePos1.Z * impulse.X - relativePos1.X * impulse.Z;
                    var num2 = relativePos1.X * impulse.Y - relativePos1.Y * impulse.X;

                    float num3 =
                        (num0 * body1.invInertiaWorld.M11) +
                        (num1 * body1.invInertiaWorld.M21) +
                        (num2 * body1.invInertiaWorld.M31);
                    float num4 =
                        (num0 * body1.invInertiaWorld.M12) +
                        (num1 * body1.invInertiaWorld.M22) +
                        (num2 * body1.invInertiaWorld.M32);
                    float num5 =
                        (num0 * body1.invInertiaWorld.M13) +
                        (num1 * body1.invInertiaWorld.M23) +
                        (num2 * body1.invInertiaWorld.M33);

                    body1.angularVelocity.X -= num3;
                    body1.angularVelocity.Y -= num4;
                    body1.angularVelocity.Z -= num5;
                }

                body1.RequiresResolution = true;
            }

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody2Movable)
            {
                body2.linearVelocity.X += (impulse.X * body2.inverseMass);
                body2.linearVelocity.Y += (impulse.Y * body2.inverseMass);
                body2.linearVelocity.Z += (impulse.Z * body2.inverseMass);

                if (!(body2IsMassPoint || body2.IsFixedVertical || body2.IsManuallyControlled))
                {
                    var num0 = relativePos2.Y * impulse.Z - relativePos2.Z * impulse.Y;
                    var num1 = relativePos2.Z * impulse.X - relativePos2.X * impulse.Z;
                    var num2 = relativePos2.X * impulse.Y - relativePos2.Y * impulse.X;

                    float num3 =
                        (num0 * body2.invInertiaWorld.M11) +
                        (num1 * body2.invInertiaWorld.M21) +
                        (num2 * body2.invInertiaWorld.M31);
                    float num4 =
                        (num0 * body2.invInertiaWorld.M12) +
                        (num1 * body2.invInertiaWorld.M22) +
                        (num2 * body2.invInertiaWorld.M32);
                    float num5 =
                        (num0 * body2.invInertiaWorld.M13) +
                        (num1 * body2.invInertiaWorld.M23) +
                        (num2 * body2.invInertiaWorld.M33);

                    body2.angularVelocity.X += num3;
                    body2.angularVelocity.Y += num4;
                    body2.angularVelocity.Z += num5;
                }

                body2.RequiresResolution = true;
            }
        }

        // TODO: Are these needed?
        public float AppliedNormalImpulse => accumulatedNormalImpulse;
	    public float AppliedTangentImpulse => accumulatedTangentImpulse;

	    /// <summary>
        /// The points in wolrd space gets recalculated by transforming the
        /// local coordinates. Also new penetration depth is estimated.
        /// </summary>
        public void UpdatePosition()
        {
            if (body1IsMassPoint)
            {
                JVector.Add(ref realRelPos1, ref body1.position, out p1);
            }
            else
            {
                JVector.Transform(ref realRelPos1, ref body1.orientation, out p1);
                JVector.Add(ref p1, ref body1.position, out p1);
            }

            if (body2IsMassPoint)
            {
                JVector.Add(ref realRelPos2, ref body2.position, out p2);
            }
            else
            {
                JVector.Transform(ref realRelPos2, ref body2.orientation, out p2);
                JVector.Add(ref p2, ref body2.position, out p2);
            }


            JVector.Subtract(ref p1, ref p2, out var dist);
            penetration = JVector.Dot(ref dist, ref normal);
        }

        /// <summary>
        /// An impulse is applied an both contact points.
        /// </summary>
        /// <param name="impulse">The impulse to apply.</param>
        public void ApplyImpulse(ref JVector impulse)
        {
			#region INLINE - HighFrequency
			//JVector temp;

			// CUSTOM: Updated to account for kinematic body types.
			if (isBody1Movable)
            {
                body1.linearVelocity.X -= (impulse.X * body1.inverseMass);
                body1.linearVelocity.Y -= (impulse.Y * body1.inverseMass);
                body1.linearVelocity.Z -= (impulse.Z * body1.inverseMass);

                float num0, num1, num2;
                num0 = relativePos1.Y * impulse.Z - relativePos1.Z * impulse.Y;
                num1 = relativePos1.Z * impulse.X - relativePos1.X * impulse.Z;
                num2 = relativePos1.X * impulse.Y - relativePos1.Y * impulse.X;

                float num3 =
                    (((num0 * body1.invInertiaWorld.M11) +
                    (num1 * body1.invInertiaWorld.M21)) +
                    (num2 * body1.invInertiaWorld.M31));
                float num4 =
                    (((num0 * body1.invInertiaWorld.M12) +
                    (num1 * body1.invInertiaWorld.M22)) +
                    (num2 * body1.invInertiaWorld.M32));
                float num5 =
                    (((num0 * body1.invInertiaWorld.M13) +
                    (num1 * body1.invInertiaWorld.M23)) +
                    (num2 * body1.invInertiaWorld.M33));

                body1.angularVelocity.X -= num3;
                body1.angularVelocity.Y -= num4;
                body1.angularVelocity.Z -= num5;
            }

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody2Movable)
            {

                body2.linearVelocity.X += (impulse.X * body2.inverseMass);
                body2.linearVelocity.Y += (impulse.Y * body2.inverseMass);
                body2.linearVelocity.Z += (impulse.Z * body2.inverseMass);

                float num0, num1, num2;
                num0 = relativePos2.Y * impulse.Z - relativePos2.Z * impulse.Y;
                num1 = relativePos2.Z * impulse.X - relativePos2.X * impulse.Z;
                num2 = relativePos2.X * impulse.Y - relativePos2.Y * impulse.X;

                float num3 =
                    (((num0 * body2.invInertiaWorld.M11) +
                    (num1 * body2.invInertiaWorld.M21)) +
                    (num2 * body2.invInertiaWorld.M31));
                float num4 =
                    (((num0 * body2.invInertiaWorld.M12) +
                    (num1 * body2.invInertiaWorld.M22)) +
                    (num2 * body2.invInertiaWorld.M32));
                float num5 =
                    (((num0 * body2.invInertiaWorld.M13) +
                    (num1 * body2.invInertiaWorld.M23)) +
                    (num2 * body2.invInertiaWorld.M33));

                body2.angularVelocity.X += num3;
                body2.angularVelocity.Y += num4;
                body2.angularVelocity.Z += num5;
            }


            #endregion
        }

        public void ApplyImpulse(JVector impulse)
        {
			#region INLINE - HighFrequency
			//JVector temp;

			// CUSTOM: Updated to account for kinematic body types.
			if (isBody1Movable)
            {
                body1.linearVelocity.X -= (impulse.X * body1.inverseMass);
                body1.linearVelocity.Y -= (impulse.Y * body1.inverseMass);
                body1.linearVelocity.Z -= (impulse.Z * body1.inverseMass);

                float num0, num1, num2;
                num0 = relativePos1.Y * impulse.Z - relativePos1.Z * impulse.Y;
                num1 = relativePos1.Z * impulse.X - relativePos1.X * impulse.Z;
                num2 = relativePos1.X * impulse.Y - relativePos1.Y * impulse.X;

                float num3 =
                    (((num0 * body1.invInertiaWorld.M11) +
                    (num1 * body1.invInertiaWorld.M21)) +
                    (num2 * body1.invInertiaWorld.M31));
                float num4 =
                    (((num0 * body1.invInertiaWorld.M12) +
                    (num1 * body1.invInertiaWorld.M22)) +
                    (num2 * body1.invInertiaWorld.M32));
                float num5 =
                    (((num0 * body1.invInertiaWorld.M13) +
                    (num1 * body1.invInertiaWorld.M23)) +
                    (num2 * body1.invInertiaWorld.M33));

                body1.angularVelocity.X -= num3;
                body1.angularVelocity.Y -= num4;
                body1.angularVelocity.Z -= num5;
            }

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody2Movable)
            {

                body2.linearVelocity.X += (impulse.X * body2.inverseMass);
                body2.linearVelocity.Y += (impulse.Y * body2.inverseMass);
                body2.linearVelocity.Z += (impulse.Z * body2.inverseMass);

                float num0, num1, num2;
                num0 = relativePos2.Y * impulse.Z - relativePos2.Z * impulse.Y;
                num1 = relativePos2.Z * impulse.X - relativePos2.X * impulse.Z;
                num2 = relativePos2.X * impulse.Y - relativePos2.Y * impulse.X;

                float num3 =
                    (((num0 * body2.invInertiaWorld.M11) +
                    (num1 * body2.invInertiaWorld.M21)) +
                    (num2 * body2.invInertiaWorld.M31));
                float num4 =
                    (((num0 * body2.invInertiaWorld.M12) +
                    (num1 * body2.invInertiaWorld.M22)) +
                    (num2 * body2.invInertiaWorld.M32));
                float num5 =
                    (((num0 * body2.invInertiaWorld.M13) +
                    (num1 * body2.invInertiaWorld.M23)) +
                    (num2 * body2.invInertiaWorld.M33));

                body2.angularVelocity.X += num3;
                body2.angularVelocity.Y += num4;
                body2.angularVelocity.Z += num5;
            }


            #endregion
        }

        /// <summary>
        /// PrepareForIteration has to be called before <see cref="Iterate"/>.
        /// </summary>
        /// <param name="timestep">The timestep of the simulation.</param>
        public void PrepareForIteration(float timestep)
        {
            var dvx = (body2.angularVelocity.Y * relativePos2.Z) - (body2.angularVelocity.Z * relativePos2.Y) + body2.linearVelocity.X;
            var dvy = (body2.angularVelocity.Z * relativePos2.X) - (body2.angularVelocity.X * relativePos2.Z) + body2.linearVelocity.Y;
            var dvz = (body2.angularVelocity.X * relativePos2.Y) - (body2.angularVelocity.Y * relativePos2.X) + body2.linearVelocity.Z;

            dvx = dvx - (body1.angularVelocity.Y * relativePos1.Z) + (body1.angularVelocity.Z * relativePos1.Y) - body1.linearVelocity.X;
            dvy = dvy - (body1.angularVelocity.Z * relativePos1.X) + (body1.angularVelocity.X * relativePos1.Z) - body1.linearVelocity.Y;
            dvz = dvz - (body1.angularVelocity.X * relativePos1.Y) + (body1.angularVelocity.Y * relativePos1.X) - body1.linearVelocity.Z;

            float kNormal = 0.0f;

            JVector rantra = JVector.Zero;

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody1Movable)
            {
                kNormal += body1.inverseMass;

                if (!body1IsMassPoint)
                {
                    // JVector.Cross(ref relativePos1, ref normal, out rantra);
                    rantra.X = (relativePos1.Y * normal.Z) - (relativePos1.Z * normal.Y);
                    rantra.Y = (relativePos1.Z * normal.X) - (relativePos1.X * normal.Z);
                    rantra.Z = (relativePos1.X * normal.Y) - (relativePos1.Y * normal.X);

                    // JVector.Transform(ref rantra, ref body1.invInertiaWorld, out rantra);
                    float num0 = (rantra.X * body1.invInertiaWorld.M11) + (rantra.Y * body1.invInertiaWorld.M21) + (rantra.Z * body1.invInertiaWorld.M31);
                    float num1 = (rantra.X * body1.invInertiaWorld.M12) + (rantra.Y * body1.invInertiaWorld.M22) + (rantra.Z * body1.invInertiaWorld.M32);
                    float num2 = (rantra.X * body1.invInertiaWorld.M13) + (rantra.Y * body1.invInertiaWorld.M23) + (rantra.Z * body1.invInertiaWorld.M33);

                    rantra.X = num0; rantra.Y = num1; rantra.Z = num2;

                    //JVector.Cross(ref rantra, ref relativePos1, out rantra);
                    num0 = (rantra.Y * relativePos1.Z) - (rantra.Z * relativePos1.Y);
                    num1 = (rantra.Z * relativePos1.X) - (rantra.X * relativePos1.Z);
                    num2 = (rantra.X * relativePos1.Y) - (rantra.Y * relativePos1.X);

                    rantra.X = num0; rantra.Y = num1; rantra.Z = num2;
                }
            }

            JVector rbntrb = JVector.Zero;

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody2Movable)
            {
                kNormal += body2.inverseMass;

                if (!body2IsMassPoint)
                {
                    // JVector.Cross(ref relativePos1, ref normal, out rantra);
                    rbntrb.X = (relativePos2.Y * normal.Z) - (relativePos2.Z * normal.Y);
                    rbntrb.Y = (relativePos2.Z * normal.X) - (relativePos2.X * normal.Z);
                    rbntrb.Z = (relativePos2.X * normal.Y) - (relativePos2.Y * normal.X);

                    // JVector.Transform(ref rantra, ref body1.invInertiaWorld, out rantra);
                    float num0 = (rbntrb.X * body2.invInertiaWorld.M11) + (rbntrb.Y * body2.invInertiaWorld.M21) + (rbntrb.Z * body2.invInertiaWorld.M31);
                    float num1 = (rbntrb.X * body2.invInertiaWorld.M12) + (rbntrb.Y * body2.invInertiaWorld.M22) + (rbntrb.Z * body2.invInertiaWorld.M32);
                    float num2 = (rbntrb.X * body2.invInertiaWorld.M13) + (rbntrb.Y * body2.invInertiaWorld.M23) + (rbntrb.Z * body2.invInertiaWorld.M33);

                    rbntrb.X = num0; rbntrb.Y = num1; rbntrb.Z = num2;

                    //JVector.Cross(ref rantra, ref relativePos1, out rantra);
                    num0 = (rbntrb.Y * relativePos2.Z) - (rbntrb.Z * relativePos2.Y);
                    num1 = (rbntrb.Z * relativePos2.X) - (rbntrb.X * relativePos2.Z);
                    num2 = (rbntrb.X * relativePos2.Y) - (rbntrb.Y * relativePos2.X);

                    rbntrb.X = num0; rbntrb.Y = num1; rbntrb.Z = num2;
                }
            }

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody1Movable)
            {
                kNormal += rantra.X * normal.X + rantra.Y * normal.Y + rantra.Z * normal.Z;
            }

            if (isBody2Movable)
            {
                kNormal += rbntrb.X * normal.X + rbntrb.Y * normal.Y + rbntrb.Z * normal.Z;
            }

            massNormal = 1.0f / kNormal;

            float num = dvx * normal.X + dvy * normal.Y + dvz * normal.Z;

            tangent.X = dvx - normal.X * num;
            tangent.Y = dvy - normal.Y * num;
            tangent.Z = dvz - normal.Z * num;

            num = tangent.X * tangent.X + tangent.Y * tangent.Y + tangent.Z * tangent.Z;

            if (num != 0)
            {
                num = (float)Math.Sqrt(num);
                tangent.X /= num;
                tangent.Y /= num;
                tangent.Z /= num;
            }

            float kTangent = 0;

	        // CUSTOM: Updated to account for kinematic body types.
            if (!isBody1Movable)
            {
                rantra.MakeZero();
            }
            else
            {
                kTangent += body1.inverseMass;
  
                if (!body1IsMassPoint)
                {
                    // JVector.Cross(ref relativePos1, ref normal, out rantra);
                    rantra.X = (relativePos1.Y * tangent.Z) - (relativePos1.Z * tangent.Y);
                    rantra.Y = (relativePos1.Z * tangent.X) - (relativePos1.X * tangent.Z);
                    rantra.Z = (relativePos1.X * tangent.Y) - (relativePos1.Y * tangent.X);

                    // JVector.Transform(ref rantra, ref body1.invInertiaWorld, out rantra);
                    float num0 = (rantra.X * body1.invInertiaWorld.M11) + (rantra.Y * body1.invInertiaWorld.M21) + (rantra.Z * body1.invInertiaWorld.M31);
                    float num1 = (rantra.X * body1.invInertiaWorld.M12) + (rantra.Y * body1.invInertiaWorld.M22) + (rantra.Z * body1.invInertiaWorld.M32);
                    float num2 = (rantra.X * body1.invInertiaWorld.M13) + (rantra.Y * body1.invInertiaWorld.M23) + (rantra.Z * body1.invInertiaWorld.M33);

                    rantra.X = num0; rantra.Y = num1; rantra.Z = num2;

                    //JVector.Cross(ref rantra, ref relativePos1, out rantra);
                    num0 = (rantra.Y * relativePos1.Z) - (rantra.Z * relativePos1.Y);
                    num1 = (rantra.Z * relativePos1.X) - (rantra.X * relativePos1.Z);
                    num2 = (rantra.X * relativePos1.Y) - (rantra.Y * relativePos1.X);

                    rantra.X = num0; rantra.Y = num1; rantra.Z = num2;
                }
            }

	        // CUSTOM: Updated to account for kinematic body types.
            if (!isBody2Movable)
            {
                rbntrb.MakeZero();
            }
            else
            {
                kTangent += body2.inverseMass;

                if (!body2IsMassPoint)
                {
                    // JVector.Cross(ref relativePos1, ref normal, out rantra);
                    rbntrb.X = (relativePos2.Y * tangent.Z) - (relativePos2.Z * tangent.Y);
                    rbntrb.Y = (relativePos2.Z * tangent.X) - (relativePos2.X * tangent.Z);
                    rbntrb.Z = (relativePos2.X * tangent.Y) - (relativePos2.Y * tangent.X);

                    // JVector.Transform(ref rantra, ref body1.invInertiaWorld, out rantra);
                    float num0 = (rbntrb.X * body2.invInertiaWorld.M11) + (rbntrb.Y * body2.invInertiaWorld.M21) + (rbntrb.Z * body2.invInertiaWorld.M31);
                    float num1 = (rbntrb.X * body2.invInertiaWorld.M12) + (rbntrb.Y * body2.invInertiaWorld.M22) + (rbntrb.Z * body2.invInertiaWorld.M32);
                    float num2 = (rbntrb.X * body2.invInertiaWorld.M13) + (rbntrb.Y * body2.invInertiaWorld.M23) + (rbntrb.Z * body2.invInertiaWorld.M33);

                    rbntrb.X = num0; rbntrb.Y = num1; rbntrb.Z = num2;

                    //JVector.Cross(ref rantra, ref relativePos1, out rantra);
                    num0 = (rbntrb.Y * relativePos2.Z) - (rbntrb.Z * relativePos2.Y);
                    num1 = (rbntrb.Z * relativePos2.X) - (rbntrb.X * relativePos2.Z);
                    num2 = (rbntrb.X * relativePos2.Y) - (rbntrb.Y * relativePos2.X);

                    rbntrb.X = num0; rbntrb.Y = num1; rbntrb.Z = num2;
                }
            }

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody1Movable)
            {
                kTangent += JVector.Dot(ref rantra, ref tangent);
            }

            if (isBody2Movable)
            {
                kTangent += JVector.Dot(ref rbntrb, ref tangent);
            }

            massTangent = 1.0f / kTangent;
            restitutionBias = lostSpeculativeBounce;
            speculativeVelocity = 0.0f;

            float relNormalVel = normal.X * dvx + normal.Y * dvy + normal.Z * dvz; //JVector.Dot(ref normal, ref dv);

            if (Penetration > settings.allowedPenetration)
            {
                restitutionBias = settings.bias * (1.0f / timestep) * JMath.Max(0.0f, Penetration - settings.allowedPenetration);
                restitutionBias = JMath.Clamp(restitutionBias, 0.0f, settings.maximumBias);
              //  body1IsMassPoint = body2IsMassPoint = false;
            }

            float timeStepRatio = timestep / lastTimeStep;
            accumulatedNormalImpulse *= timeStepRatio;
            accumulatedTangentImpulse *= timeStepRatio;
            
            // Static/Dynamic friction
            float relTangentVel = -(tangent.X * dvx + tangent.Y * dvy + tangent.Z * dvz);
            float tangentImpulse = massTangent * relTangentVel;
            float maxTangentImpulse = -staticFriction * accumulatedNormalImpulse;

            friction = tangentImpulse < maxTangentImpulse ? dynamicFriction : staticFriction;

            JVector impulse;

            // Simultaneos solving and restitution is simply not possible
            // so fake it a bit by just applying restitution impulse when there
            // is a new contact.
            if (relNormalVel < -1.0f && isNewContact)
            {
                restitutionBias = Math.Max(-restitution * relNormalVel, restitutionBias);
            }

            // Speculative Contacts!
            // if the penetration is negative (which means the bodies are not already in contact, but they will
            // be in the future) we store the current bounce bias in the variable 'lostSpeculativeBounce'
            // and apply it the next frame, when the speculative contact was already solved.
            if (penetration < -settings.allowedPenetration)
            {
                speculativeVelocity = penetration / timestep;
                lostSpeculativeBounce = restitutionBias;
                restitutionBias = 0.0f;
            }
            else
            {
                lostSpeculativeBounce = 0.0f;
            }

            impulse.X = normal.X * accumulatedNormalImpulse + tangent.X * accumulatedTangentImpulse;
            impulse.Y = normal.Y * accumulatedNormalImpulse + tangent.Y * accumulatedTangentImpulse;
            impulse.Z = normal.Z * accumulatedNormalImpulse + tangent.Z * accumulatedTangentImpulse;

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody1Movable)
            {
                body1.linearVelocity.X -= (impulse.X * body1.inverseMass);
                body1.linearVelocity.Y -= (impulse.Y * body1.inverseMass);
                body1.linearVelocity.Z -= (impulse.Z * body1.inverseMass);

                if (!body1IsMassPoint)
                {
                    var num0 = relativePos1.Y * impulse.Z - relativePos1.Z * impulse.Y;
                    var num1 = relativePos1.Z * impulse.X - relativePos1.X * impulse.Z;
                    var num2 = relativePos1.X * impulse.Y - relativePos1.Y * impulse.X;

                    float num3 =
                        (num0 * body1.invInertiaWorld.M11) +
                        (num1 * body1.invInertiaWorld.M21) +
                        (num2 * body1.invInertiaWorld.M31);
                    float num4 =
                        (num0 * body1.invInertiaWorld.M12) +
                        (num1 * body1.invInertiaWorld.M22) +
                        (num2 * body1.invInertiaWorld.M32);
                    float num5 =
                        (num0 * body1.invInertiaWorld.M13) +
                        (num1 * body1.invInertiaWorld.M23) +
                        (num2 * body1.invInertiaWorld.M33);

                    body1.angularVelocity.X -= num3;
                    body1.angularVelocity.Y -= num4;
                    body1.angularVelocity.Z -= num5;
                }
            }

	        // CUSTOM: Updated to account for kinematic body types.
			if (isBody2Movable)
            {
                body2.linearVelocity.X += (impulse.X * body2.inverseMass);
                body2.linearVelocity.Y += (impulse.Y * body2.inverseMass);
                body2.linearVelocity.Z += (impulse.Z * body2.inverseMass);

                if (!body2IsMassPoint)
                {
                    var num0 = relativePos2.Y * impulse.Z - relativePos2.Z * impulse.Y;
                    var num1 = relativePos2.Z * impulse.X - relativePos2.X * impulse.Z;
                    var num2 = relativePos2.X * impulse.Y - relativePos2.Y * impulse.X;

                    float num3 =
                        (num0 * body2.invInertiaWorld.M11) +
                        (num1 * body2.invInertiaWorld.M21) +
                        (num2 * body2.invInertiaWorld.M31);
                    float num4 =
                        (num0 * body2.invInertiaWorld.M12) +
                        (num1 * body2.invInertiaWorld.M22) +
                        (num2 * body2.invInertiaWorld.M32);
                    float num5 =
                        (num0 * body2.invInertiaWorld.M13) +
                        (num1 * body2.invInertiaWorld.M23) +
                        (num2 * body2.invInertiaWorld.M33);

                    body2.angularVelocity.X += num3;
                    body2.angularVelocity.Y += num4;
                    body2.angularVelocity.Z += num5;
                }
            }

            lastTimeStep = timestep;
            isNewContact = false;
        }

		// CUSTOM: Commented out this function (since it caused errors and didn't seem to be used).
		/*
        public void TreatBodyAsStatic(RigidBodyIndex index)
        {
            if (index == RigidBodyIndex.RigidBody1) treatBody1AsStatic = true;
            else treatBody2AsStatic = true;
        }
		*/

        /// <summary>
        /// Initializes a contact.
        /// </summary>
        /// <param name="body1">The first body.</param>
        /// <param name="body2">The second body.</param>
        /// <param name="point1">The collision point in worldspace</param>
        /// <param name="point2">The collision point in worldspace</param>
        /// <param name="n">The normal pointing to body2.</param>
        /// <param name="penetration">The estimated penetration depth.</param>
        public void Initialize(RigidBody body1, RigidBody body2, ref JVector point1, ref JVector point2, ref JVector n,
            JVector[] triangle, float penetration, bool newContact, ContactSettings settings)
        {
            this.body1 = body1;
            this.body2 = body2;
            this.triangle = triangle;

            normal = n;
            normal.Normalize();

            p1 = point1;
            p2 = point2;

            isNewContact = newContact;

            JVector.Subtract(ref p1, ref body1.position, out relativePos1);
            JVector.Subtract(ref p2, ref body2.position, out relativePos2);
            JVector.Transform(ref relativePos1, ref body1.invOrientation, out realRelPos1);
            JVector.Transform(ref relativePos2, ref body2.invOrientation, out realRelPos2);

            this.initialPen = penetration;
            this.penetration = penetration;

            body1IsMassPoint = body1.IsParticle;
            body2IsMassPoint = body2.IsParticle;

            // Material Properties
            if (newContact)
            {
				// CUSTOM: Modified to account for kinematic bodies.
	            var type1 = body1.BodyType;
	            var type2 = body2.BodyType;

                // Kinematic bodies cannot be moved by dynamic bodies. Otherwise, all collisions occur.
		        isBody1Movable = type1 <= type2;
		        isBody2Movable = type2 <= type1;

                accumulatedNormalImpulse = 0.0f;
                accumulatedTangentImpulse = 0.0f;

                lostSpeculativeBounce = 0.0f;

                switch (settings.MaterialCoefficientMixing)
                {
                    case ContactSettings.MaterialCoefficientMixingType.TakeMaximum:
                        staticFriction = JMath.Max(body1.material.staticFriction, body2.material.staticFriction);
                        dynamicFriction = JMath.Max(body1.material.kineticFriction, body2.material.kineticFriction);
                        restitution = JMath.Max(body1.material.restitution, body2.material.restitution);

                        break;

                    case ContactSettings.MaterialCoefficientMixingType.TakeMinimum:
                        staticFriction = JMath.Min(body1.material.staticFriction, body2.material.staticFriction);
                        dynamicFriction = JMath.Min(body1.material.kineticFriction, body2.material.kineticFriction);
                        restitution = JMath.Min(body1.material.restitution, body2.material.restitution);

                        break;

                    case ContactSettings.MaterialCoefficientMixingType.UseAverage:
                        staticFriction = (body1.material.staticFriction + body2.material.staticFriction) / 2.0f;
                        dynamicFriction = (body1.material.kineticFriction + body2.material.kineticFriction) / 2.0f;
                        restitution = (body1.material.restitution + body2.material.restitution) / 2.0f;

                        break;
                }
            }

            this.settings = settings;
        }
    }
}
