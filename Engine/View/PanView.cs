using System.Diagnostics;
using Engine.Structures;
using Engine.Timing;
using GlmSharp;

namespace Engine.View
{
	public class PanView : CameraController3D
	{
		private SingleTimer timer;

		public PanView(Camera3D camera) : base(camera)
		{
			timer = new SingleTimer();
		}

		// If target is set, the camera orients itself to face the target each frame.
		public vec3? Target { private get; set; }

		// Using this view, the camera can pan either 1) between two points (using this function), or 2) along a curved
		// path (using the overloaded version below).
		public void Refresh(vec3 p1, vec3 p2, float duration)
		{
			// TODO: Assign timer trigger function.
			timer.Duration = duration;
			timer.Tick = t =>
			{
				Recompute(vec3.Lerp(p1, p2, t));
			};
		}

		public void Refresh(Curve3D path, float duration)
		{
			Debug.Assert(path != null, "Given path was null.");

			// TODO: Assign timer trigger function.
			timer.Duration = duration;
			timer.Tick = t =>
			{
				Recompute(path.Evaluate(t));
			};
		}

		private void Recompute(vec3 p)
		{
			Camera.Position = p;

			if (Target.HasValue)
			{
				Camera.Orientation = mat4.LookAt(p, Target.Value, vec3.UnitY).ToQuaternion;
			}
		}

		public override void Update(float dt)
		{
			Debug.Assert(timer.Tick != null, "Pan view wasn't initialized.");

			timer.Update(dt);
		}
	}
}
