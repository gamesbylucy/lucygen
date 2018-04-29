using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LucygenEdge {

    public LucygenPolygon m_innerPolygon;
    public LucygenPolygon m_outerPolygon;

    public List<int> m_sharedVertices;

    public LucygenEdge(LucygenPolygon innerPolygon, LucygenPolygon outerPolygon)
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

    public LucygenEdge clone()
    {
        LucygenPolygon cloneInnerPolygon = new LucygenPolygon(m_innerPolygon.m_vertices[0], m_innerPolygon.m_vertices[1], m_innerPolygon.m_vertices[2]);
        LucygenPolygon cloneOuterPolgon = new LucygenPolygon(m_outerPolygon.m_vertices[0], m_outerPolygon.m_vertices[1], m_outerPolygon.m_vertices[2]);
        LucygenEdge cloneEdge = new LucygenEdge(cloneInnerPolygon, cloneOuterPolgon);
        return cloneEdge;
    }
}
