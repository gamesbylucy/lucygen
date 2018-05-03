using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LucygenPlanet : MonoBehaviour {

    public int subdivisions;
    public int size;
    public float scalingFactor;
    public int perlinRepetitions = -1;
    public int perlinOctaves = 24;
    public float perlinPersistence = .5f;
    public Sun sun;

    private LucygenPolygonSet m_LucygenPolygons;
    private List<Vector3> m_VerticesList;
    private Mesh m_mesh;
    private static LucygenPerlin m_perlin;
    private float m_greatestPolyHeight = 0;

    public void Awake()
    {

    }

    public void Start()
    {
        GetComponent<MeshFilter>().mesh = m_mesh = new Mesh();
        List<Vector3> tempVertices = new List<Vector3>();

        InitAsIcosohedron(size);
        Subdivide(subdivisions);
        m_perlin = new LucygenPerlin(perlinRepetitions);

        float potentialGreatestPolyHeight = 0;

        List<Vector3> shiftedVertices = new List<Vector3>();
        foreach (Vector3 vert in m_VerticesList)
        {
            Vector3 tempVert = vert + Vector3.up + Vector3.right + Vector3.forward;
            shiftedVertices.Add(tempVert);
        }

        foreach (LucygenPolygon poly in m_LucygenPolygons)
        {
            Vector3 vert1 = shiftedVertices[poly.m_vertices[0]];
            Vector3 vert2 = shiftedVertices[poly.m_vertices[1]];
            Vector3 vert3 = shiftedVertices[poly.m_vertices[2]];
            LucygenPolygonSet set = new LucygenPolygonSet();

            float centerX = (vert1.x + vert2.x + vert3.x)/3;
            float centerY = (vert1.y + vert2.y + vert3.y)/3;
            float centerZ = (vert1.z + vert2.z + vert3.z)/3;

            Vector3 vertCenter = new Vector3(centerX, centerY, centerZ);

            set.Add(poly);

            LucygenEventLog.write(System.DateTime.Now + ": Pre-perlin poly coords: " + vertCenter.x + ", " + vertCenter.y + ", " + vertCenter.z);

            float unscaledPolyHeight = (float)m_perlin.OctavePerlin(
                 vertCenter.x,
                 vertCenter.y,
                 vertCenter.z,
                 perlinOctaves,
                 perlinPersistence);

            //float unscaledPolyHeight = Perlin.Fbm(vertCenter.x/4, vertCenter.y/4, vertCenter.z/4, perlinOctaves);

            extrude(set, potentialGreatestPolyHeight = unscaledPolyHeight * scalingFactor);

            LucygenEventLog.write(System.DateTime.Now + ": Unscaled perlin height : " + unscaledPolyHeight);
            LucygenEventLog.write(System.DateTime.Now + ": Scaled perlin height : " + potentialGreatestPolyHeight);

            if (potentialGreatestPolyHeight > m_greatestPolyHeight)
                m_greatestPolyHeight = potentialGreatestPolyHeight;
        }



        foreach (Vector3 enlargedVertex in m_VerticesList)
        {
            Vector3 newVertex = enlargedVertex * size;
            tempVertices.Add(newVertex);
        }

        m_VerticesList = tempVertices;
        m_mesh.vertices = m_VerticesList.ToArray();
        m_mesh.triangles = new int[m_LucygenPolygons.Count * 3];
        m_mesh.triangles = buildTriangles(m_LucygenPolygons, m_mesh.triangles);

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();
        m_mesh.RecalculateTangents();

        LucygenEventLog.write("MESH STATISTICS");
        LucygenEventLog.write("Total Number of Polys: " + m_LucygenPolygons.Count);
        LucygenEventLog.write("Triangles array length: " + m_mesh.triangles.Length);
        LucygenEventLog.write("Vertices array length: " + m_mesh.vertices.Length);
        LucygenEventLog.write("Normals array length: " + m_mesh.normals.Length);
        LucygenEventLog.write("Greatest poly height: " + m_greatestPolyHeight);
        LucygenEventLog.write("Greatest poly height: " + m_greatestPolyHeight);

        /*LucygenEventLog.write("List of Triangle Indices");
        LucygenEventLog.write("-----------------");
        LucygenEventLog.write(string.Empty);

        foreach (int triangle in m_mesh.triangles)
        {
            if ((triangle + 1) % 3 == 0)
            {
                LucygenEventLog.write("Index 1: " + m_mesh.triangles[triangle] + ", Index 2: " + m_mesh.triangles[triangle + 1] + ", index 3: " + m_mesh.triangles[triangle + 2]);
            }
        }*/

        LucygenEventLog.printLog();
    }

    private void Update()
    {
        transform.RotateAround(sun.transform.position, Vector3.up, 20 * Time.deltaTime);
        transform.RotateAround(transform.position, transform.up, 20 * Time.deltaTime);
    }

    //generation methods

    public void InitAsIcosohedron(float size)
    {
        LucygenEventLog.write("Creating icosohedron");
        m_LucygenPolygons = new LucygenPolygonSet();
        m_VerticesList = new List<Vector3>();

        // Formula for calculating vertex angle? Need to figure out what this does...

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        // Initial 12 points of the icosohedron

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

        // Initial 20 sides of the icosohedron
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
        LucygenEventLog.write("Subdividing surface.");
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < recursions; i++)
        {
            var newPolys = new LucygenPolygonSet();
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
        LucygenEventLog.write("Getting mid-point index for " + m_VerticesList[indexA] + " and " + m_VerticesList[indexB]);
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

        LucygenEventLog.write("Midpoint for " + m_VerticesList[indexA] + " and " + m_VerticesList[indexB] + " is " + middle);

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
        return result.ToArray();
    }

    public Vector3 toUnitCircle(Vector3 point)
    {
        Vector3 result = Vector3.zero;
        float distToOrigin = Vector3.Distance(point, Vector3.zero);
        result *= distToOrigin;
        return result;
    } 

    Vector3 ToCartesian(Vector3 aSpherical)
    {
        return ToCartesian(aSpherical, Quaternion.identity);
    }
    Vector3 ToCartesian(Vector3 aSpherical, Quaternion aSpace)
    {
        Vector3 result;
        float c = Mathf.Cos(aSpherical.y);
        result.x = Mathf.Cos(aSpherical.x) * c;
        result.y = Mathf.Sin(aSpherical.y);
        result.z = Mathf.Sin(aSpherical.x) * c;
        result *= aSpherical.z;
        result = aSpace * result;
        return result;
    }

    Vector3 ToCircle(Vector3 aSpherical, float aRadius)
    {
        Vector3 result;
        float r = -aRadius * (aSpherical.y - Mathf.PI / 2) / Mathf.PI;
        result.x = Mathf.Cos(aSpherical.x) * r;
        result.y = aSpherical.z;
        result.z = Mathf.Sin(aSpherical.x) * r;
        return result;
    }

}
