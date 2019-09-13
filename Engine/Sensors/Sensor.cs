﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Engine.Interfaces._3D;
using Engine.Shapes._3D;
using GlmSharp;

namespace Engine.Sensors
{
	public class Sensor : ITransformable3D
	{
		private const string AssertMessage = "Can't modify a sensor from a callback.";

		private bool isEnabled;
		private int collidesWith;

		private Shape3D shape;

		protected Sensor(SensorTypes type, object owner, int group, bool isCompound, Shape3D shape = null)
		{
			Debug.Assert(owner != null, "Owner can't be null.");

			this.shape = shape;

			Type = type;
			Group = group;

			// By default, sensors collide with nothing.
			CollidesWith = 0;
			Owner = owner;
			IsEnabled = true;
			IsCompound = isCompound;
			Contacts = new List<Sensor>();
		}

		public Sensor(SensorTypes type, object owner, int group, Shape3D shape = null) :
			this(type, owner, group, false, shape)
		{
		}

		internal Space Space { get; set; }
		
		internal bool IsTogglePending { get; private set; }
		internal bool IsMarkedForDestruction { get; set; }
		internal bool IsCompound { get; }
		internal int Group { get; }


		internal SensorTypes Type { get; }

		public int CollidesWith
		{
			get => collidesWith;
			set
			{
				Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);

				collidesWith = value;
			}
		}

		public virtual vec3 Position
		{
			get => Shape.Position;
			set
			{
				Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);

				Shape.Position = value;
			}
		}

		public virtual quat Orientation
		{
			get => Shape.Orientation;
			set
			{
				Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);

				Shape.Orientation = value;
			}
		}

		public object Owner { get; }

		public Shape3D Shape
		{
			internal get => shape;
			set
			{
				Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);

				shape = value;
			}
		}

		public List<Sensor> Contacts { get; }

		public Action<SensorTypes, object> OnSense { get; set; }
		public Action<SensorTypes, object> OnStay { get; set; }
		public Action<SensorTypes, object> OnSeparate { get; set; }

		public bool IsEnabled
		{
			get => isEnabled;
			set
			{
				// Disabling an active sensor is more complex than just swapping a flag (since existing contacts need
				// to be updated). As such, attempting to toggle a sensor mid-loop instead marks it to be changed
				// later (after the Space's main loop has finished).
				if (Space != null && Space.IsUpdateActive)
				{
					IsTogglePending = isEnabled != value;
				}
				else
				{
					if (isEnabled && !value)
					{
						ClearContacts();
					}

					isEnabled = value;
				}
			}
		}

		public virtual void SetTransform(vec3 position, quat orientation)
		{
			Debug.Assert(Space == null || !Space.IsUpdateActive, AssertMessage);

			Position = position;
			Orientation = orientation;
		}

	    internal void ClearContacts()
	    {
	        foreach (Sensor other in Contacts)
	        {
				OnSeparate?.Invoke(other.Type, other.Owner);

		        other.OnSeparate?.Invoke(Type, Owner);
		        other.Contacts.Remove(this);
			}

	        Contacts.Clear();
        }
	}
}
