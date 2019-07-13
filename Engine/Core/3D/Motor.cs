using Engine.Interfaces;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Core._3D
{
	public class Motor : IDynamic
	{
		private IOrientable parent;

		private vec3 axis;
		private quat step;

		private float angularVelocity;

		public Motor(IOrientable parent)
		{
			this.parent = parent;
		}

		public bool IsRunning { get; set; }

		public vec3 Axis
		{
			get => axis;
			set
			{
				axis = value;
				step = quat.FromAxisAngle(angularVelocity, axis);
			}
		}

		public float AngularVelocity
		{
			get => angularVelocity;
			set
			{
				angularVelocity = value;
				step = quat.FromAxisAngle(angularVelocity, axis);
			}
		}

		public void Update(float dt)
		{
			if (IsRunning)
			{
				parent.Orientation *= step;
			}
		}
	}
}
