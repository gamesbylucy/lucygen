using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Edge {

    public Polygon m_innerPolygon;
    public Polygon m_outerPolygon;

    public List<int> m_sharedVertices;

    public Edge(Polygon innerPolygon, Polygon outerPolygon)
    {
        m_innerPolygon = innerPolygon;
        m_outerPolygon = outerPolygon;
        m_sharedVertices = new List<int>();

        foreach(int vertex in innerPolygon.m_vertices)
        {
            if (outerPolygon.m_vertices.Contains(vertex))
                m_sharedVertices.Add(vertex);
        }
    }

    public Edge clone()
    {
        return new Edge(m_innerPolygon, m_outerPolygon);
    }
}
