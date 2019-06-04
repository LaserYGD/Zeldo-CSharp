using GlmSharp;
using static Engine.GL;

namespace Engine.Core._2D
{
	public abstract class QuadSource
	{
		protected QuadSource() : this(0, 0, 0)
		{
		}

		protected QuadSource(int width, int height) : this(0, width, height)
		{
		}

		protected QuadSource(uint id, int width, int height)
		{
			Id = id;
			Width = width;
			Height = height;
		}

		public uint Id { get; protected set; }
		public int Width { get; protected set; }
		public int Height { get; protected set; }

		public ivec2 Dimensions => new ivec2(Width, Height);

		public void Bind(uint index)
		{
			glActiveTexture(GL_TEXTURE0 + index);
			glBindTexture(GL_TEXTURE_2D, Id);
		}
	}
}
