using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace Zeldo.Entities.Bosses.Octopus
{
	public class OctopusBoss : Boss
	{
		private Tentacle tentacle;

		public OctopusBoss() : base("octopus.boss")
		{
			tentacle = new Tentacle(this);
		}

		// The octopus boss fight takes place in a roughly cylindrical arena (a partial cylinder, at least), with the
		// boss located in the center. Storing the central axis allows certain tentacle attacks (like sweeps) to
		// better control their movement (by moving around that axis).
		public vec2 Axis { get; }
	}
}
