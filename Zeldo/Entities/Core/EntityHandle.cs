using System;
using System.Diagnostics;
using System.Linq;
using Engine;
using Newtonsoft.Json.Linq;

namespace Zeldo.Entities.Core
{
	// TODO: If needed, find a way to link entities among fragments (likely by using a generated fragment ID).
	public class EntityHandle
	{
		private EntityGroups group;

		private int id;
		private int usage;

		public EntityHandle(JToken token)
		{
			if (!Enum.TryParse(token["Group"].Value<string>(), out group))
			{
				Debug.Fail("Invalid group within entity handle.");
			}

			id = token["Id"].Value<int>();

			Debug.Assert(id >= 0, "Entity handle ID can't be negative.");

			// Usage is optional and allows entities to distinguish among multiple handles (if group isn't sufficient).
			if (token.TryParse("Usage", out usage))
			{
				Debug.Assert(usage >= 0, "Given entity handle usage can't be negative.");
			}
			else
			{
				usage = -1;
			}
		}

		public int Usage => usage;

		public T Resolve<T>(Scene scene) where T : Entity
		{
			return (T)scene.GetEntities(group).First(e => e.Id == id);
		}
	}
}
