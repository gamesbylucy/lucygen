using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Edge {

    public Polygon m_innerPolygon;
    public Polygon m_outerPolygon;

    public List<int> mli_sharedVertices;

    public Edge(Polygon innerPolygon, Polygon outerPolygon)
    {
        m_innerPolygon = innerPolygon;
        m_outerPolygon = outerPolygon;
        mli_sharedVertices = new List<int>();

        foreach(int vertex in innerPolygon.mli_vertices)
        {
            if (outerPolygon.mli_vertices.Contains(vertex))
                mli_sharedVertices.Add(vertex);
        }
    }

    public Edge Clone()
    {
        return new Edge(m_innerPolygon, m_outerPolygon);
    }
}
