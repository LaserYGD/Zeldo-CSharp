using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine.View
{
	public abstract class CameraController3D
	{
		protected Camera3D Camera { get; private set; }

		public virtual void Initialize(Camera3D camera)
		{
			Camera = camera;
		}

		public abstract void Update();
	}
}
