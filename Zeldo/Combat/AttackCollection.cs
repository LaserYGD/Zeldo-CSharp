using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine.Utility;
using Zeldo.Entities.Core;

namespace Zeldo.Combat
{
	public class AttackCollection<T> where T : LivingEntity
	{
		private Dictionary<int, Attack<T>> attacks;

		public AttackCollection(string filename, T parent)
		{
			Debug.Assert(parent != null, "Can't create an attack collection with a null parent.");

			var map = JsonUtilities.Deserialize<Dictionary<string, AttackData>>("Combat/" + filename);
				
			attacks = new Dictionary<int, Attack<T>>();

			// Attack names are internal, so they're stored using hash codes rather than the raw string.
			foreach (var pair in map)
			{
				attacks.Add(pair.Key.GetHashCode(), pair.Value.CreateAttack(parent));
			}
		}

		public Attack<T> this[string key] => attacks[key.GetHashCode()];

		public Attack<T> Execute()
		{
			// If no attacks have their trigger requirements met, null is returned. This shouldn't happen in the final
			// game (once all attacks are finished), but it might during development.
			return attacks.Values.FirstOrDefault(a => a.ShouldTrigger());
		}
	}
}
