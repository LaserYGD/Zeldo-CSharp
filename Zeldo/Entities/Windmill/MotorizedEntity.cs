using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;

namespace Zeldo.Entities.Windmill
{
	public class MotorizedEntity : Entity
	{
		private MotorNode node;

		public MotorizedEntity() : base(EntityGroups.Structure)
		{
		}

		public override void Initialize(Scene scene, JToken data)
		{
			// The base function is called first to ensure that the entity's transform is already set (required to have
			// motor nodes function correctly).
			base.Initialize(scene, data);

			// TODO: Pull radius (if needed).
			node = new MotorNode(this, 0);
		}

		protected override void ResolveHandles(Scene scene, List<EntityHandle> handles)
		{
			handles.ForEach(h =>
			{
				node.Children.Add(h.Resolve<MotorizedEntity>(scene).node);
			});
		}
	}
}
