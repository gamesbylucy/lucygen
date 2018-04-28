using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using System.Linq;
using Assets.Map;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeSphereExperimental : MonoBehaviour {

    public int gridSize;
	public float radius = 1f;

	private Mesh mesh;
	private Vector3[] vertices;
	private Vector3[] normals;

	private void Awake () {
		Generate();
	}

	private void Generate () {
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();
		mesh.name = "Procedural Sphere";
		CreateVertices();
		CreateTriangles();
		CreateColliders();
    }

	private void CreateVertices () {

		int cornerVertices = 8;

		int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;

		int faceVertices = (
			(gridSize - 1) * (gridSize - 1) +
			(gridSize - 1) * (gridSize - 1) +
			(gridSize - 1) * (gridSize - 1)) * 2;

		vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];

		normals = new Vector3[vertices.Length];

        Mesh mesh = GetComponent<MeshFilter>().mesh;

        int v = 0;

        //SET SIDES OF CUBE
		for (int y = 0; y <= gridSize; y++) {
			for (int x = 0; x <= gridSize; x++) {
				SetVertex(v++, x, y, 0);
			}
			for (int z = 1; z <= gridSize; z++) {
				SetVertex(v++, gridSize, y, z);
			}
			for (int x = gridSize - 1; x >= 0; x--) {
				SetVertex(v++, x, y, gridSize);
			}
			for (int z = gridSize - 1; z > 0; z--) {
				SetVertex(v++, 0, y, z);
			}
		}

        //SET TOP OF CUBE
		for (int z = 1; z < gridSize; z++) {
			for (int x = 1; x < gridSize; x++) {
				SetVertex(v++, x, gridSize, z);
			}
		}

        //SET BOTTOM OF CUBE
		for (int z = 1; z < gridSize; z++) {
			for (int x = 1; x < gridSize; x++) {
				SetVertex(v++, x, 0, z);
			}
		}

        //assigns the vertices to the mesh
        mesh.vertices = vertices;
		mesh.normals = normals;
    }

	private void SetVertex (int i, int x, int y, int z) {

        //CONVERT THE COORDINATES TO A VECTOR3 POINT IN 3D SPACE ON A UNIT CUBE
        //The unit cube has 8 quadrants, all permutations on positive and negative
        //values of the x, y, and z axis. The new vector 3 is scaled to twice the
        //input value, which is the side length of the unit cube containing the
        //resultant cubesphere. The elements of the vector are then divided by
        //the grid size, which scales the components of the vector down to whatever
        //the grid size has been determined to be. Finally, vector [1,1,1] is
        //subtracted from the resultant vector to move the point to its proper
        //location on the surface of the unit cube. If [1,1,1] was not subtracted
        //from the vector, the resulting cube would be completely in positive vector
        //space and the origin of the cubesphere would not be on the world origin.

		Vector3 v = new Vector3(x, y, z) * 2f / gridSize - Vector3.one;

        //MUTATE THE VECTOR3 TO FORM THE POINT ON THE CUBESPHERE
        //
		float x2 = v.x * v.x;
		float y2 = v.y * v.y;
		float z2 = v.z * v.z;
		Vector3 s;
		s.x = v.x * Mathf.Sqrt(1f - y2 / 2f - z2 / 2f + y2 * z2 / 3f);
		s.y = v.y * Mathf.Sqrt(1f - x2 / 2f - z2 / 2f + x2 * z2 / 3f);
		s.z = v.z * Mathf.Sqrt(1f - x2 / 2f - y2 / 2f + x2 * y2 / 3f);
		normals[i] = s;
		vertices[i] = normals[i] * radius;
    }

	private void CreateTriangles () {
		int[] trianglesZ = new int[(gridSize * gridSize) * 12];
		int[] trianglesX = new int[(gridSize * gridSize) * 12];
		int[] trianglesY = new int[(gridSize * gridSize) * 12];
		int ring = (gridSize + gridSize) * 2;
		int tZ = 0, tX = 0, tY = 0, v = 0;

		for (int y = 0; y < gridSize; y++, v++) {
			for (int q = 0; q < gridSize; q++, v++) {
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < gridSize; q++, v++) {
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < gridSize; q++, v++) {
				tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
			}
			for (int q = 0; q < gridSize - 1; q++, v++) {
				tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
			}
			tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
		}

		tY = CreateTopFace(trianglesY, tY, ring);
		tY = CreateBottomFace(trianglesY, tY, ring);

		mesh.subMeshCount = 3;
		mesh.SetTriangles(trianglesZ, 0);
		mesh.SetTriangles(trianglesX, 1);
		mesh.SetTriangles(trianglesY, 2);
	}

	private int CreateTopFace (int[] triangles, int t, int ring) {
		int v = ring * gridSize;
		for (int x = 0; x < gridSize - 1; x++, v++) {
			t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
		}
		t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

		int vMin = ring * (gridSize + 1) - 1;
		int vMid = vMin + 1;
		int vMax = v + 2;

		for (int z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++) {
			t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + gridSize - 1);
			for (int x = 1; x < gridSize - 1; x++, vMid++) {
				t = SetQuad(
					triangles, t,
					vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
			}
			t = SetQuad(triangles, t, vMid, vMax, vMid + gridSize - 1, vMax + 1);
		}

		int vTop = vMin - 2;
		t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
		for (int x = 1; x < gridSize - 1; x++, vTop--, vMid++) {
			t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
		}
		t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);

		return t;
	}

	private int CreateBottomFace (int[] triangles, int t, int ring) {
		int v = 1;
		int vMid = vertices.Length - (gridSize - 1) * (gridSize - 1);
		t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
		for (int x = 1; x < gridSize - 1; x++, v++, vMid++) {
			t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
		}
		t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

		int vMin = ring - 2;
		vMid -= gridSize - 2;
		int vMax = v + 2;

		for (int z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++) {
			t = SetQuad(triangles, t, vMin, vMid + gridSize - 1, vMin + 1, vMid);
			for (int x = 1; x < gridSize - 1; x++, vMid++) {
				t = SetQuad(
					triangles, t,
					vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
			}
			t = SetQuad(triangles, t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
		}

		int vTop = vMin - 1;
		t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
		for (int x = 1; x < gridSize - 1; x++, vTop--, vMid++) {
			t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
		}
		t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

		return t;
	}

	private static int
	SetQuad (int[] triangles, int i, int v00, int v10, int v01, int v11) {
		triangles[i] = v00;
		triangles[i + 1] = triangles[i + 4] = v01;
		triangles[i + 2] = triangles[i + 3] = v10;
		triangles[i + 5] = v11;
		return i + 6;
	}

	private void CreateColliders () {
		gameObject.AddComponent<SphereCollider>();
	}

//	private void OnDrawGizmos () {
//		if (vertices == null) {
//			return;
//		}
//		for (int i = 0; i < vertices.Length; i++) {
//			Gizmos.color = Color.black;
//			Gizmos.DrawSphere(vertices[i], 0.1f);
//			Gizmos.color = Color.yellow;
//			Gizmos.DrawRay(vertices[i], normals[i]);
//		}
//	}
}