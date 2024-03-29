﻿using NAudio.Wave;

namespace Engine.Sound
{
	public class AutoDisposeFileReader : ISampleProvider
	{
		private readonly AudioFileReader reader;
		private bool isDisposed;

		public AutoDisposeFileReader(AudioFileReader reader)
		{
			this.reader = reader;

			WaveFormat = reader.WaveFormat;
		}

		public WaveFormat WaveFormat { get; }

		public int Read(float[] buffer, int offset, int count)
		{
			if (isDisposed)
			{
				return 0;
			}

			int read = reader.Read(buffer, offset, count);

			if (read == 0)
			{
				reader.Dispose();
				isDisposed = true;
			}

			return read;
		}
	}
}
