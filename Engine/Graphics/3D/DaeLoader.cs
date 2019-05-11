using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Engine.Utility;
using GlmSharp;

namespace Engine.Graphics._3D
{
	public static class DaeLoader
	{
		public static Mesh Load(string filename)
		{
			XDocument document = XDocument.Load(Mesh.Path + filename);
			XElement geometryElement = document.Root.Local("library_geometries").Local("geometry");
			XElement meshElement = geometryElement.Local("mesh");
			XElement[] sourceElements = meshElement.Locals("source").ToArray();

			// Source elements are identified using the mesh ID (e.g. Cube-mesh-positions or Cube-mesh-normals).
			string id = geometryElement.Attribute("id").Value;

			XElement GetSourceById(string value) => sourceElements.First(e =>
				e.Attribute("id").Value == $"{id}-{value}");

			// Parse points and normals.
			vec3[] points = ParseVec3Source(GetSourceById("positions"));
			vec3[] normals = ParseVec3Source(GetSourceById("normals"));

			// Parse the rest.
			string rawList = meshElement.Local("polylist").Local("p").Value;
			string[] tokens = rawList.Split(' ');

			int[] rawIndices = tokens.Select(int.Parse).ToArray();

			vec2[] source = { vec2.Zero };
			ivec3[] vertices = new ivec3[rawIndices.Length / 2];

			for (int i = 0; i < rawIndices.Length; i += 2)
			{
				// This assumes that the VERTEX (i.e. POSITION) offset will always be zero and the NORMAL offset will
				// always be one.
				int pointIndex = rawIndices[i];
				int normalIndex = rawIndices[i + 1];

				vertices[i / 2] = new ivec3(pointIndex, 0, normalIndex);
			}

			ushort[] indices = new ushort[vertices.Length];

			for (int i = 0; i < indices.Length; i++)
			{
				indices[i] = (ushort)i;
			}

			return new Mesh(points, source, normals, vertices, indices, null);
		}

		private static vec3[] ParseVec3Source(XElement source)
		{
			XElement arrayElement = source.Local("float_array");

			string[] tokens = arrayElement.Value.Split(' ');

			vec3[] points = new vec3[tokens.Length / 3];

			for (int i = 0; i < points.Length; i++)
			{
				int start = i * 3;

				float x = float.Parse(tokens[start]);
				float y = float.Parse(tokens[start + 1]);
				float z = float.Parse(tokens[start + 2]);

				points[i] = new vec3(x, y, z);
			}

			return points;
		}
	}
}
