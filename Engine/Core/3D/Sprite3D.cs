using System;
using Engine.Core._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Interfaces._3D;
using GlmSharp;

namespace Engine.Core._3D
{
	public class Sprite3D : ITransformable3D, IScalable2D, IColorable, IDisposable
	{
		private QuadSource source;
		private Bounds2D sourceRect;

		public Sprite3D(string filename) : this(ContentCache.GetTexture(filename))
		{
		}

		public Sprite3D(QuadSource source, Bounds2D sourceRect = null, Alignments alignment = Alignments.Center)
		{
			this.source = source;
			this.sourceRect = sourceRect;
		}

		public vec3 Position { get; set; }
		public quat Orientation { get; set; }
		public vec2 Scale { get; set; }
		public Color Color { get; set; }

		public void Dispose()
		{
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}
	}
}
