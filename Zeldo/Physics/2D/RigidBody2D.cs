using Engine.Interfaces._2D;
using Engine.Shapes._2D;
using GlmSharp;

namespace Zeldo.Physics._2D
{
	public class RigidBody2D : IPositionable2D, IRotatable
	{
		public RigidBody2D(Shape2D shape, bool isStatic = false)
		{
			Shape = shape;
			IsStatic = isStatic;
		}

		public vec2 Position
		{
			get => Shape.Position;
			set => Shape.Position = value;
		}

		public vec2 Velocity { get; set; }
		public Shape2D Shape { get; }

		public float Elevation { get; set; }
		public float Rotation { get; set; }

		public bool IsStatic { get; }
	}
}
