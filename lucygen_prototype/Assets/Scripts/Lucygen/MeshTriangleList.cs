using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangleList : List<MeshTriangle> {
    public int[] generateMeshTriangles()
    {
        int[] result = new int[this.Count * 3];
        
        for (int i = 0; i < this.Count; i++)
        {
            for (int j = 0; j < this[i].m_vertices.Length; j++)
            {
                result[i] = this.[i].m_vertices[j];
            }
        }

        return result;
    }
}
