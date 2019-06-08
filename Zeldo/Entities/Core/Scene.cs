using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics._3D;
using Engine.Interfaces;
using Engine.Interfaces._3D;
using Engine.UI;
using Engine.Utility;
using Engine.View;
using Jitter;
using Newtonsoft.Json.Linq;
using Zeldo.Physics._2D;
using Zeldo.Sensors;

namespace Zeldo.Entities.Core
{
	public class Scene : IDynamic, IRenderable3D
	{
		private Camera3D camera;
		private ModelBatch batch;
		private List<Entity>[] entities;
		private List<SceneFragment> fragments;

		public Scene()
		{
			entities = new List<Entity>[Utilities.EnumCount<EntityGroups>()];

			for (int i = 0; i < entities.Length; i++)
			{
				entities[i] = new List<Entity>();
			}

			fragments = new List<SceneFragment>();
			UserData = new Dictionary<string, object>();
		}

		public Camera3D Camera
		{
			get => camera;
			set
			{
				camera = value;

				int bufferSize = Properties.GetInt("model.batch.buffer.size");
				int indexSize = Properties.GetInt("model.batch.index.size");

				batch = new ModelBatch(camera, bufferSize, indexSize);
				batch.ShadowNearPlane = Properties.GetFloat("shadow.near.plane");
				batch.ShadowFarPlane = Properties.GetFloat("shadow.far.plane");
			}
		}

		public Canvas Canvas { get; set; }
		public Space Space { get; set; }
		public World2D World2D { get; set; }
		public World World3D { get; set; }
		public ModelBatch ModelBatch => batch;

		// In this context, "user data" means custom data optionally loaded with each fragment. Used as needed in order
		// to implement custom features for different kinds of locations.
		public Dictionary<string, object> UserData { get; }

		public void LoadFragment(string filename)
		{
			var fragment = SceneFragment.Load(filename);
			fragments.Add(fragment);
			batch.Add(fragment.MapModel);
			World3D.AddBody(fragment.MapBody);
		}

		public void UnloadFragment()
		{
		}

		public void Add(Entity entity)
		{
			entities[(int)entity.Group].Add(entity);
			entity.Initialize(this);
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
			batch.ViewProjection = camera.ViewProjection;
			batch.Draw();
		}
	}
}
