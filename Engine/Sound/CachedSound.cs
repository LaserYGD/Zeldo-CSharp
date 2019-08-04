using System.Collections.Generic;
using System.Linq;
using NAudio.Wave;

namespace Engine.Sound
{
	// See https://markheath.net/post/fire-and-forget-audio-playback-with.
	public class CachedSound
	{
		public CachedSound(string filename)
		{
			using (var audioFileReader = new AudioFileReader("Content/Sounds/" + filename))
			{
				WaveFormat = audioFileReader.WaveFormat;

				var wholeFile = new List<float>((int)(audioFileReader.Length / 4));
				var readBuffer = new float[audioFileReader.WaveFormat.SampleRate * audioFileReader.WaveFormat.Channels];

				int samplesRead;

				while ((samplesRead = audioFileReader.Read(readBuffer, 0, readBuffer.Length)) > 0)
				{
					wholeFile.AddRange(readBuffer.Take(samplesRead));
				}

				AudioData = wholeFile.ToArray();
			}
		}

		public float[] AudioData { get; }
		public WaveFormat WaveFormat { get; }
	}
}
