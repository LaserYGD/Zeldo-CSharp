namespace Zeldo.Items
{
	public class ItemData
	{
		public int Id { get; set; }

		public string Name { get; set; }
		public string Description { get; set; }
		
		// Spoiler text is optional and allows item descriptions to not spoil usage or lore too early.
		public string Spoiler { get; set; }
	}
}
