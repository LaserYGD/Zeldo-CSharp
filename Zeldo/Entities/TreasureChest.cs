using System.Diagnostics;
using Engine;
using Engine.Core._3D;
using Engine.Physics;
using Engine.Timing;
using GlmSharp;
using Jitter.Dynamics;
using Newtonsoft.Json.Linq;
using Zeldo.Entities.Core;
using Zeldo.Interfaces;

namespace Zeldo.Entities
{
	// TODO: Allow chests to be locked.
	// TODO: Add more complex chest animations (e.g. using gears or vines).
	public class TreasureChest : Entity, IInteractive
	{
		private static readonly float LidRange;
		private static readonly float LidDuration;

		static TreasureChest()
		{
			LidRange = Properties.GetFloat("treasure.chest.lid.range");
			LidDuration = Properties.GetFloat("treasure.chest.lid.duration");
		}

		private int itemId;
		private bool isOpened;

		private Model lidModel;
		private RigidBody lidBody;
		private vec3 lidPivot;

		public TreasureChest() : base(EntityGroups.Object)
		{
		}

		public bool IsInteractionEnabled => !isOpened;
		public bool RequiresFacing => true;

		public override void Initialize(Scene scene, JToken data)
		{
			Debug.Assert(data["Item"] != null, "Missing item ID.");
			Debug.Assert(data["Model"] != null, "Missing model.");

			itemId = data["Item"].Value<int>();

			// Treasure chests contain two meshes (the base container and the lid).
			var model = CreateModel(scene, data["Model"].Value<string>());
			var bounds = model.Mesh.Bounds;

			/*
			float interactionRadius = Properties.GetFloat("treasure.chest.interaction.radius");
			float thickness = Properties.GetFloat("treasure.chest.thickness");

			var bounds = CreateModel(scene, "TreasureChest.obj").Mesh.Bounds;
			bounds.x = 0.75f;

			float halfThickness = thickness / 2;
			float xOffset = bounds.x / 2 - halfThickness;
			float zOffset = bounds.z / 2 - halfThickness;

			BoxShape widthBox = new BoxShape(bounds.x, bounds.y, thickness);
			BoxShape depthBox = new BoxShape(thickness, bounds.y, bounds.z);

			//CreateRigidBody3D(scene, new BoxShape(bounds.z, bounds.y, bounds.x), false, true);
			CreateRigidBody(scene, widthBox, false, true, new vec3(0, 0, zOffset));
			CreateRigidBody(scene, widthBox, false, true, new vec3(0, 0, -zOffset));
			CreateRigidBody(scene, depthBox, false, true, new vec3(xOffset, 0, 0));
			CreateRigidBody(scene, depthBox, false, true, new vec3(-xOffset, 0, 0));
			CreateGroundBody(scene, new Rectangle(bounds.x, bounds.z), true);
			CreateSensor(scene, new Circle(interactionRadius), SensorUsages.Interaction);

			lidPivot = new vec3(-0.375f, bounds.y / 2, 0);
			lidModel = CreateModel(scene, "TreasureChestLid.obj", lidPivot);

			var lidBounds = lidModel.Mesh.Bounds;

			lidBody = CreateRigidBody3D(scene, new BoxShape(lidBounds.ToJVector()), false, true,
				new vec3(0, bounds.y / 2 + lidBounds.y / 2, 0));
			*/

			base.Initialize(scene, data);
		}

		public void OnInteract(Entity entity)
		{
			Player player = (Player)entity;
			player.Inventory.Add(itemId);

			/*
			SingleTimer timer = new SingleTimer(time =>	{ }, LidDuration);
			timer.Tick = progress =>
			{
				quat orientation = quat.SLerp(quat.Identity, quat.FromAxisAngle(LidRange, vec3.UnitZ), progress);
				vec3 bodyPosition = Position + lidPivot + orientation * new vec3(lidModel.Mesh.Bounds.swizzle.xy / 2);

				// Note that due to the way attachments work, the lid body won't be positioned properly if the chest is
				// moved after (or while) being opened. This doesn't matter if treasure chests are fixed, but might
				// need to be modified if chests can move after being spawned.
				lidModel.Orientation = orientation;
				lidBody.Orientation = orientation.ToJMatrix();
				lidBody.Position = bodyPosition.ToJVector();
			};

			Components.Add(timer);
			*/

			isOpened = true;
			RemoveSensor();
		}
	}
}
