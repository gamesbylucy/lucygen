using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonSet : List<Polygon> {

	public EdgeSet CreateEdgeSet()
    {
        EdgeSet edgeSet = new EdgeSet();

        foreach(Polygon polygon in this)
        {
            foreach (Polygon neighbor in polygon.ml_neighborPolygons)
            {
                if (this.Contains(neighbor))
                    continue;

                Edge edge = new Edge(polygon, neighbor);
                edgeSet.Add(edge);
            }
        }
        return edgeSet;
    }

    public List<int> GetUniqueVertices()
    {
        List<int> verts = new List<int>();

        foreach (Polygon poly in this)
        {
            foreach (int vert in poly.mli_vertices)
            {
                if (!verts.Contains(vert))
                    verts.Add(vert);
            }
        }

        return verts;
    }

    public void CalculatePolygonNeighbors()
    {
        foreach (Polygon polygon in this)
        {
            foreach (Polygon otherPolygon in this)
            {
                if (polygon == otherPolygon)
                    continue;
                if (polygon.IsNeighborOf(otherPolygon))
                {
                    polygon.ml_neighborPolygons.Add(otherPolygon);
                }
            }
        }
    }

    public PolygonSet GetRandomClusterSet(int clusterSize)
    {
        System.Random random = new System.Random();
        PolygonSet clusterSet = new PolygonSet();

        clusterSet.Add(this[random.Next(0, this.Count - 1)]);

        Polygon current = clusterSet[0];

        for (int i = 0; i < clusterSize; i++)
        {
            clusterSet.Add(current.ml_neighborPolygons[random.Next(0, current.ml_neighborPolygons.Count - 1)]);
            current = clusterSet[i + 1];
        }

        return clusterSet;
    }
}
