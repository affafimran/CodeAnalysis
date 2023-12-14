using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NullPointerGame.Extras
{
	public static class MeshUtilities
	{
		/// <summary>
		/// Creates a Cylinder primitive and returns it as a mesh.
		/// </summary>
		/// <param name="radius">Radius of the cylinder.</param>
		/// <param name="height">Height of the cylinder.</param>
		/// <param name="sides">Number of sides of the cylinder.</param>
		/// <returns></returns>
		public static Mesh CreateCylinder(float radius, float height, int sides=18)
		{
			return CreateCone(radius, radius, height, sides);
		}

		/// <summary>
		/// Creates a cone mesh primitive. The code isn't mine, proper credits needs to be shared:
		/// Author: Bérenger.
		/// Source: http://wiki.unity3d.com/index.php/ProceduralPrimitives
		/// </summary>
		/// <param name="bottomRadius">Radius of the base of the cone</param>
		/// <param name="topRadius">Radius of the top of the cone</param>
		/// <param name="height">Height of the cone.</param>
		/// <param name="sides">Number of sides of the cone</param>
		/// <returns></returns>
		public static Mesh CreateCone(float bottomRadius, float topRadius, float height, int sides=18)
		{
			Mesh mesh = new Mesh();
			int nbHeightSeg = 1; // Not implemented yet
			int nbVerticesCap = sides + 1;
		
			#region Vertices
			// bottom + top + sides
			Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + sides * nbHeightSeg * 2 + 2];
			int vert = 0;
			float _2pi = Mathf.PI * 2f;
 
			// Bottom cap
			vertices[vert++] = new Vector3(0f, 0f, 0f);
			while( vert <= sides )
			{
				float rad = (float)vert / sides * _2pi;
				vertices[vert] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius);
				vert++;
			}
 
			// Top cap
			vertices[vert++] = new Vector3(0f, height, 0f);
			while (vert <= sides * 2 + 1)
			{
				float rad = (float)(vert - sides - 1)  / sides * _2pi;
				vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
				vert++;
			}
 
			// Sides
			int v = 0;
		
			while (vert <= vertices.Length - 4 )
			{
				float rad = (float)v / sides * _2pi;
				vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
				vertices[vert + 1] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius);
				vert+=2;
				v++;
			}
			vertices[vert] = vertices[ sides * 2 + 2 ];
			vertices[vert + 1] = vertices[sides * 2 + 3 ];
			#endregion
 
			#region Normales
 
			// bottom + top + sides
			Vector3[] normales = new Vector3[vertices.Length];
			vert = 0;
 
			// Bottom cap
			while( vert  <= sides )
			{
				normales[vert++] = Vector3.down;
			}
 
			// Top cap
			while( vert <= sides * 2 + 1 )
			{
				normales[vert++] = Vector3.up;
			}
		
			// Sides
			v = 0;
			while (vert <= vertices.Length - 4 )
			{			
				float rad = (float)v / sides * _2pi;
				float cos = Mathf.Cos(rad);
				float sin = Mathf.Sin(rad);
 
				normales[vert] = new Vector3(cos, 0f, sin);
				normales[vert+1] = normales[vert];
 
				vert+=2;
				v++;
			}
			normales[vert] = normales[ sides * 2 + 2 ];
			normales[vert + 1] = normales[sides * 2 + 3 ];
		
			#endregion
 
			#region UVs
			Vector2[] uvs = new Vector2[vertices.Length];
 
			// Bottom cap
			int u = 0;
			uvs[u++] = new Vector2(0.5f, 0.5f);
			while (u <= sides)
			{
				float rad = (float)u / sides * _2pi;
				uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
				u++;
			}
 
			// Top cap
			uvs[u++] = new Vector2(0.5f, 0.5f);
			while (u <= sides * 2 + 1)
			{
				float rad = (float)u / sides * _2pi;
				uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
				u++;
			}
 
			// Sides
			int u_sides = 0;
			while (u <= uvs.Length - 4 )
			{
				float t = (float)u_sides / sides;
				uvs[u] = new Vector3(t, 1f);
				uvs[u + 1] = new Vector3(t, 0f);
				u += 2;
				u_sides++;
			}
			uvs[u] = new Vector2(1f, 1f);
			uvs[u + 1] = new Vector2(1f, 0f);
			#endregion 
 
			#region Triangles
			int nbTriangles = sides + sides + sides*2;
			int[] triangles = new int[nbTriangles * 3 + 3];
 
			// Bottom cap
			int tri = 0;
			int i = 0;
			while (tri < sides - 1)
			{
				triangles[ i ] = 0;
				triangles[ i+1 ] = tri + 1;
				triangles[ i+2 ] = tri + 2;
				tri++;
				i += 3;
			}
			triangles[i] = 0;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = 1;
			tri++;
			i += 3;
 
			// Top cap
			//tri++;
			while (tri < sides*2)
			{
				triangles[ i ] = tri + 2;
				triangles[i + 1] = tri + 1;
				triangles[i + 2] = nbVerticesCap;
				tri++;
				i += 3;
			}
 
			triangles[i] = nbVerticesCap + 1;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = nbVerticesCap;		
			tri++;
			i += 3;
			tri++;
 
			// Sides
			while( tri <= nbTriangles )
			{
				triangles[ i ] = tri + 2;
				triangles[ i+1 ] = tri + 1;
				triangles[ i+2 ] = tri + 0;
				tri++;
				i += 3;
 
				triangles[ i ] = tri + 1;
				triangles[ i+1 ] = tri + 2;
				triangles[ i+2 ] = tri + 0;
				tri++;
				i += 3;
			}
			#endregion
 
			mesh.vertices = vertices;
			mesh.normals = normales;
			mesh.uv = uvs;
			mesh.triangles = triangles;
 
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			return mesh;
		}

		public static Mesh CreateCircle(float radius, int sides)
		{
			if(sides<3) sides=3;
			Mesh mesh = new Mesh();
			//int nbVerticesCap = sides + 1;
		
			#region Vertices
			Vector3[] vertices = GenerateCircleVertices(radius, sides);
			float _2pi = Mathf.PI * 2f;
			#endregion
 
			#region Normals
			Vector3[] normals = new Vector3[vertices.Length];
			int vert = 0;
			while( vert < vertices.Length )
				normals[vert++] = Vector3.up;
			#endregion
 
			#region UVs
			Vector2[] uvs = new Vector2[vertices.Length];
 
			// Top cap
			int u = 0;
			uvs[u++] = new Vector2(0.5f, 0.5f);
			while (u <= sides)
			{
				float rad = (float)u / sides * _2pi;
				uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
				u++;
			}
			#endregion 
 
			#region Triangles
			int nbTriangles = sides;
			int[] triangles = new int[nbTriangles * 3 + 3];
 
			int tri = 0;
			int i = 0;
			while (tri < sides - 1)
			{
				triangles[ i ] = tri + 2;
				triangles[i + 1] = tri + 1;
				triangles[i + 2] = 0;
				tri++;
				i += 3;
			}
 
			triangles[i] = 0 + 1;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = 0;		
			#endregion
 
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uvs;
			mesh.triangles = triangles;
 
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();

			return mesh;
		}

		/// <summary>
		/// Helper method useful to generate the points of a centred circle shape.
		/// </summary>
		/// <param name="radius">Radius of the circle.</param>
		/// <param name="sides">Number of sides for the circle.</param>
		/// <returns>An array of point that forms the circle shape.</returns>
		public static Vector3[] GenerateCircleVertices(float radius, int sides)
		{
			Vector3[] vertices = new Vector3[sides + 1];
			int vert = 0;
			float _2pi = Mathf.PI * 2f;
 
			// Top cap
			vertices[vert++] = new Vector3(0f, 0.0f, 0f);
			while (vert <= sides )
			{
				float rad = (float)(vert - sides - 1)  / sides * _2pi;
				vertices[vert] = new Vector3(Mathf.Cos(rad) * radius, 0.0f, Mathf.Sin(rad) * radius);
				vert++;
			}
			return vertices;
		}

		/// <summary>
		/// A simple method to generate the array of points required for a centred box shape.
		/// if size.y == 0.0f it will return just the 4 vertices needed for the plane construction.
		/// if size.y != 0.0f then it will return 8 vertices forming the box shape.
		/// </summary>
		/// <param name="size"></param>
		/// <param name="center"></param>
		/// <returns></returns>
		public static Vector3[] GenerateBoxVertices(Vector3 size, Vector3 center)
		{
			Vector3[] vertices = new Vector3[size.y!=0.0f ? 8 : 4];
			vertices[0] = center + new Vector3(-size.x/2, size.y/2, -size.z/2);
			vertices[1] = center + new Vector3(-size.x/2, size.y/2, size.z/2);
			vertices[2] = center + new Vector3(size.x/2, size.y/2, -size.z/2);
			vertices[3] = center + new Vector3(size.x/2, size.y/2, size.z/2);
			if(size.y!=0.0f)
			{
				vertices[4] = center + new Vector3(-size.x/2, -size.y/2, -size.z/2);
				vertices[5] = center + new Vector3(-size.x/2, -size.y/2, size.z/2);
				vertices[6] = center + new Vector3(size.x/2, -size.y/2, -size.z/2);
				vertices[7] = center + new Vector3(size.x/2, -size.y/2, size.z/2);
			}
			return vertices;
		}

		public static Vector3 GetClosestPoint(this Vector3 referencePoint, IEnumerable<Vector3> points )
		{
			return points.OrderBy(x => (x-referencePoint).sqrMagnitude).FirstOrDefault();
		}
	}
}