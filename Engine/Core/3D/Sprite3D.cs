using System;
using Engine.Core._2D;
using Engine.Interfaces;
using Engine.Interfaces._2D;
using Engine.Interfaces._3D;
using Engine.Utility;
using Engine.View;
using GlmSharp;

namespace Engine.Core._3D
{
	public class Sprite3D : IRenderable3D, IScalable2D, IColorable, IDisposable
	{
		// This value scales pixels to meters (or in-game units). When unscaled, every X pixels spans one meter.
		public const int PixelDivisor = 100;
		
		private vec2 scale;
		private ivec2 origin;

		public Sprite3D(string filename, Alignments alignment = Alignments.Center) :
			this(ContentCache.GetTexture(filename), alignment)
		{
		}

		// For the time being, source rects for 3D sprites aren't supported (in order to simplify the rendering
		// process). This might be fine, even if source rects are never added.
		public Sprite3D(QuadSource source, Alignments alignment = Alignments.Center)
		{
			Source = source;
			origin = Utilities.ComputeOrigin(source.Width, source.Height, alignment) -
				new ivec2(source.Width, source.Height) / 2;
			Scale = vec2.Ones;
			Orientation = quat.Identity;
			Color = Color.White;
			IsShadowCaster = true;
		}

		public QuadSource Source { get; }
		public mat4 WorldMatrix { get; private set; }
		public vec3 Position { get; set; }
		public quat Orientation { get; set; }
		public Color Color { get; set; }

		public vec2 Scale
		{
			get => scale;
			set => scale = value * new vec2(Source.Width, Source.Height) / PixelDivisor;
		}

		public bool IsBillboarded { get; set; }
		public bool IsShadowCaster { get; set; }

		public void Dispose()
		{
		}

		public void SetTransform(vec3 position, quat orientation)
		{
			Position = position;
			Orientation = orientation;
		}

		public void RecomputeWorldMatrix()
		{
			// By shifting the world matrix using the origin, all sprites (regardless of transform or alignment) can be
			// rendered using the same unit square in GPU memory.
			vec3 correction = new vec3((vec2)origin / PixelDivisor, 0) * Orientation;

			WorldMatrix = mat4.Translate(Position + correction) * Orientation.ToMat4 * mat4.Scale(new vec3(scale, 0));
		}
	}
}
