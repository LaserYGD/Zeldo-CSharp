using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zeldo.Settings;
using Zeldo.UI.Controls;

namespace Zeldo.UI.Screens.Settings
{
	public class AudioScreen
	{
		private Slider masterSlider;
		private Slider effectsSlider;
		private Slider musicSlider;
		private Slider voiceSlider;

		private ToggleButton masterMuteButton;
		private ToggleButton effectsMuteButton;
		private ToggleButton musicMuteButton;
		private ToggleButton voiceMuteButton;

		private AudioSettings settings;

		public AudioScreen()
		{
			masterSlider = new Slider();
			effectsSlider = new Slider();
			musicSlider = new Slider();
			voiceSlider = new Slider();
		}

		public AudioSettings Settings
		{
			set
			{
				settings = value;

				masterSlider.Value = settings.MasterVolume;
				effectsSlider.Value = settings.EffectsVolume;
				musicSlider.Value = settings.MusicVolume;
				voiceSlider.Value = settings.VoiceVolume;

				masterMuteButton.IsPressed = settings.IsMasterMuted;
				effectsMuteButton.IsPressed = settings.IsEffectsMuted;
				musicMuteButton.IsPressed = settings.IsMusicMuted;
				voiceMuteButton.IsPressed = settings.IsVoiceMuted;
			}
		}
	}
}
