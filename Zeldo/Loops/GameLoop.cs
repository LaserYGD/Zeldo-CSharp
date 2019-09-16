using System;
using System.Collections.Generic;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.UI;

namespace Zeldo.Loops
{
	public abstract class GameLoop : IDynamic, IDisposable
	{
		protected Canvas canvas;
		protected SpriteBatch sb;
		protected List<IRenderTargetUser3D> renderTargetUsers3D;

		protected GameLoop(LoopTypes type)
		{
			LoopType = type;
			renderTargetUsers3D = new List<IRenderTargetUser3D>();
		}

		public LoopTypes LoopType { get; }

		// All loops can render 2D elements.
		public Canvas Canvas { set => canvas = value; }
		public SpriteBatch SpriteBatch { set => sb = value; }

		public virtual void Dispose()
		{
		}

		public abstract void Initialize();
		public abstract void Update(float dt);
		public abstract void Draw();

		public void DrawTargets()
		{
			renderTargetUsers3D.ForEach(t => t.DrawTargets());
		}
	}
}
