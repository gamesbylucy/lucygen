using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonSet : List<Polygon> {

	public EdgeSet createEdgeSet()
    {
        EdgeSet edgeSet = new EdgeSet();

        foreach(Polygon polygon in this)
        {
            foreach (Polygon neighbor in polygon.m_neighbors)
            {
                if (this.Contains(neighbor))
                    continue;

                Edge edge = new Edge(polygon, neighbor);
                edgeSet.Add(edge);
            }
        }
        return edgeSet;
    }

    public List<int> getUniqueVertices()
    {
        List<int> verts = new List<int>();

        foreach(Polygon poly in this)
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
