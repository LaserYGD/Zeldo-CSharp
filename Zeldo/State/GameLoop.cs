using System;
using Engine.Graphics._2D;
using Engine.Interfaces;
using Engine.UI;

namespace Zeldo.State
{
	public abstract class GameLoop : IDynamic, IDisposable
	{
		protected Canvas canvas;
		protected SpriteBatch sb;

		public void Transfer(GameLoop loop)
		{
			if (loop == null)
			{
				canvas = new Canvas();
				sb = new SpriteBatch();

				return;
			}

			canvas = loop.Canvas;
			sb = loop.SpriteBatch;
		}

		public Canvas Canvas => canvas;
		public SpriteBatch SpriteBatch => sb;

		public virtual void Dispose()
		{
		}

		public abstract void Initialize();
		public abstract void Update(float dt);
		public abstract void Draw();
	}
}
