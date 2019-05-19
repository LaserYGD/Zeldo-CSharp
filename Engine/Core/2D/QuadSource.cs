using static Engine.GL;

namespace Engine.Core._2D
{
	public abstract class QuadSource
	{
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
		public int Width { get; }
		public int Height { get; }

		public void Bind(uint index)
		{
			glActiveTexture(GL_TEXTURE0 + index);
			glBindTexture(GL_TEXTURE_2D, Id);
		}
	}
}
