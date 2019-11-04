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

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

using Jitter.Dynamics;
using Jitter.LinearMath;
using Jitter.Collision.Shapes;
using Jitter.Collision;
using Jitter.Dynamics.Constraints;
using Jitter.DataStructures;
#endregion

namespace Jitter
{

    /// <summary>
    /// This class brings 'dynamics' and 'collisions' together. It handles
    /// all bodies and constraints.
    /// </summary>
    public class World
    {
        public delegate bool ContactCreationHandler(RigidBody body1, RigidBody body2, JVector p1, JVector p2,
            JVector normal, JVector[] triangle, float penetration);
        public delegate void WorldStep(float timestep);

        public class WorldEvents
        {
            // Post&Prestep
            public event WorldStep PreStep;
            public event WorldStep PostStep;

            // Add&Remove
            public event Action<RigidBody> AddedRigidBody;
            public event Action<RigidBody> RemovedRigidBody;
            public event Action<Constraint> AddedConstraint;
            public event Action<Constraint> RemovedConstraint;
            public event Action<SoftBody> AddedSoftBody;
            public event Action<SoftBody> RemovedSoftBody;

            // Collision
            public event Action<RigidBody, RigidBody> BodiesBeginCollide;
            public event Action<RigidBody, RigidBody> BodiesEndCollide;

            // CUSTOM: The return value indicates whether the contact should be kept.
            public event ContactCreationHandler ContactCreated;

            // Deactivation
            public event Action<RigidBody> DeactivatedBody;
            public event Action<RigidBody> ActivatedBody;

            internal WorldEvents() { }

            #region Raise Events

            internal void RaiseWorldPreStep(float timestep)
            {
                PreStep?.Invoke(timestep);
            }

            internal void RaiseWorldPostStep(float timestep)
            {
                PostStep?.Invoke(timestep);
            }

            internal void RaiseAddedRigidBody(RigidBody body)
            {
                if (AddedRigidBody != null) AddedRigidBody(body);
            }

            internal void RaiseRemovedRigidBody(RigidBody body)
            {
                if (RemovedRigidBody != null) RemovedRigidBody(body);
            }

            internal void RaiseAddedConstraint(Constraint constraint)
            {
                if (AddedConstraint != null) AddedConstraint(constraint);
            }

            internal void RaiseRemovedConstraint(Constraint constraint)
            {
                if (RemovedConstraint != null) RemovedConstraint(constraint);
            }

            internal void RaiseAddedSoftBody(SoftBody body)
            {
                if (AddedSoftBody != null) AddedSoftBody(body);
            }

            internal void RaiseRemovedSoftBody(SoftBody body)
            {
                if (RemovedSoftBody != null) RemovedSoftBody(body);
            }

            internal void RaiseBodiesBeginCollide(RigidBody body1, RigidBody body2)
            {
                if (BodiesBeginCollide != null) BodiesBeginCollide(body1, body2);
            }

            internal void RaiseBodiesEndCollide(RigidBody body1, RigidBody body2)
            {
                if (BodiesEndCollide != null) BodiesEndCollide(body1, body2);
            }

            internal void RaiseActivatedBody(RigidBody body)
            {
                if (ActivatedBody != null) ActivatedBody(body);
            }

            internal void RaiseDeactivatedBody(RigidBody body)
            {
                if (DeactivatedBody != null) DeactivatedBody(body);
            }

            internal bool RaiseContactCreated(RigidBody body1, RigidBody body2, JVector p1, JVector p2, JVector normal,
                JVector[] triangle, float penetration)
            {
                return ContactCreated?.Invoke(body1, body2, p1, p2, normal, triangle, penetration) ?? true;
            }

            #endregion
        }

        private ContactSettings contactSettings = new ContactSettings();

        private float inactiveAngularThresholdSq = 0.1f;
        private float inactiveLinearThresholdSq = 0.1f;
        private float deactivationTime = 2f;

        private float angularDamping = 0.85f;
        private float linearDamping = 0.85f;
        private float accumulatedTime;
        private float timestep;

        private int contactIterations = 10;
        private int smallIterations = 4;

        private IslandManager islands = new IslandManager();

        private HashSet<RigidBody> rigidBodies = new HashSet<RigidBody>();
        private HashSet<Constraint> constraints = new HashSet<Constraint>();
        private HashSet<SoftBody> softbodies = new HashSet<SoftBody>();

        public ReadOnlyHashset<RigidBody> RigidBodies { get; }
        public ReadOnlyHashset<Constraint> Constraints { get; }
        public ReadOnlyHashset<SoftBody> SoftBodies { get; }

        private WorldEvents events = new WorldEvents();
        
        // Used by arbiters to trigger the contact creation event.
        public WorldEvents Events => events;

        private ThreadManager threadManager = ThreadManager.Instance;

        /// <summary>
        /// Holds a list of <see cref="Arbiter"/>. All currently
        /// active arbiter in the <see cref="World"/> are stored in this map.
        /// </summary>
        public ArbiterMap ArbiterMap => arbiterMap;

        private ArbiterMap arbiterMap;

        private Queue<Arbiter> removedArbiterQueue = new Queue<Arbiter>();
        private Queue<Arbiter> addedArbiterQueue = new Queue<Arbiter>();

        private JVector gravity = new JVector(0, -9.81f, 0);

        public ContactSettings ContactSettings => contactSettings;

        /// <summary>
        /// Gets a read only collection of the <see cref="Jitter.Collision.CollisionIsland"/> objects managed by
        /// this class.
        /// </summary>
        public ReadOnlyCollection<CollisionIsland> Islands => islands;

        private Action<object> arbiterCallback;
        private Action<object> integrateCallback;

        private CollisionDetectedHandler collisionDetectionHandler;

        /// <summary>
        /// Create a new instance of the <see cref="World"/> class.
        /// </summary>
        /// <param name="collision">The collisionSystem which is used to detect
        /// collisions. See for example: <see cref="CollisionSystemSAP"/>
        /// or <see cref="CollisionSystemBrute"/>.
        /// </param>
        public World(CollisionSystem system)
        {
            Debug.Assert(system != null, "Collision system can't be null.");

            Arbiter.World = this;

            arbiterCallback = ArbiterCallback;
            integrateCallback = IntegrateCallback;

            // Create the readonly wrappers
            this.RigidBodies = new ReadOnlyHashset<RigidBody>(rigidBodies);
            this.Constraints = new ReadOnlyHashset<Constraint>(constraints);
            this.SoftBodies = new ReadOnlyHashset<SoftBody>(softbodies);

            this.CollisionSystem = system;

            collisionDetectionHandler = new CollisionDetectedHandler(CollisionDetected);

            this.CollisionSystem.CollisionDetected += collisionDetectionHandler;

            this.arbiterMap = new ArbiterMap();

            AllowDeactivation = true;
        }

        public void AddBody(SoftBody body)
        {
            if (body == null) throw new ArgumentNullException("body", "body can't be null.");
            if (softbodies.Contains(body)) throw new ArgumentException("The body was already added to the world.", "body");

            this.softbodies.Add(body);
            this.CollisionSystem.AddEntity(body);

            events.RaiseAddedSoftBody(body);

            foreach (Constraint constraint in body.EdgeSprings)
                AddConstraint(constraint);

            foreach (SoftBody.MassPoint massPoint in body.VertexBodies)
            {
                events.RaiseAddedRigidBody(massPoint);
                rigidBodies.Add(massPoint);
            }
        }

        public bool RemoveBody(SoftBody body)
        {
            if (!this.softbodies.Remove(body)) return false;

            this.CollisionSystem.RemoveEntity(body);

            events.RaiseRemovedSoftBody(body);

            foreach (Constraint constraint in body.EdgeSprings)
                RemoveConstraint(constraint);

            foreach (SoftBody.MassPoint massPoint in body.VertexBodies)
                Remove(massPoint, true);

            return true;
        }

        /// <summary>
        /// Gets the <see cref="CollisionSystem"/> used
        /// to detect collisions.
        /// </summary>
        public CollisionSystem CollisionSystem { set; get; }

        /// <summary>
        /// In Jitter many objects get added to stacks after they were used.
        /// If a new object is needed the old object gets removed from the stack
        /// and is reused. This saves some time and also garbage collections.
        /// Calling this method removes all cached objects from all
        /// stacks.
        /// </summary>
        public void ResetResourcePools()
        {
            IslandManager.Pool.ResetResourcePool();
            Arbiter.Pool.ResetResourcePool();
            Contact.Pool.ResetResourcePool();
        }

        /// <summary>
        /// Removes all objects from the world and removes all memory cached objects.
        /// </summary>
        public void Clear()
        {
            // remove bodies from collision system
            foreach (RigidBody body in rigidBodies)
            {
                CollisionSystem.RemoveEntity(body);

                if (body.island != null)
                {
                    body.island.ClearLists();
                    body.island = null;
                }

                body.connections.Clear();
                body.arbiters.Clear();
                body.constraints.Clear();

                events.RaiseRemovedRigidBody(body);
            }

            foreach (SoftBody body in softbodies)
            {
                CollisionSystem.RemoveEntity(body);
            }

            // remove bodies from the world
            rigidBodies.Clear();

            // remove constraints
            foreach (Constraint constraint in constraints)
            {
                events.RaiseRemovedConstraint(constraint);
            }
            constraints.Clear();

            softbodies.Clear();

            // remove all islands
            islands.RemoveAll();

            // delete the arbiters
            arbiterMap.Clear();

            ResetResourcePools();
        }

        /// <summary>
        /// Gets or sets the gravity in this <see cref="World"/>. The default gravity
        /// is (0,-9.81,0)
        /// </summary>
        public JVector Gravity 
        { 
            get => gravity;
            set => gravity = value;
        }

        /// <summary>
        /// Global sets or gets if a body is able to be temporarily deactivated by the engine to
        /// safe computation time. Use <see cref="SetInactivityThreshold"/> to set parameters
        /// of the deactivation process.
        /// </summary>
        public bool AllowDeactivation { get; set; }

        /// <summary>
        /// Every computation <see cref="Step"/> the angular and linear velocity 
        /// of a <see cref="RigidBody"/> gets multiplied by this value.
        /// </summary>
        /// <param name="angularDamping">The factor multiplied with the angular velocity.
        /// The default value is 0.85.</param>
        /// <param name="linearDamping">The factor multiplied with the linear velocity.
        /// The default value is 0.85</param>
        public void SetDampingFactors(float angularDamping, float linearDamping)
        {
            if (angularDamping < 0.0f || angularDamping > 1.0f)
                throw new ArgumentException("Angular damping factor has to be between 0.0 and 1.0", "angularDamping");

            if (linearDamping < 0.0f || linearDamping > 1.0f)
                throw new ArgumentException("Linear damping factor has to be between 0.0 and 1.0", "linearDamping");

            this.angularDamping = angularDamping;
            this.linearDamping = linearDamping;
        }

        /// <summary>
        /// Sets parameters for the <see cref="RigidBody"/> deactivation process.
        /// If the bodies angular velocity is less than the angular velocity threshold
        /// and its linear velocity is lower then the linear velocity threshold for a 
        /// specific time the body gets deactivated. A body can be reactivated by setting
        /// <see cref="RigidBody.IsActive"/> to true. A body gets also automatically
        /// reactivated if another moving object hits it or the <see cref="CollisionIsland"/>
        /// the object is in gets activated.
        /// </summary>
        /// <param name="angularVelocity">The threshold value for the angular velocity. The default value
        /// is 0.1.</param>
        /// <param name="linearVelocity">The threshold value for the linear velocity. The default value
        /// is 0.1</param>
        /// <param name="time">The threshold value for the time in seconds. The default value is 2.</param>
        public void SetInactivityThreshold(float angularVelocity, float linearVelocity, float time)
        {
            if (angularVelocity < 0.0f) throw new ArgumentException("Angular velocity threshold has to " +
                 "be larger than zero", "angularVelocity");

            if (linearVelocity < 0.0f) throw new ArgumentException("Linear velocity threshold has to " +
                "be larger than zero", "linearVelocity");

            if (time < 0.0f) throw new ArgumentException("Deactivation time threshold has to " +
                "be larger than zero", "time");

            this.inactiveAngularThresholdSq = angularVelocity * angularVelocity;
            this.inactiveLinearThresholdSq = linearVelocity * linearVelocity;
            this.deactivationTime = time;
        }

        /// <summary>
        /// Jitter uses an iterativ approach to solve collisions and contacts. You can set the number of
        /// iterations Jitter should do. In general the more iterations the more stable a simulation gets
        /// but also costs computation time.
        /// </summary>
        /// <param name="iterations">The number of contact iterations. Default value 10.</param>
        /// <param name="smallIterations">The number of contact iteration used for smaller (two and three constraint) systems. Default value 4.</param>
        /// <remarks>The number of iterations for collision and contact should be between 3 - 30.
        /// More iterations means more stability and also a longer calculation time.</remarks>
        public void SetIterations(int iterations, int smallIterations)
        {
            if (iterations < 1) throw new ArgumentException("The number of collision " +
                 "iterations has to be larger than zero", "iterations");

            if (smallIterations < 1) throw new ArgumentException("The number of collision " +
                "iterations has to be larger than zero", "smallIterations");

            this.contactIterations = iterations;
            this.smallIterations = smallIterations;
        }

        // TODO: Should this return a boolean?
        /// <summary>
        /// Removes a <see cref="RigidBody"/> from the world.
        /// </summary>
        /// <param name="body">The body which should be removed.</param>
        /// <returns>Returns false if the body could not be removed from the world.</returns>
        public bool Remove(RigidBody body)
        {
            return Remove(body, false);
        }

        // TODO: Should this return a boolean?
        public void Remove(IEnumerable<RigidBody> bodies)
        {
            foreach (var b in bodies)
            {
                Remove(b);
            }
        }

        private bool Remove(RigidBody body, bool removeMassPoints)
        {
            // Its very important to clean up, after removing a body
            if (!removeMassPoints && body.IsParticle) return false;

            // remove the body from the world list
            if (!rigidBodies.Remove(body)) return false;

            // Remove all connected constraints and arbiters
            foreach (Arbiter arbiter in body.arbiters)
            {
                arbiterMap.Remove(arbiter);
                events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
            }

            foreach (Constraint constraint in body.constraints)
            {
                constraints.Remove(constraint);
                events.RaiseRemovedConstraint(constraint);
            }

            // remove the body from the collision system
            CollisionSystem.RemoveEntity(body);

            // remove the body from the island manager
            islands.RemoveBody(body);

            events.RaiseRemovedRigidBody(body);

            return true;
        }

        /// <summary>
        /// Adds a <see cref="RigidBody"/> to the world.
        /// </summary>
        /// <param name="body">The body which should be added.</param>
        public void AddBody(RigidBody body)
        {
            if (body == null) throw new ArgumentNullException("body", "body can't be null.");
            if (rigidBodies.Contains(body)) throw new ArgumentException("The body was already added to the world.", "body");

            events.RaiseAddedRigidBody(body);

            this.CollisionSystem.AddEntity(body);

            rigidBodies.Add(body);
        }

        /// <summary>
        /// Add a <see cref="Constraint"/> to the world. Fast, O(1).
        /// </summary>
        /// <param name="constraint">The constraint which should be added.</param>
        /// <returns>True if the constraint was successfully removed.</returns>
        public bool RemoveConstraint(Constraint constraint)
        {
            if (!constraints.Remove(constraint)) return false;
            events.RaiseRemovedConstraint(constraint);

            islands.ConstraintRemoved(constraint);

            return true;
        }

        /// <summary>
        /// Add a <see cref="Constraint"/> to the world.
        /// </summary>
        /// <param name="constraint">The constraint which should be removed.</param>
        public void AddConstraint(Constraint constraint)
        {
            if (constraints.Contains(constraint))
                throw new ArgumentException("The constraint was already added to the world.", "constraint");

            constraints.Add(constraint);

            islands.ConstraintCreated(constraint);

            events.RaiseAddedConstraint(constraint);
        }

        private float currentLinearDampFactor = 1.0f;
        private float currentAngularDampFactor = 1.0f;

#if(!WINDOWS_PHONE)
        Stopwatch sw = new Stopwatch();

        public enum DebugType
        {
            CollisionDetect, BuildIslands, HandleArbiter, UpdateContacts,
            PreStep, DeactivateBodies, IntegrateForces, Integrate, PostStep, ClothUpdate, Num
        }

        /// <summary>
        /// Time in ms for every part of the <see cref="Step"/> method.
        /// </summary>
        /// <example>int time = DebugTimes[(int)DebugType.CollisionDetect] gives
        /// the amount of time spent on collision detection during the last <see cref="Step"/>.
        /// </example>
        private double[] debugTimes = new double[(int)DebugType.Num];
        public double[] DebugTimes { get { return debugTimes; } }
#endif

        /// <summary>
        /// Integrates the whole world a timestep further in time.
        /// </summary>
        /// <param name="timestep">The timestep in seconds. 
        /// It should be small as possible to keep the simulation stable.
        /// The physics simulation shouldn't run slower than 60fps.
        /// (timestep=1/60).</param>
        /// <param name="multithread">If true the engine uses several threads to
        /// integrate the world. This is faster on multicore CPUs.</param>
        public void Step(float timestep, bool multithread)
        {
            this.timestep = timestep;

            if (timestep == 0)
            {
                return;
            }

            Debug.Assert(timestep > 0, "Timestep can't be negative.");

            currentAngularDampFactor = (float)Math.Pow(angularDamping, timestep);
            currentLinearDampFactor = (float)Math.Pow(linearDamping, timestep);

            // Pre-step (world first, then bodies).
            sw.Reset();
            sw.Start();
            events.RaiseWorldPreStep(timestep);

            foreach (RigidBody body in rigidBodies)
            {
                body.PreStep?.Invoke(timestep);
            }

            sw.Stop();
            debugTimes[(int)DebugType.PreStep] = sw.Elapsed.TotalMilliseconds;

            // Update existing contacts.
            sw.Reset();
            sw.Start();
            UpdateContacts();
            sw.Stop();
            debugTimes[(int)DebugType.UpdateContacts] = sw.Elapsed.TotalMilliseconds;

            // Remove arbiters (based on dead contacts).
            sw.Reset();
            sw.Start();

            while (removedArbiterQueue.Count > 0)
            {
                islands.ArbiterRemoved(removedArbiterQueue.Dequeue());
            }

            sw.Stop();

            var ms = sw.Elapsed.TotalMilliseconds;

            // Update soft bodies.
            sw.Reset();
            sw.Start();

            foreach (SoftBody body in softbodies)
            {
                body.Update(timestep);
                body.DoSelfCollision(collisionDetectionHandler);
            }

            // Integrate forces (changes linear and angular velocity on relevant bodies).
            sw.Reset();
            sw.Start();
            IntegrateForces();
            sw.Stop();
            debugTimes[(int)DebugType.IntegrateForces] = sw.Elapsed.TotalMilliseconds;

            sw.Stop();
            debugTimes[(int)DebugType.ClothUpdate] = sw.Elapsed.TotalMilliseconds;

            // Integrate bodies (by applying linear and angular velocity). Also updates bounding boxes.
            sw.Reset();
            sw.Start();
            Integrate(multithread);
            sw.Stop();
            debugTimes[(int)DebugType.Integrate] = sw.Elapsed.TotalMilliseconds;

            // Detect collisions.
            sw.Reset();
            sw.Start();
            CollisionSystem.Detect(multithread);
            sw.Stop();
            debugTimes[(int)DebugType.CollisionDetect] = sw.Elapsed.TotalMilliseconds;

            // Add arbiters (that were detected).
            sw.Reset();
            sw.Start();

            while (addedArbiterQueue.Count > 0)
            {
                islands.ArbiterCreated(addedArbiterQueue.Dequeue());
            }

            sw.Stop();
            debugTimes[(int)DebugType.BuildIslands] = sw.Elapsed.TotalMilliseconds + ms;

            // Mid-step (allows entities to manually process contacts as needed).
            foreach (RigidBody body in rigidBodies)
            {
                body.MidStep?.Invoke(timestep);
            }

            // Check deactivation (i.e. sleep bodies as applicable).
            sw.Reset();
            sw.Start();
            CheckDeactivation();
            sw.Stop();
            debugTimes[(int)DebugType.DeactivateBodies] = sw.Elapsed.TotalMilliseconds;

            // Store velocities (linear and angular) and reset resolution flags.
            foreach (RigidBody body in RigidBodies)
            {
                body.storedLinear = body.linearVelocity;
                body.storedAngular = body.angularVelocity;
                body.RequiresResolution = false;
            }

            // Iterate contacts (modifies linear and angular velocity on relevant bodies, but does NOT change position
            // directly).
            sw.Reset();
            sw.Start();
            HandleArbiter(contactIterations, multithread);
            sw.Stop();
            debugTimes[(int)DebugType.HandleArbiter] = sw.Elapsed.TotalMilliseconds;

            // Resolve collisions.
            foreach (RigidBody body in RigidBodies)
            {
                if (!body.RequiresResolution)
                {
                    continue;
                }

                JVector effectiveLinear = body.linearVelocity - body.storedLinear;
                JVector effectiveAngular = body.angularVelocity - body.storedAngular;

                // Apply linear velocity.
                if (!body.IsManuallyControlled)
                {
                    body.Position += effectiveLinear * timestep;
                }

                if (!body.IsParticle && !body.IsFixedVertical && !body.IsManuallyControlled)
                {
                    //exponential map
                    JVector axis;
                    float angle = effectiveAngular.Length();

                    if (angle < 0.001f)
                    {
                        // use Taylor's expansions of sync function
                        // axis = body.angularVelocity * (0.5f * timestep - (timestep * timestep * timestep) * (0.020833333333f) * angle * angle);
                        JVector.Multiply(ref effectiveAngular, (0.5f * timestep - (timestep * timestep * timestep) * (0.020833333333f) * angle * angle), out axis);
                    }
                    else
                    {
                        // sync(fAngle) = sin(c*fAngle)/t
                        JVector.Multiply(ref effectiveAngular, ((float)Math.Sin(0.5f * angle * timestep) / angle), out axis);
                    }

                    JQuaternion dorn = new JQuaternion(axis.X, axis.Y, axis.Z, (float)Math.Cos(angle * timestep * 0.5f));
                    JQuaternion.CreateFromMatrix(ref body.orientation, out var ornA);
                    JQuaternion.Multiply(ref dorn, ref ornA, out dorn);

                    dorn.Normalize();

                    JMatrix.CreateFromQuaternion(ref dorn, out body.orientation);
                }
            }

            // Check deactivation (i.e. sleep bodies as applicable).
            /*
            sw.Reset();
            sw.Start();
            CheckDeactivation();
            sw.Stop();
            debugTimes[(int)DebugType.DeactivateBodies] = sw.Elapsed.TotalMilliseconds;
            */

            // Post-step (bodies first, then the world).
            sw.Reset();
            sw.Start();

            foreach (RigidBody body in rigidBodies)
            {
                body.PostStep?.Invoke(timestep);
            }

            events.RaiseWorldPostStep(timestep);
            sw.Stop();
            debugTimes[(int)DebugType.PostStep] = sw.Elapsed.TotalMilliseconds;
        }

        /// <summary>
        /// Integrates the whole world several fixed timestep further in time.
        /// </summary>
        /// <param name="totalTime">The time to integrate.</param>
        /// <param name="timestep">The timestep in seconds. 
        /// It should be small as possible to keep the simulation stable.
        /// The physics simulation shouldn't run slower than 60fps.
        /// (timestep=1/60).</param>
        /// <param name="multithread">If true the engine uses several threads to
        /// integrate the world. This is faster on multicore CPUs.</param>
        /// <param name="maxSteps">The maximum number of substeps. After that Jitter gives up
        /// to keep up with the given totalTime.</param>
        public void Step(float totalTime, bool multithread, float timestep, int maxSteps)
        {
            int counter = 0;
            accumulatedTime += totalTime;

            while (accumulatedTime > timestep)
            {
                Step(timestep, multithread);

                accumulatedTime -= timestep;
                counter++;

                if (counter > maxSteps)
                {
                    // okay, okay... we can't keep up
                    accumulatedTime = 0.0f;
                    break;
                }
            }
        }

        private void UpdateArbiterContacts(Arbiter arbiter)
        {
            if (arbiter.contactList.Count == 0)
            {
                lock (removedArbiterStack) { removedArbiterStack.Push(arbiter); }
                return;
            }

            for (int i = arbiter.contactList.Count - 1; i >= 0; i--)
            {
                Contact c = arbiter.contactList[i];
                c.UpdatePosition();

                if (c.penetration < -contactSettings.breakThreshold)
                {
                    Contact.Pool.GiveBack(c);
                    arbiter.contactList.RemoveAt(i);

                    continue;
                }

                JVector.Subtract(ref c.p1, ref c.p2, out var diff);
                float distance = JVector.Dot(ref diff, ref c.normal);

                diff = diff - distance * c.normal;
                distance = diff.LengthSquared();

                // hack (multiplication by factor 100) in the
                // following line.
                if (distance > contactSettings.breakThreshold * contactSettings.breakThreshold * 100)
                {
                    Contact.Pool.GiveBack(c);
                    arbiter.contactList.RemoveAt(i);
                }
            }
        }

        private Stack<Arbiter> removedArbiterStack = new Stack<Arbiter>();

        private void UpdateContacts()
        {
            foreach (Arbiter arbiter in arbiterMap.Arbiters)
            {
                UpdateArbiterContacts(arbiter);
            }

            while (removedArbiterStack.Count > 0)
            {
                Arbiter arbiter = removedArbiterStack.Pop();
                Arbiter.Pool.GiveBack(arbiter);
                arbiterMap.Remove(arbiter);

                removedArbiterQueue.Enqueue(arbiter);
                events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
            }

        }

        private void ArbiterCallback(object obj)
        {
            CollisionIsland island = obj as CollisionIsland;

            var thisIterations = island.Bodies.Count + island.Constraints.Count > 3
                ? contactIterations
                : smallIterations;

            for (int i = -1; i < thisIterations; i++)
            {
                // Contact and Collision
                foreach (Arbiter arbiter in island.arbiter)
                {
                    int contactCount = arbiter.contactList.Count;

                    for (int e = 0; e < contactCount; e++)
                    {
                        if (i == -1)
                        {
                            arbiter.contactList[e].PrepareForIteration(timestep);
                        }
                        else
                        {
                            arbiter.contactList[e].Iterate();
                        }
                    }
                }

                //  Constraints
                foreach (Constraint c in island.constraints)
                {
                    if (c.body1 != null && !c.body1.IsActive && c.body2 != null && !c.body2.IsActive)
                    {
                        continue;
                    }

                    if (i == -1)
                    {
                        c.PrepareForIteration(timestep);
                    }
                    else
                    {
                        c.Iterate();
                    }
                }
            }
        }

        private void HandleArbiter(int iterations, bool multiThreaded)
        {
            if (multiThreaded)
            {
                foreach (var island in islands)
                {
                    if (island.IsActive())
                    {
                        threadManager.AddTask(arbiterCallback, island);
                    }
                }

                threadManager.Execute();
            }
            else
            {
                foreach (var island in islands)
                {
                    if (island.IsActive())
                    {
                        arbiterCallback(island);
                    }
                }
            }
        }

        private void IntegrateForces()
        {
            foreach (RigidBody body in rigidBodies)
            {
                if (!body.IsStatic && body.IsActive)
                {
                    JVector temp;

                    if (body.IsAffectedByGravity)
                    {
                        JVector.Multiply(ref gravity, timestep, out temp);
                        JVector.Add(ref body.linearVelocity, ref temp, out body.linearVelocity);
                    }

                    // Fixed-vertical bodies can't have forces applied (gravity still applies though).
                    if (body.IsFixedVertical)
                    {
                        continue;
                    }

                    // Modify linear velocity.
                    JVector.Multiply(ref body.force, body.inverseMass * timestep, out temp);
                    JVector.Add(ref temp, ref body.linearVelocity, out body.linearVelocity);

                    // Modify angular velocity.
                    if (!body.IsParticle && !body.IsManuallyControlled)
                    {
                        JVector.Multiply(ref body.torque, timestep, out temp);
                        JVector.Transform(ref temp, ref body.invInertiaWorld, out temp);
                        JVector.Add(ref temp, ref body.angularVelocity, out body.angularVelocity);
                    }
                }

                body.force.MakeZero();
                body.torque.MakeZero();
            }
        }

        private void IntegrateCallback(object obj)
        {
            RigidBody body = obj as RigidBody;

            // Apply linear velocity.
            if (!body.IsManuallyControlled)
            {
                body.Position += body.linearVelocity * timestep;
            }

            bool isFixedVertical = body.IsFixedVertical;
            bool isManuallyControlled = body.IsManuallyControlled;

            if (!body.IsParticle && !isFixedVertical && !isManuallyControlled)
            {
                //exponential map
                JVector axis;
                float angle = body.angularVelocity.Length();

                if (angle < 0.001f)
                {
                    // use Taylor's expansions of sync function
                    // axis = body.angularVelocity * (0.5f * timestep - (timestep * timestep * timestep) * (0.020833333333f) * angle * angle);
                    JVector.Multiply(ref body.angularVelocity, (0.5f * timestep - (timestep * timestep * timestep) * (0.020833333333f) * angle * angle), out axis);
                }
                else
                {
                    // sync(fAngle) = sin(c*fAngle)/t
                    JVector.Multiply(ref body.angularVelocity, ((float)Math.Sin(0.5f * angle * timestep) / angle), out axis);
                }

                JQuaternion dorn = new JQuaternion(axis.X, axis.Y, axis.Z, (float)Math.Cos(angle * timestep * 0.5f));
                JQuaternion.CreateFromMatrix(ref body.orientation, out var ornA);
                JQuaternion.Multiply(ref dorn, ref ornA, out dorn);

                dorn.Normalize();

                JMatrix.CreateFromQuaternion(ref dorn, out body.orientation);
            }

            // Bodies on platforms don't have any damping applied (to either linear or angular velocity).
            if ((body.Damping & RigidBody.DampingType.Linear) != 0 && !isManuallyControlled)
            {
                JVector.Multiply(ref body.linearVelocity, currentLinearDampFactor, out body.linearVelocity);
            }

            if (!isFixedVertical && (body.Damping & RigidBody.DampingType.Angular) != 0 && !isManuallyControlled)
            {
                JVector.Multiply(ref body.angularVelocity, currentAngularDampFactor, out body.angularVelocity);
            }

            body.Update();

            if (CollisionSystem.EnableSpeculativeContacts || body.AreSpeculativeContactsEnabled)
            {
                body.SweptExpandBoundingBox(timestep);
            }
        }

        private void Integrate(bool multithread)
        {
            if (multithread)
            {
                foreach (RigidBody body in rigidBodies)
                {
                    // CUSTOM: Modified to use the IsStatic property.
                    if (body.IsStatic || !body.IsActive)
                    {
                        continue;
                    }

                    threadManager.AddTask(integrateCallback, body);
                }

                threadManager.Execute();
            }
            else
            {
                foreach (RigidBody body in rigidBodies)
                {
                    // CUSTOM: Modified to use the IsStatic property.
                    if (body.IsStatic || !body.IsActive)
                    {
                        continue;
                    }

                    integrateCallback(body);
                }
            }
        }

        private void CollisionDetected(RigidBody body1, RigidBody body2, JVector point1, JVector point2,
            JVector normal, JVector[] triangle, float penetration)
        {
            Arbiter arbiter;

            lock (arbiterMap)
            {
                arbiterMap.LookUpArbiter(body1, body2, out arbiter);

                if (arbiter == null)
                {
                    arbiter = Arbiter.Pool.GetNew();
                    arbiter.body1 = body1;
                    arbiter.body2 = body2;

                    arbiterMap.Add(new ArbiterKey(body1, body2), arbiter);
                    addedArbiterQueue.Enqueue(arbiter);
                    events.RaiseBodiesBeginCollide(body1, body2);
                }
            }

            if (arbiter.body1 == body1)
            {
                JVector.Negate(ref normal, out normal);
            }

            arbiter.AddContact(point1, point2, normal, triangle, penetration, contactSettings);
        }

        private void CheckDeactivation()
        {
            // A body deactivation DOESN'T kill the contacts - they are stored in
            // the arbitermap within the arbiters. So, waking up ist STABLE - old
            // contacts are reused. Also the collisionislands build every frame (based 
            // on the contacts) keep the same.

            foreach (CollisionIsland island in islands)
            {
                bool shouldDeactivateIsland = true;

                // Global allow deactivation.
                if (!AllowDeactivation)
                {
                    shouldDeactivateIsland = false;
                }
                else
                {
                    foreach (RigidBody body in island.bodies)
                    {
                        // Body allow deactivation.
                        if (body.IsDeactivationAllowed &&
                            body.angularVelocity.LengthSquared() < inactiveAngularThresholdSq &&
                            body.linearVelocity.LengthSquared() < inactiveLinearThresholdSq)
                        {
                            body.inactiveTime += timestep;

                            if (body.inactiveTime < deactivationTime)
                            {
                                shouldDeactivateIsland = false;
                            }
                        }
                        else
                        {
                            body.inactiveTime = 0;
                            shouldDeactivateIsland = false;
                        }
                    }
                }

                foreach (RigidBody body in island.bodies)
                {
                    bool isActive = body.IsActive;

                    if (isActive == shouldDeactivateIsland)
                    {
                        if (isActive)
                        {
                            body.IsActive = false;
                            events.RaiseDeactivatedBody(body);
                        }
                        else
                        {
                            body.IsActive = true;
                            events.RaiseActivatedBody(body);
                        }
                    }

                }
            }
        }
    }
}
