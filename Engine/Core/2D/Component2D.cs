using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Graphics;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using GlmSharp;

namespace Engine.Core._2D
{
	public abstract class Component2D : IPositionable2D, IRotatable, IColorable, IRenderable2D
	{
		public vec2 Position { get; set; }

		public float Rotation { get; set; }

		public Color Color { get; set; }

		public abstract void Draw(SpriteBatch sb);
	}
}
