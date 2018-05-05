using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Polygon {

    public List<int> m_vertices;
    public List<Polygon> m_neighbors;

    public Polygon(int a, int b, int c)
    {
        m_vertices = new List<int>() { a, b, c };
        m_neighbors = new List<Polygon>();
    }

    public void assignFace(int a, int b, int c)
    {
        m_vertices = new List<int>() { a, b, c };
    }

    //determines that this polygon is a neighbor of the arg if the polys have
    //more than 2 shared vertices
    public bool isNeighborOf(Polygon other_poly)
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

    public void ReplaceNeighbor(Polygon oldPoly, Polygon newPoly)
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
