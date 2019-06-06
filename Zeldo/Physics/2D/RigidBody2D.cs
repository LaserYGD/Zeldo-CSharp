using Engine.Shapes._2D;
using GlmSharp;
using Zeldo.Interfaces;

namespace Zeldo.Physics._2D
{
	public class RigidBody2D : ITransformable2D
	{
		public RigidBody2D(Shape2D shape, bool isStatic = false)
		{
			Shape = shape;
			IsStatic = isStatic;
			IsEnabled = true;
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
		public bool IsEnabled { get; set; }

		public void SetTransform(vec2 position, float elevation, float rotation)
		{
			Position = position;
			Elevation = elevation;
			Rotation = rotation;
		}
	}
}
