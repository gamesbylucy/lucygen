using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EdgeSet : List<Edge> {

	public EdgeSet Clone()
    {
        EdgeSet clone = new EdgeSet();
        foreach (Edge edge in this)
        {
            Edge cloneEdge = new Edge(edge.m_innerPolygon, edge.m_outerPolygon);
            cloneEdge.mli_sharedVertices[0] = edge.mli_sharedVertices[0];
            cloneEdge.mli_sharedVertices[1] = edge.mli_sharedVertices[1];
            clone.Add(cloneEdge);
        }
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
