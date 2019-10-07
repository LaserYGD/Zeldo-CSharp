using System;
using System.Collections.Generic;
using System.IO;
using Engine;
using Jitter.Collision;
using Jitter.Collision.Shapes;
using Jitter.LinearMath;

namespace Zeldo.Physics
{
	public static class TriangleMeshLoader
	{
		public static TriangleMeshShape Load(string filename)
		{
			int lineIndex = 2;

			// This function is very similar to the main mesh loading function, but physics meshes generally contain
			// less data (fewer vertices and no texture coordinates). Physics meshes also aren't cached in the same way
			// as regular models (since triangle meshes tend to be loaded in large chunks infrequently as the player
			// moves around the world).
			string[] lines = File.ReadAllLines(Paths.Meshes + filename);
			string line = lines[lineIndex];

			// Parse points.
			List<JVector> points = new List<JVector>();

			do
			{
				points.Add(ParseJVector(line));
				line = lines[++lineIndex];
			}
			while (line[0] == 'v' && line[1] != 'n');

			// Normals may or may not be present in the exported .obj file.
			while (line[1] == 'n')
			{
				line = lines[++lineIndex];
			}

			// The next line is smoothing ("s off"), which isn't relevant.
			lineIndex++;

			// Parse triangles. All remaining lines should be faces.
			List<TriangleVertexIndices> tris = new List<TriangleVertexIndices>();

			do
			{
				// With only indices exported, each face line looks like "f 1 2 3".
				string[] tokens = lines[lineIndex++].Split(' ');

				// If normals were exported, each token will look like "1/2/3" instead. Also note that assigning
				// indices in the opposite order (2-1-0 rather than 0-1-2) causes Jitter to process triangles in the
				// correct direction (without having to flip normals in Blender).
				int i2 = int.Parse(tokens[1].Split('/')[0]) - 1;
				int i1 = int.Parse(tokens[2].Split('/')[0]) - 1;
				int i0 = int.Parse(tokens[3].Split('/')[0]) - 1;

				tris.Add(new TriangleVertexIndices(i0, i1, i2));
			}
			while (lineIndex < lines.Length);

			var octree = new Octree(points, tris);
			octree.BuildOctree();

			// TODO: Lists are attached to the triangle mesh for debug rendering. They should be removed later.
			var shape = new TriangleMeshShape(octree);
			shape.Tag = new Tuple<List<JVector>, List<TriangleVertexIndices>>(points, tris);

			return shape;
		}
		
		public static JVector ParseJVector(string line)
		{
			string[] tokens = line.Split(' ');

			float x = float.Parse(tokens[1]);
			float y = float.Parse(tokens[2]);
			float z = float.Parse(tokens[3]);

			return new JVector(x, y, z);
		}
	}
}
