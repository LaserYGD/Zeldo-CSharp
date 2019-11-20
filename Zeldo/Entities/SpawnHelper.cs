using System;
using Engine.Editing;
using Zeldo.Entities.Core;

namespace Zeldo.Entities
{
	public class SpawnHelper
	{
		private Scene scene;

		public SpawnHelper(Scene scene)
		{
			this.scene = scene;

			var terminal = scene.Canvas.GetElement<Terminal>();
			terminal.Add("spawn", Spawn);
		}

		private bool Spawn(string[] args, out string result)
		{
			const string Prefix = "Zeldo.Entities.";

			if (args.Length != 1)
			{
				result = "Usage: spawn *class* (e.g. 'spawn Objects.Crate')";

				return false;
			}

			var raw = args[0];
			var type = Type.GetType(Prefix + args[0]);

			if (type == null)
			{
				result = $"Unknown entity '{Prefix}{raw}'.";

				return false;
			}

			result = $"Spawned '{Prefix}{raw}'.";

			return true;
		}
	}
}
