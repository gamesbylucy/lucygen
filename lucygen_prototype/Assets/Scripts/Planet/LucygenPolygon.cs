using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LucygenPolygon {

    public List<int> m_vertices;
    public List<LucygenPolygon> m_neighbors;

    public LucygenPolygon(int a, int b, int c)
    {
        m_vertices = new List<int>() { a, b, c };
        m_neighbors = new List<LucygenPolygon>();
    }

    public void assignFace(int a, int b, int c)
    {
        m_vertices = new List<int>() { a, b, c };
    }

    //determines that this polygon is a neighbor of the arg if the polys have
    //more than 2 shared vertices
    public bool isNeighborOf(LucygenPolygon other_poly)
    {
        bool result = false;

        int shared_vertices = 0;

        foreach (int vertex in m_vertices)
        {
            if (other_poly.m_vertices.Contains(vertex))
                shared_vertices++;
        }

        return shared_vertices == 2;
    }

    public void ReplaceNeighbor(LucygenPolygon oldPoly, LucygenPolygon newPoly)
    {
        for (int i = 0; i < m_neighbors.Count; i++)
        {
            if (oldPoly == m_neighbors[i])
            {
                m_neighbors[i] = newPoly;
                return;
            }
        }
    }
}
