using Engine;
using Engine.Smoothers._3D;
using Engine.Utility;
using GlmSharp;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities
{
	public class JesterWall : Entity, IMechanical
	{
		private float distance;
		private float duration;

		public JesterWall() : base(EntityGroups.Object)
		{
			distance = Properties.GetFloat("jester.wall.distance");
			duration = Properties.GetFloat("jester.wall.duration");
		}

		public int Id => EntityIds.JesterWall;

		public void TriggerMechanism()
		{
			vec3 start = Position;
			vec3 end = Position + new vec3(0, distance, 0);

			Components.Add(new PositionSmoother3D(this, start, end, duration, EaseTypes.Linear));
		}
	}
}
