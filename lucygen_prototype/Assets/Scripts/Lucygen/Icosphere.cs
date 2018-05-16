using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icosphere
{
    public PolygonSet polygons { get; private set; }
    public List<Vector3> vertices { get; private set; }

    public Icosphere(float size, int recursions)
    {
        Initialize();
        Subdivide(recursions);
        EnlargeVertices(size);
    }

    private void Initialize()
    {
        EventLog.Add("Creating icosohedron");
        polygons = new PolygonSet();
        vertices = new List<Vector3>();

        // Formula for calculating vertex angle? Need to figure out what this does...

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        // Initial 12 points of the icosohedron

        vertices.Add(new Vector3(-1.0f, t, 0).normalized);
        vertices.Add(new Vector3(1.0f, t, 0).normalized);
        vertices.Add(new Vector3(-1.0f, -t, 0).normalized);
        vertices.Add(new Vector3(1.0f, -t, 0).normalized);
        vertices.Add(new Vector3(0, -1.0f, t).normalized);
        vertices.Add(new Vector3(0, 1.0f, t).normalized);
        vertices.Add(new Vector3(0, -1.0f, -t).normalized);
        vertices.Add(new Vector3(0, 1.0f, -t).normalized);
        vertices.Add(new Vector3(t, 0, -1.0f).normalized);
        vertices.Add(new Vector3(t, 0, 1.0f).normalized);
        vertices.Add(new Vector3(-t, 0, -1.0f).normalized);
        vertices.Add(new Vector3(-t, 0, 1.0f).normalized);

        // Initial 20 sides of the icosohedron

        polygons.Add(new Polygon(0, 11, 5));
        polygons.Add(new Polygon(0, 5, 1));
        polygons.Add(new Polygon(0, 1, 7));
        polygons.Add(new Polygon(0, 7, 10));
        polygons.Add(new Polygon(0, 10, 11));
        polygons.Add(new Polygon(1, 5, 9));
        polygons.Add(new Polygon(5, 11, 4));
        polygons.Add(new Polygon(11, 10, 2));
        polygons.Add(new Polygon(10, 7, 6));
        polygons.Add(new Polygon(7, 1, 8));
        polygons.Add(new Polygon(3, 9, 4));
        polygons.Add(new Polygon(3, 4, 2));
        polygons.Add(new Polygon(3, 2, 6));
        polygons.Add(new Polygon(3, 6, 8));
        polygons.Add(new Polygon(3, 8, 9));
        polygons.Add(new Polygon(4, 9, 5));
        polygons.Add(new Polygon(2, 4, 11));
        polygons.Add(new Polygon(6, 2, 10));
        polygons.Add(new Polygon(8, 6, 7));
        polygons.Add(new Polygon(9, 8, 1));
    }

    private void Subdivide(int recursions)
    {
        EventLog.Add("Subdividing surface.");
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < recursions; i++)
        {
            var newPolys = new PolygonSet();

            foreach (Polygon poly in polygons)
            {
                int a = poly.mli_vertices[0];
                int b = poly.mli_vertices[1];
                int c = poly.mli_vertices[2];

                // Use GetMidPointIndex to either create a
                // new vertex between two old vertices, or
                // find the one that was already created.

                int ab = GetMidPointIndex(midPointCache, a, b);
                int bc = GetMidPointIndex(midPointCache, b, c);
                int ca = GetMidPointIndex(midPointCache, c, a);

                // Create the four new polygons using our original
                // three vertices, and the three new midpoints.

                newPolys.Add(new Polygon(a, ab, ca));
                newPolys.Add(new Polygon(b, bc, ab));
                newPolys.Add(new Polygon(c, ca, bc));
                newPolys.Add(new Polygon(ab, bc, ca));
            }

            // Replace all our old polygons with the new set of
            // subdivided ones.
            polygons = newPolys;
        }
    }

    private int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {
        EventLog.Add("Getting mid-point index for " + vertices[indexA] + " and " + vertices[indexB]);

        // We create a key out of the two original indices
        // by storing the smaller index in the upper two bytes
        // of an integer, and the larger index in the lower two
        // bytes. By sorting them according to whichever is smaller
        // we ensure that this function returns the same result
        // whether you call
        // GetMidPointIndex(cache, 5, 9)
        // or...
        // GetMidPointIndex(cache, 9, 5)

        int smallerIndex = Mathf.Min(indexA, indexB);
        int greaterIndex = Mathf.Max(indexA, indexB);
        int key = (smallerIndex << 16) + greaterIndex;
        // If a midpoint is already defined, just return it.
        int ret;
        if (cache.TryGetValue(key, out ret))
            return ret;

        // If we're here, it's because a midpoint for these two
        // vertices hasn't been created yet. Let's do that now!

        Vector3 p1 = vertices[indexA];
        Vector3 p2 = vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        EventLog.Add("Midpoint for " + vertices[indexA] + " and " + vertices[indexB] + " is " + middle);

        ret = vertices.Count;
        vertices.Add(middle);

        cache.Add(key, ret);
        return ret;
    }

    private void EnlargeVertices(float size)
    {
        List<Vector3> tempVertices = new List<Vector3>();

        foreach (Vector3 point in vertices)
        {
            Vector3 newVertex = point * size;
            tempVertices.Add(point);
        }

        vertices = tempVertices;
    }
}
