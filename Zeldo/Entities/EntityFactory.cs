using System;
using System.Collections.Generic;
using Zeldo.Entities.Characters;
using Zeldo.Entities.Core;
using Zeldo.Entities.Enemies;

namespace Zeldo.Entities
{
	public class EntityFactory
	{
		private Dictionary<string, Type> typeMap;

		public EntityFactory()
		{
			typeMap = new Dictionary<string, Type>()
			{
				// Enemies
				{ "Sunflower", typeof(Sunflower) },

				// Characters
				{ "Watchmaker", typeof(Watchmaker) },

				// Objects
				{ "TreasureChest", typeof(TreasureChest) }
			};
		}

		public Entity Create(string entityType)
		{
			return (Entity)Activator.CreateInstance(typeMap[entityType]);
		}
	}
}
