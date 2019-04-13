using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlmSharp;

namespace Engine.Graphics
{
	public class Mesh
	{
		public static Mesh Load(string filename)
		{
			string[] lines = File.ReadAllLines("Content/Models/" + filename);

			Texture texture = lines[0].Length > 0 ? ContentCache.GetTexture(lines[0]) : null;

			vec3[] points = ParseVec3s(lines[1]);
			vec2[] source = ParseVec2s(lines[2]);
			vec3[] normals = ParseVec3s(lines[3]);

			// Parse vertices.
			string[] tokens = lines[4].Split(',');

			ivec3[] vertices = new ivec3[tokens.Length / 3];

			for (int i = 0; i < tokens.Length; i += 3)
			{
				int pointIndex = int.Parse(tokens[i]);
				int sourceIndex = int.Parse(tokens[i + 1]);
				int normalIndex = int.Parse(tokens[i + 2]);

				vertices[i / 3] = new ivec3(pointIndex, sourceIndex, normalIndex);
			}

			// Parse indices.
			tokens = lines[5].Split(',');

			ushort[] indices = new ushort[tokens.Length];

			for (int i = 0; i < tokens.Length; i++)
			{
				indices[i] = ushort.Parse(tokens[i]);
			}

			return new Mesh(points, source, normals, vertices, indices, texture);
		}

		private static vec2[] ParseVec2s(string line)
		{
			string[] tokens = line.Split(',');

			vec2[] array = new vec2[tokens.Length / 3];

			for (int i = 0; i < tokens.Length; i += 2)
			{
				float x = float.Parse(tokens[i]);
				float y = float.Parse(tokens[i + 1]);

				array[i / 2] = new vec2(x, y);
			}

			return array;
		}

		private static vec3[] ParseVec3s(string line)
		{
			string[] tokens = line.Split(',');

			vec3[] array = new vec3[tokens.Length / 3];

			for (int i = 0; i < tokens.Length; i += 3)
			{
				float x = float.Parse(tokens[i]);
				float y = float.Parse(tokens[i + 1]);
				float z = float.Parse(tokens[i + 2]);

				array[i / 3] = new vec3(x, y, z);
			}

			return array;
		}

		public Mesh(vec3[] points, vec2[] source, vec3[] normals, ivec3[] vertices, ushort[] indices, Texture texture)
		{
			Points = points;
			Source = source;
			Normals = normals;
			Vertices = vertices;
			Indices = indices;
			Texture = texture;
		}

		public vec3[] Points { get; }
		public vec2[] Source { get; }
		public vec3[] Normals { get; }
		public ivec3[] Vertices { get; }
		public ushort[] Indices { get; }

		public Texture Texture { get; }
	}
}
