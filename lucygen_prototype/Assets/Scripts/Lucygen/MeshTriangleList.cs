using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangleList : List<MeshTriangle> {
    public int[] GenerateMeshTriangles()
    {
        int[] result = new int[this.Count * 3];
        
        for (int i = 0; i < this.Count; i++)
        {
            for (int j = 0; j < this[i].mli_vertices.Length; j++)
            {
                result[i] = this[i].mli_vertices[j];
            }
        }

        return result;
    }
}
