using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EdgeSet : List<Edge> {

	public EdgeSet clone()
    {
        EdgeSet clone = new EdgeSet();
        foreach (Edge edge in this)
            clone.Add(edge.clone());
        return clone;
    }

    public List<int> GetUniqueVertices()
    {
        List<int> vertices = new List<int>();
        foreach (Edge edge in this)
        {
            foreach (int vert in edge.m_sharedVertices)
            {
                if (!vertices.Contains(vert))
                    vertices.Add(vert);
            }
        }
        return vertices;
    }
}
