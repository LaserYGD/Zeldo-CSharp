using Engine.Core._3D;
using Engine.Shapes._2D;
using GlmSharp;
using Jitter.Collision.Shapes;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities
{
	public class TreasureChest : Entity, IInteractive
	{
		private int itemId;
		private bool opened;

		private Model lidModel;

		public TreasureChest() : base(EntityGroups.Object)
		{
		}

		public bool InteractionEnabled => !opened;

		public override void Initialize(Scene scene)
		{
			var bounds = CreateModel(scene, "TreasureChest.obj").Mesh.Bounds;

			CreateRigidBody3D(scene, new BoxShape(bounds.z, bounds.y, bounds.x), false, true);
			CreateGroundBody(scene, new Rectangle(bounds.x, bounds.z), true);

			lidModel = CreateModel(scene, "TreasureChestLid.obj", new vec3(-0.375f, bounds.y / 2, 0));

			base.Initialize(scene);
		}

		public void OnInteract(Entity entity)
		{
			Player player = (Player)entity;
			player.GiveItem(itemId);

			opened = true;
		}
	}
}
