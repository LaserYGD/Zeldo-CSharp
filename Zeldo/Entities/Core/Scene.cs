using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics._3D;
using Engine.Graphics._3D.Rendering;
using Engine.Interfaces;
using Engine.Sensors;
using Engine.UI;
using Engine.Utility;
using Engine.View;
using Jitter;

namespace Zeldo.Entities.Core
{
	public class Scene : IDynamic
	{
		private Camera3D camera;
		private MasterRenderer3D renderer;
		private List<Entity>[] entities;
		private List<SceneFragment> fragments;
		private PrimitiveRenderer3D debugPrimitives;

		public Scene()
		{
			entities = new List<Entity>[Utilities.EnumCount<EntityGroups>()];

			for (int i = 0; i < entities.Length; i++)
			{
				entities[i] = new List<Entity>();
			}

			fragments = new List<SceneFragment>();
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

				debugPrimitives = new PrimitiveRenderer3D(value, 10000, 1000);
			}
		}

		public Canvas Canvas { get; set; }
		public Space Space { get; set; }
		public World World { get; set; }
		public MasterRenderer3D Renderer => renderer;
		public PrimitiveRenderer3D DebugPrimitives => debugPrimitives;

		// In this context, a "tag" means custom data optionally loaded with each fragment. Used as needed in order to
		// implement custom features for different kinds of locations.
		public Dictionary<string, object> Tags { get; }

		public void LoadFragment(string filename)
		{
			var fragment = SceneFragment.Load(filename, this);
			fragments.Add(fragment);
			//renderer.Add(fragment.MapModel);
			//World.AddBody(fragment.MapBody);

			// This means that the fragment contains no entities (unlikely in the finished product, but possible during
			// development).
			if (fragment.Entities != null)
			{
				foreach (var entity in fragment.Entities)
				{
					// Entities are already initialized by this point (which is why the main Add function isn't used).
					entities[(int)entity.Group].Add(entity);
				}
			}
		}

		public void UnloadFragment()
		{
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
			return entities[(int)group].Cast<T>().ToList();
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

			debugPrimitives.Flush();
		}
	}
}
