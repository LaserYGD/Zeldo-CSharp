using Zeldo.State;

namespace Zeldo.Data
{
	public class SaveSlot
	{
		// TODO: Load data.
		public SaveSlot(string filename)
		{
		}

		public string Name { get; }

		// As an anti-spoiler mechanism, save slots hide completion percentage until the player enables it.
		public bool ShowPercentage { get; set; }

		public Progression Progression { get; }
	}
}
