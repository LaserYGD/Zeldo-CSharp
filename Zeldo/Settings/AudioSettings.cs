
namespace Zeldo.Settings
{
	public class AudioSettings
	{
		public int MasterVolume { get; set; }
		public int EffectsVolume { get; set; }
		public int MusicVolume { get; set; }
		public int VoiceVolume { get; set; }

		public bool IsMasterMuted { get; set; }
		public bool IsEffectsMuted { get; set; }
		public bool IsMusicMuted { get; set; }
		public bool IsVoiceMuted { get; set; }

		public SubtitleTypes Subtitles { get; set; }

		public bool DirectionalSubtitlesEnabled { get; set; }
	}
}
