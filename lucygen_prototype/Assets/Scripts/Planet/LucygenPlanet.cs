using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LucygenPlanet : MonoBehaviour {

    public int subdivisions;
    public int size;

    private List<LucygenPolygon> m_LucygenPolygons;
    private List<Vector3> m_VerticesList;
    private Mesh m_mesh;

    public void Awake()
    {

    }

    public void Start()
    {
        GetComponent<MeshFilter>().mesh = m_mesh = new Mesh();
        Debug.Log("Initiating planet generation.");
        InitAsIcosohedron(size);
        Subdivide(subdivisions);
        List<Vector3> tempVertices = new List<Vector3>();

        foreach (Vector3 vertex in m_VerticesList)
        {
            Vector3 newVertex = vertex * size;
            tempVertices.Add(newVertex);
        }

        m_VerticesList = tempVertices;
        Debug.Log("Creating vertex array.");
        m_mesh.vertices = m_VerticesList.ToArray();
        m_mesh.triangles = new int[m_VerticesList.Count];
        buildTriangles(m_LucygenPolygons, m_mesh.triangles);
        for (int i = 0; i < m_mesh.triangles.Length - 1; i++)
        {
            Debug.Log(m_mesh.triangles[i]);
        }
    }

    //generation methods

    public void InitAsIcosohedron(float size)
    {
        Debug.Log("Creating initial vertices.");
        m_LucygenPolygons = new List<LucygenPolygon>();
        m_VerticesList = new List<Vector3>();

        // An icosahedron has 12 vertices, and
        // since it's completely symmetrical the
        // formula for calculating them is kind of
        // symmetrical too:

        float t = (1.0f + Mathf.Sqrt(0.5f)) / 2.0f;

        m_VerticesList.Add(new Vector3(-1.0f, t, 0).normalized);
        m_VerticesList.Add(new Vector3(1.0f, t, 0).normalized);
        m_VerticesList.Add(new Vector3(-1.0f, -t, 0).normalized);
        m_VerticesList.Add(new Vector3(1.0f, -t, 0).normalized);
        m_VerticesList.Add(new Vector3(0, -1.0f, t).normalized);
        m_VerticesList.Add(new Vector3(0, 1.0f, t).normalized);
        m_VerticesList.Add(new Vector3(0, -1.0f, -t).normalized);
        m_VerticesList.Add(new Vector3(0, 1.0f, -t).normalized);
        m_VerticesList.Add(new Vector3(t, 0, -1.0f).normalized);
        m_VerticesList.Add(new Vector3(t, 0, 1.0f).normalized);
        m_VerticesList.Add(new Vector3(-t, 0, -1.0f).normalized);
        m_VerticesList.Add(new Vector3(-t, 0, 1.0f).normalized);

        // And here's the formula for the 20 sides,
        // referencing the 12 vertices we just created.
        m_LucygenPolygons.Add(new LucygenPolygon(0, 11, 5));
        m_LucygenPolygons.Add(new LucygenPolygon(0, 5, 1));
        m_LucygenPolygons.Add(new LucygenPolygon(0, 1, 7));
        m_LucygenPolygons.Add(new LucygenPolygon(0, 7, 10));
        m_LucygenPolygons.Add(new LucygenPolygon(0, 10, 11));
        m_LucygenPolygons.Add(new LucygenPolygon(1, 5, 9));
        m_LucygenPolygons.Add(new LucygenPolygon(5, 11, 4));
        m_LucygenPolygons.Add(new LucygenPolygon(11, 10, 2));
        m_LucygenPolygons.Add(new LucygenPolygon(10, 7, 6));
        m_LucygenPolygons.Add(new LucygenPolygon(7, 1, 8));
        m_LucygenPolygons.Add(new LucygenPolygon(3, 9, 4));
        m_LucygenPolygons.Add(new LucygenPolygon(3, 4, 2));
        m_LucygenPolygons.Add(new LucygenPolygon(3, 2, 6));
        m_LucygenPolygons.Add(new LucygenPolygon(3, 6, 8));
        m_LucygenPolygons.Add(new LucygenPolygon(3, 8, 9));
        m_LucygenPolygons.Add(new LucygenPolygon(4, 9, 5));
        m_LucygenPolygons.Add(new LucygenPolygon(2, 4, 11));
        m_LucygenPolygons.Add(new LucygenPolygon(6, 2, 10));
        m_LucygenPolygons.Add(new LucygenPolygon(8, 6, 7));
        m_LucygenPolygons.Add(new LucygenPolygon(9, 8, 1));
    }

    public void Subdivide(int recursions)
    {
        Debug.Log("Subdividing surface.");
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < recursions; i++)
        {
            var newPolys = new List<LucygenPolygon>();
            foreach (var poly in m_LucygenPolygons)
            {
                int a = poly.m_Vertices[0];
                int b = poly.m_Vertices[1];
                int c = poly.m_Vertices[2];
                // Use GetMidPointIndex to either create a
                // new vertex between two old vertices, or
                // find the one that was already created.
                int ab = GetMidPointIndex(midPointCache, a, b);
                int bc = GetMidPointIndex(midPointCache, b, c);
                int ca = GetMidPointIndex(midPointCache, c, a);
                // Create the four new polygons using our original
                // three vertices, and the three new midpoints.
                newPolys.Add(new LucygenPolygon(a, ab, ca));
                newPolys.Add(new LucygenPolygon(b, bc, ab));
                newPolys.Add(new LucygenPolygon(c, ca, bc));
                newPolys.Add(new LucygenPolygon(ab, bc, ca));
            }
            // Replace all our old polygons with the new set of
            // subdivided ones.
            m_LucygenPolygons = newPolys;
        }
    }

    //helper methods
    public int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {
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
        Vector3 p1 = m_VerticesList[indexA];
        Vector3 p2 = m_VerticesList[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        ret = m_VerticesList.Count;
        m_VerticesList.Add(middle);

        cache.Add(key, ret);
        return ret;
    }

    //takes the indices stored in each polygon and copies them, in order, into a list of triangle points
    public void buildTriangles(List<LucygenPolygon> polygons, int[] trianglePoints)
    {
        //for every polygon
        int i = 0;
        //for each point in the polygon
        for (int j = 0; j < polygons.Count; j++)
        {
            for (int k = 0; k < polygons[j].m_Vertices.Count; k++)
            {
                Debug.Log("Copying point " + polygons[j].m_Vertices[k] + " into triangle index " + i + ".");
                trianglePoints[i] = polygons[j].m_Vertices[k];
                if (i < trianglePoints.Length - 1)
                {
                    i++;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (m_VerticesList != null && m_VerticesList.Count > 0)
        {
            foreach (Vector3 vertex in m_VerticesList)
            {
                Gizmos.DrawSphere(vertex, 0.005f);
            }
        }
    }
}
