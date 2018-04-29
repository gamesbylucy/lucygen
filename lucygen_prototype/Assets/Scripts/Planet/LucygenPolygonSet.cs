using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LucygenPolygonSet : List<LucygenPolygon> {

	public LucygenEdgeSet createEdgeSet()
    {
        LucygenEdgeSet edgeSet = new LucygenEdgeSet();

        foreach(LucygenPolygon polygon in this)
        {
            foreach (LucygenPolygon neighbor in polygon.m_neighbors)
            {
                if (this.Contains(neighbor))
                    continue;

                LucygenEdge edge = new LucygenEdge(polygon, neighbor);
                edgeSet.Add(edge);
            }
        }
        return edgeSet;
    }

    public List<int> getUniqueVertices()
    {
        List<int> verts = new List<int>();

        foreach(LucygenPolygon poly in this)
        {
            foreach(int vert in poly.m_vertices)
            {
                if (!verts.Contains(vert))
                    verts.Add(vert);
            }
        }

        return verts;
    }
}
