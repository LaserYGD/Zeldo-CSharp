using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace Engine.Sound
{
	// See https://markheath.net/post/fire-and-forget-audio-playback-with.
	public class AudioPlayback : IDisposable
	{
		private IWavePlayer outputDevice;
		private MixingSampleProvider mixer;

		public AudioPlayback()
		{
			mixer = new MixingSampleProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
			mixer.ReadFully = true;

			outputDevice = new WaveOutEvent();
			outputDevice.Init(mixer);
			outputDevice.Play();
		}

		public void Play(string filename)
		{
			var input = new AudioFileReader("Content/Sounds/" + filename);
			AddMixerInput(new AutoDisposeFileReader(input));
		}

		public void Play(CachedSound sound)
		{
			AddMixerInput(new CachedSoundSampleProvider(sound));
		}

		private ISampleProvider ConvertToRightChannelCount(ISampleProvider input)
		{
			if (input.WaveFormat.Channels == mixer.WaveFormat.Channels)
			{
				return input;
			}
			
			if (input.WaveFormat.Channels == 1 && mixer.WaveFormat.Channels == 2)
			{
				return new MonoToStereoSampleProvider(input);
			}

			throw new NotImplementedException("Not yet implemented this channel count conversion.");
		}

		private void AddMixerInput(ISampleProvider input)
		{
			mixer.AddMixerInput(ConvertToRightChannelCount(input));
		}

		public void Dispose()
		{
		}
	}
}
