using System.Linq;
using Engine.Graphics._3D.Loaders;
using GlmSharp;

namespace Engine.Graphics._3D
{
	public class Mesh
	{
		public const string Path = "Content/Meshes/";

		public static Mesh Load(string filename)
		{
			return filename.EndsWith(".obj") ? ObjLoader.Load(filename) : DaeLoader.Load(filename);
		}

		public Mesh(vec3[] points, vec2[] source, vec3[] normals, ivec3[] vertices, ushort[] indices, string texture,
			ivec2[] boneIndexes = null, vec2[] boneWeights = null)
		{
			float minX = points.Min(p => p.x);
			float maxX = points.Max(p => p.x);
			float minY = points.Min(p => p.y);
			float maxY = points.Max(p => p.y);
			float minZ = points.Min(p => p.z);
			float maxZ = points.Max(p => p.z);
			float width = maxX - minX;
			float height = maxY - minY;
			float depth = maxZ - minZ;

			Points = points;
			Source = source;
			Normals = normals;
			Vertices = vertices;
			Indices = indices;
			MaxIndex = indices.Max();
			Bounds = new vec3(width, height, depth);
			Origin = new vec3(minX, minY, minZ);
			BoneIndexes = boneIndexes;
			BoneWeights = boneWeights;
			Texture = ContentCache.GetTexture(texture ?? "Grey.png");
		}

		public vec3[] Points { get; }
		public vec2[] Source { get; }
		public vec3[] Normals { get; }
		public ivec3[] Vertices { get; }

		// These fields help when creating shapes around meshes (such as physics bodies or sensors).
		public vec3 Bounds { get; }
		public vec3 Origin { get; }

		// These two arrays will be left null for non-animated meshes.
		public ivec2[] BoneIndexes { get; }
		public vec2[] BoneWeights { get; }

		public ushort[] Indices { get; }
		public ushort MaxIndex { get; }

		public Texture Texture { get; }
		public MeshHandle Handle { get; set; }
	}
}
