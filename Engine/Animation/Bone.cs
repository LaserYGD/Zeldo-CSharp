using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Animation
{
	public class Bone : ITransformable3D
	{
		public vec3 Position { get; set; }
		public vec3 LocalPosition { get; set; }
		public quat Orientation { get; set; }
		public quat LocalOrientation { get; set; }
		public Bone[] Children { get; }

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}
	}
}