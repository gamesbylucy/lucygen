using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle {

    public int mi_vertA, mi_vertB, mi_vertC;

    public int[] mli_vertices;

    public MeshTriangle(int a, int b, int c)
    {
        mi_vertA = a;
        mi_vertB = b;
        mi_vertC = c;
        mli_vertices = new int[] { a, b, c };
    }

    public MeshTriangle(Polygon poly)
    {
        mi_vertA = poly.mli_vertices[0];
        mi_vertB = poly.mli_vertices[1];
        mi_vertC = poly.mli_vertices[2];
        mli_vertices = new int[] { poly.mli_vertices[0], poly.mli_vertices[1], poly.mli_vertices[2] };
    }
}
