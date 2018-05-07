using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon {

    public List<int> mli_vertices;
    public List<Polygon> ml_neighborPolygons;
    public int[] mli_meshTriangleVertices;

    public Polygon(int vertA, int vertB, int vertC)
    {
        mli_vertices = new List<int>() { vertA, vertB, vertC };
        ml_neighborPolygons = new List<Polygon>();
        mli_meshTriangleVertices = new int[3] { vertA, vertB, vertC };
    }

    //determines that this polygon is a neighbor of the arg if the polys have
    //more than 2 shared vertices
    public bool IsNeighborOf(Polygon potentialNeighbor)
    {
        bool result = false;

        int sharedVertices = 0;

        foreach (int vertex in mli_vertices)
        {
            if (potentialNeighbor.mli_vertices.Contains(vertex))
                sharedVertices++;
        }

        return sharedVertices == 2;
    }

    public void ReplaceNeighbor(Polygon oldPoly, Polygon newPoly)
    {
        for (int i = 0; i < ml_neighborPolygons.Count; i++)
        {
            if (oldPoly == ml_neighborPolygons[i])
            {
                ml_neighborPolygons[i] = newPoly;
                return;
            }
        }
    }

    public void FlipMeshTriangle()
    {
        int temp = mli_meshTriangleVertices[1];
        mli_meshTriangleVertices[1] = mli_meshTriangleVertices[2];
        mli_meshTriangleVertices[2] = temp;
    }
}
