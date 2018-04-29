using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LucygenEdgeSet : List<LucygenEdge> {

	public LucygenEdgeSet clone()
    {
        LucygenEdgeSet clone = new LucygenEdgeSet();
        foreach (LucygenEdge edge in this)
            clone.Add(edge.clone());
        return clone;
    }

    public List<int> GetUniqueVertices()
    {
        List<int> vertices = new List<int>();
        foreach (LucygenEdge edge in this)
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
