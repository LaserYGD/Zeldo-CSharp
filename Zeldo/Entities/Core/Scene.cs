using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Engine;
using Engine.Graphics._3D;
using Engine.Graphics._3D.Rendering;
using Engine.Input.Data;
using Engine.Interfaces;
using Engine.Messaging;
using Engine.Sensors;
using Engine.UI;
using Engine.Utility;
using Engine.View;
using Jitter;
using Zeldo.Entities.Player;
using static Engine.GLFW;

namespace Zeldo.Entities.Core
{
	public class Scene : IDynamic
	{
		private Camera3D camera;
		private MasterRenderer3D renderer;
		private List<Entity>[] entities;
		private SceneFragment lastFragment;
		private PrimitiveRenderer3D primitives;

		public Scene()
		{
			entities = new List<Entity>[Utilities.EnumCount<EntityGroups>()];

			for (int i = 0; i < entities.Length; i++)
			{
				entities[i] = new List<Entity>();
			}

			Tags = new Dictionary<string, object>();
		}

		public Camera3D Camera
		{
			get => camera;
			set
			{
				camera = value;

				renderer = new MasterRenderer3D();
				renderer.ShadowNearPlane = Properties.GetFloat("shadow.near.plane");
				renderer.ShadowFarPlane = Properties.GetFloat("shadow.far.plane");

				primitives = new PrimitiveRenderer3D(value, 10000, 1000);
			}
		}

		public Canvas Canvas { get; set; }
		public Space Space { get; set; }
		public World World { get; set; }
		public MasterRenderer3D Renderer => renderer;
		public PrimitiveRenderer3D Primitives => primitives;

		// TODO: Are tags needed?
		// In this context, a "tag" means custom data optionally loaded with each fragment. Used as needed in order to
		// implement custom features for different kinds of locations.
		public Dictionary<string, object> Tags { get; }

		// This is used for debug purposes.
		public int Size => entities.Sum(l => l.Count);

		// TODO: Consider generating a fragment ID (in order to ensure that entity IDs are unique among fragments).
		public SceneFragment LoadFragment(string filename)
		{
			// TODO: Track fragments (and assert for duplicate fragments).
			var fragment = SceneFragment.Load(filename, this);
			var eList = fragment.Entities;

			renderer.Add(fragment.MapModel);
			World.AddBody(fragment.MapBody);

			// It's valid (though unlikely) for a fragment to contain no entities.
			if (eList != null)
			{
				foreach (var e in eList)
				{
					// Entities are initialized by the fragment (which is why the regular Add function isn't used).
					entities[(int)e.Group].Add(e);
				}

				foreach (var e in eList)
				{
					// Handles must be resolved after all entities are initialized *and* added to their corresponding
					// groups lists.
					e.ResolveHandles(this);
				}
			}

			lastFragment = fragment;

			return fragment;
		}

		// TODO: Should this be moved to the fragment class?
		public void UnloadFragment(SceneFragment fragment)
		{
			var model = fragment.MapModel;
			model.Dispose();

			Renderer.Remove(model);
			World.Remove(fragment.MapBody);

			foreach (var e in fragment.Entities)
			{
				e.Dispose();
				entities[(int)e.Group].Remove(e);
			}
		}

		public void Reload()
		{
			var filename = lastFragment.Filename;

			UnloadFragment(lastFragment);
			LoadFragment(filename);

			var player = (PlayerCharacter)entities[(int)EntityGroups.Player][0];
			var p = lastFragment.Origin + lastFragment.Spawn;

			player.Reset(p);
			primitives.Clear();
		}

		public void Add(Entity entity)
		{
			entities[(int)entity.Group].Add(entity);
			entity.Initialize(this, null);
		}

		public void Remove(Entity entity)
		{
			entity.Dispose();
			entities[(int)entity.Group].Remove(entity);
		}

		public List<Entity> GetEntities(EntityGroups group)
		{
			return entities[(int)group];
		}

		public List<T> GetEntities<T>(EntityGroups group) where T : Entity
		{
			// TODO: Should this filter down to entities of the given type instead?
			return entities[(int)group].Cast<T>().ToList();
		}

		public void Dispose()
		{
		}

		public void Update(float dt)
		{
			foreach (var list in entities)
			{
				list.ForEach(e => e.Update(dt));
			}
		}

		public void Draw(Camera3D camera)
		{
			renderer.VpMatrix = camera.ViewProjection;
			renderer.Draw();

			primitives.Flush();
		}
	}
}
