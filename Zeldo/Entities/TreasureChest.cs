using Engine;
using Engine.Core._3D;
using Engine.Shapes._2D;
using Engine.Smoothers._3D;
using Engine.Utility;
using GlmSharp;
using Jitter.Collision.Shapes;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities
{
	public class TreasureChest : Entity, IInteractive
	{
		private static float lidRange;
		private static float lidDuration;

		static TreasureChest()
		{
			lidRange = Properties.GetFloat("treasure.chest.lid.range");
			lidDuration = Properties.GetFloat("treasure.chest.lid.duration");
		}

		private int itemId;
		private bool isOpened;

		private Model lidModel;

		public TreasureChest() : base(EntityGroups.Object)
		{
		}

		public bool IsInteractionEnabled => !isOpened;

		public override void Initialize(Scene scene)
		{
			float interactionRadius = Properties.GetFloat("treasure.chest.interaction.radius");
			float thickness = Properties.GetFloat("treasure.chest.thickness");

			var bounds = CreateModel(scene, "TreasureChest.obj").Mesh.Bounds;

			float halfX = bounds.x / 2;
			float halfZ = bounds.z / 2;

			BoxShape widthBox = new BoxShape(thickness, bounds.y, bounds.x);
			BoxShape depthBox = new BoxShape(bounds.z, bounds.y, thickness);

			//CreateRigidBody3D(scene, new BoxShape(bounds.z, bounds.y, bounds.x), false, true);
			CreateRigidBody3D(scene, widthBox, false, true, new vec3(0, 0, halfZ));
			CreateRigidBody3D(scene, widthBox, false, true, new vec3(0, 0, -halfZ));
			CreateRigidBody3D(scene, depthBox, false, true, new vec3(halfX, 0, 0));
			CreateRigidBody3D(scene, depthBox, false, true, new vec3(-halfX, 0, 0));
			CreateGroundBody(scene, new Rectangle(bounds.x, bounds.z), true);
			CreateSensor(scene, new Circle(interactionRadius));

			lidModel = CreateModel(scene, "TreasureChestLid.obj", new vec3(-0.375f, bounds.y / 2, 0));

			base.Initialize(scene);
		}

		public void OnInteract(Entity entity)
		{
			Player player = (Player)entity;
			player.GiveItem(itemId);

			Components.Add(new OrientationSmoother(lidModel, quat.Identity, quat.FromAxisAngle(lidRange,
				vec3.UnitZ), lidDuration, EaseTypes.Linear));

			isOpened = true;
			RemoveSensor();
		}
	}
}
