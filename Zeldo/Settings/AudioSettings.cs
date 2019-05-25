
namespace Zeldo.Settings
{
	public class AudioSettings
	{
		public int MasterVolume { get; set; }
		public int EffectsVolume { get; set; }
		public int MusicVolume { get; set; }
		public int VoiceVolume { get; set; }

		public bool MasterMuted { get; set; }
		public bool EffectsMuted { get; set; }
		public bool MusicMuted { get; set; }
		public bool VoiceMuted { get; set; }

		public bool SubtitlesEnabled { get; set; }
	}
}
