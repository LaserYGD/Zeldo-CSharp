using Engine.Interfaces;

namespace Engine.View
{
	public abstract class CameraController3D : IDynamic
	{
		protected CameraController3D(Camera3D camera)
		{
			Camera = camera;
		}

		protected Camera3D Camera { get; }

		public abstract void Update(float dt);
	}
}
