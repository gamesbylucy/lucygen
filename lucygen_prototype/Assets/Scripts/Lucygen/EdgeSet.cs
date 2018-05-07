using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EdgeSet : List<Edge> {

	public EdgeSet Clone()
    {
        EdgeSet clone = new EdgeSet();
        foreach (Edge edge in this)
            clone.Add(edge.Clone());
        return clone;
    }

    public List<int> GetUniqueVertices()
    {
        List<int> vertices = new List<int>();
        foreach (Edge edge in this)
        {
            foreach (int vert in edge.mli_sharedVertices)
            {
                if (!vertices.Contains(vert))
                    vertices.Add(vert);
            }
        }
        return vertices;
    }
}
