using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LucygenPlanet : MonoBehaviour {

    public int subdivisions;
    public int size;
    public Sun sun;
    public float percentMountain;
    public float percentHills;
    public float percentPlains;
    public float percentValleys;
    public float percentCanyons;

    private List<LucygenPolygon> m_LucygenPolygons;
    private List<Vector3> m_VerticesList;
    private Mesh m_mesh;
    private string docstring;

    public void Awake()
    {

    }

    public void Start()
    {
        GetComponent<MeshFilter>().mesh = m_mesh = new Mesh();
        InitAsIcosohedron(size);
        Subdivide(subdivisions);
        List<Vector3> tempVertices = new List<Vector3>();

        LucygenPolygonSet hills = new LucygenPolygonSet();
        LucygenPolygonSet mountains = new LucygenPolygonSet();
        LucygenPolygonSet plains = new LucygenPolygonSet();
        LucygenPolygonSet valleys = new LucygenPolygonSet();
        LucygenPolygonSet canyons = new LucygenPolygonSet();

        int numMountains = (int)(percentMountain * (float)m_LucygenPolygons.Count);
        int numHills = (int)(percentHills * (float)m_LucygenPolygons.Count);
        int numPlains = (int)(percentPlains * (float)m_LucygenPolygons.Count);
        int numValleys = (int)(percentValleys * (float)m_LucygenPolygons.Count);
        int numCanyons = (int)(percentCanyons * (float)m_LucygenPolygons.Count);
        int sum = numMountains + numHills + numPlains + numValleys + numCanyons;

        docstring += ("Sum of polys: " + sum + "\n" + "Mountain polys: " + numMountains + "\n" +
            "Hill polys: " + numHills + "\n" + "Plains polys: " + numPlains + "\n" + "Valley polys: " 
            + numValleys + "\n" + "Canyon polys: " + numCanyons + "\n");

        foreach (LucygenPolygon polygon in m_LucygenPolygons)
        {
            if (sum > m_LucygenPolygons.Count)
            {
                numPlains = numPlains - (m_LucygenPolygons.Count - sum);
            }
            else if (sum < m_LucygenPolygons.Count)
            {
                numPlains = numValleys - (m_LucygenPolygons.Count - sum);
            }

            List<LucygenPolygon> assigned = new List<LucygenPolygon>();

            while (sum > 0)
            {
                int index = Random.Range(0, m_LucygenPolygons.Count);

                LucygenPolygon current = m_LucygenPolygons[index];

                if (!(assigned.Contains(current)))
                {
                    if(numMountains > 0)
                    {
                        mountains.Add(current);
                        numMountains--;
                    }
                    else if(numHills > 0)
                    {
                        hills.Add(current);
                        numHills--;
                    }
                    else if(numPlains > 0)
                    {
                        plains.Add(current);
                        numPlains--;
                    }
                    else if(numValleys > 0)
                    {
                        valleys.Add(current);
                        numValleys--;
                    }
                    else if(numCanyons > 0)
                    {
                        canyons.Add(current);
                        numCanyons--;
                    }
                    sum--;
                }
            }
        }

        extrude(mountains, .1f);
        extrude(hills, .05f);
        extrude(valleys, -.05f);
        extrude(canyons, -.1f);

        foreach (Vector3 vertex in m_VerticesList)
        {
            Vector3 newVertex = vertex * size;
            tempVertices.Add(newVertex);
        }

        

        m_VerticesList = tempVertices;
        m_mesh.vertices = m_VerticesList.ToArray();
        m_mesh.triangles = new int[m_LucygenPolygons.Count * 3];
        m_mesh.triangles = buildTriangles(m_LucygenPolygons, m_mesh.triangles);

        m_mesh.RecalculateNormals();

        docstring += ("Number of Polys: " + m_LucygenPolygons.Count + "\n" + 
            "Triangles array length: " + m_mesh.triangles.Length + "\n" + 
            "Vertices array length: " + m_mesh.vertices.Length + "\n" +
            "Normals array length: " + m_mesh.normals.Length);

        for (int i = 0; i < m_mesh.triangles.Length; i++)
        {
            docstring += m_mesh.triangles[i] + " ";
            if ((i + 1) % 3 == 0)
                docstring += "\n";
        }
        Debug.Log(docstring);
    }

    private void Update()
    {
        transform.RotateAround(sun.transform.position, Vector3.up, 20 * Time.deltaTime);
        transform.RotateAround(transform.position, transform.up, 20 * Time.deltaTime);
    }

    //generation methods

    public void InitAsIcosohedron(float size)
    {
        m_LucygenPolygons = new List<LucygenPolygon>();
        m_VerticesList = new List<Vector3>();

        // An icosahedron has 12 vertices, and
        // since it's completely symmetrical the
        // formula for calculating them is kind of
        // symmetrical too:

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

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
                int a = poly.m_vertices[0];
                int b = poly.m_vertices[1];
                int c = poly.m_vertices[2];
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

    public void calculatePolygonNeighbors()
    {
        foreach (LucygenPolygon polygon in m_LucygenPolygons)
        {
            foreach (LucygenPolygon otherPolygon in m_LucygenPolygons)
            {
                if (polygon == otherPolygon)
                    continue;
                if (polygon.isNeighborOf(otherPolygon))
                {
                    polygon.m_neighbors.Add(otherPolygon);
                }
            }
        }
    }

    public List<int> cloneVertices(List<int> oldVerts)
    {
        List<int> newVerts = new List<int>();

        foreach (int oldVert in oldVerts)
        {
            Vector3 clonedVert = new Vector3(m_VerticesList[oldVert].x, m_VerticesList[oldVert].y, m_VerticesList[oldVert].z);
            newVerts.Add(m_VerticesList.Count);
            m_VerticesList.Add(clonedVert);
        }
        return newVerts;
    }

    public void StitchPolygons(LucygenPolygonSet polys)
    {
        LucygenEdgeSet oldEdges = polys.createEdgeSet();
        List<int> oldVertices = oldEdges.GetUniqueVertices();
        List<int> newVertices = cloneVertices(oldVertices);
        LucygenEdgeSet newEdges = oldEdges.clone();

        for (int i = 0; i < oldEdges.Count; i++)
        {
            LucygenEdge oldEdge = oldEdges[i];
            LucygenEdge newEdge = newEdges[i];

            //Make sure that the vertices in newEdge
            //are the ones we recently cloned, instead
            //of the old vertices.

            for (int j = 0; j < 2; j++)
            {
                int oldVertex = oldEdge.m_sharedVertices[j];
                int index = oldVertices.IndexOf(oldVertex);
                newEdge.m_sharedVertices[j] = newVertices[index];
            }

            //Create new polys along the stitched edge. These
            //will connect the original poly to its former neighbor.

            LucygenPolygon stitchPoly1 = new LucygenPolygon(oldEdge.m_sharedVertices[0],
                                                            oldEdge.m_sharedVertices[1],
                                                            oldEdge.m_sharedVertices[0]);

            LucygenPolygon stitchPoly2 = new LucygenPolygon(oldEdge.m_sharedVertices[1],
                                                            oldEdge.m_sharedVertices[1],
                                                            oldEdge.m_sharedVertices[0]);

            oldEdge.m_innerPolygon.ReplaceNeighbor(oldEdge.m_outerPolygon, stitchPoly2);
            oldEdge.m_outerPolygon.ReplaceNeighbor(oldEdge.m_innerPolygon, stitchPoly1);

            m_LucygenPolygons.Add(stitchPoly1);
            m_LucygenPolygons.Add(stitchPoly2);
        }

        //Swap to the new vertices on the inner polys.

        foreach (LucygenPolygon poly in polys)
        {
            for (int i = 0; i < 3; i++)
            {
                int vertID = poly.m_vertices[i];
                if (!oldVertices.Contains(vertID))
                    continue;

                int vertIndex = oldVertices.IndexOf(vertID);
                poly.m_vertices[i] = newVertices[vertIndex];
            }
        }
    }

    public void extrude(LucygenPolygonSet polys, float height)
    {
        StitchPolygons(polys);

        List<int> verts = polys.getUniqueVertices();

        foreach (int vert in verts)
        {
            Vector3 v = m_VerticesList[vert];

            v = v.normalized * (v.magnitude + height);

            m_VerticesList[vert] = v;
        }
    }

    public void inset(LucygenPolygonSet polys, float interpolation)
    {
        StitchPolygons(polys);

        List<int> verts = polys.getUniqueVertices();

        //Calculate the average center of all the vertices
        //in these polygons.

        Vector3 center = Vector3.zero;

        foreach (int vert in verts)
            center += m_VerticesList[vert];

        center /= verts.Count;

        //Pull each vertex towards the center, then correct
        //its height so that it's as far from the center of
        //the planet as it was before.

        foreach (int vert in verts)
        {
            Vector3 v = m_VerticesList[vert];

            float height = v.magnitude;

            v = Vector3.Lerp(v, center, interpolation);

            v = v.normalized * height;

            m_VerticesList[vert] = v;
        }
    }

    //takes the indices stored in each polygon and copies them, in order, into a list of triangle points
    public int[] buildTriangles(List<LucygenPolygon> polygons, int[] trianglePoints)
    { 
        for (int i = 0; i < m_LucygenPolygons.Count * 3;)
        {
            for (int j = 0; j < polygons.Count; j++)
            {
                for (int k = 0; k < polygons[j].m_vertices.Count; k++)
                {
                    trianglePoints[i++] = polygons[j].m_vertices[k];
                }
            }
        }
        return trianglePoints;
    }



    /*private void OnDrawGizmos()
    {
        if (m_VerticesList != null && m_VerticesList.Count > 0)
        {
            foreach (Vector3 vertex in m_VerticesList)
            {
                Gizmos.DrawSphere(vertex, 0.005f);
            }
        }
    }*/

    //LEGACY

    public Vector3[] buildNormals(List<LucygenPolygon> polygons)
    {
        List<Vector3> result = new List<Vector3>();
        foreach (LucygenPolygon polygon in polygons)
        {
            for (int i = 0; i < polygon.m_vertices.Count; i++)
            {
                Vector3 a;
                Vector3 b;
                Vector3 c;
                Vector3 side1;
                Vector3 side2;
                Vector3 perp;
                switch (i)
                {

                    case 0:
                        a = m_mesh.vertices[polygon.m_vertices[0]];
                        b = m_mesh.vertices[polygon.m_vertices[1]];
                        c = m_mesh.vertices[polygon.m_vertices[2]];
                        side1 = b - a;
                        side2 = c - a;
                        perp = Vector3.Cross(side1, side2);
                        if (!result.Contains(perp))
                            result.Add(perp);
                        break;
                    case 1:
                        a = m_mesh.vertices[polygon.m_vertices[1]];
                        b = m_mesh.vertices[polygon.m_vertices[2]];
                        c = m_mesh.vertices[polygon.m_vertices[0]];
                        side1 = b - a;
                        side2 = c - a;
                        perp = Vector3.Cross(side1, side2);
                        if (!result.Contains(perp))
                            result.Add(perp);
                        break;
                    case 2:
                        a = m_mesh.vertices[polygon.m_vertices[2]];
                        b = m_mesh.vertices[polygon.m_vertices[0]];
                        c = m_mesh.vertices[polygon.m_vertices[1]];
                        side1 = b - a;
                        side2 = c - a;
                        perp = Vector3.Cross(side1, side2);
                        if (!result.Contains(perp))
                            result.Add(perp);
                        break;
                }

            }
        }
        Debug.Log("Number of calculated normals: " + result.Count);
        return result.ToArray();
    }
}
