using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Planet : MonoBehaviour {

    //PUBLIC MEMEBERS

    public int subdivisions;
    public int size;
    public int randomSeed = 0;
    public int perlinRepetitions = -1;
    public int perlinOctaves = 24;

    public double scalingFactor;
    public double perlinPersistence = .5f;
    public double perlinIntitialAmplitude = 128;
    public double perlinInitialFrequency = 4;
    public double perlinLacunarity = 2;

    public Sun sun;

    //PRIVATE MEMEBERS

    private PolygonSet m_polygons;
    private List<Vector3> m_vertices;
    private Mesh m_mesh;
    private static Perlin m_perlin;
    private double m_greatestPolyHeight = 0;
    private double m_leastPolyHeight = 0;
    private double[] m_randomNoise;
    private MeshTriangleList m_meshTriangles;

    //UNITY NATIVE

    public void Awake()
    {

    }

    public void Start()
    {
        m_meshTriangles = new MeshTriangleList();

        InitAsIcosohedron(size);
        Subdivide(subdivisions);
        calculatePolygonNeighbors();

        m_randomNoise = getRandomDoubles(m_polygons.Count * 3);

        List<Vector3> shiftedVertices = new List<Vector3>();
        
        int randomIndex = 0;
        double potentialGreatestPolyHeight = 0;
        double potentialLeastPolyHeight = 0;
        double scaledPolyHeight = 0;
        double[] heightMap = new double[m_polygons.Count];
        int heightMapIndex = 0;

        foreach (Polygon poly in m_polygons)
        {
            double xCoord = m_randomNoise[randomIndex++];
            double yCoord = m_randomNoise[randomIndex++];
            double zCoord = m_randomNoise[randomIndex++];

            EventLog.write("Inputs into the perlin function");
            EventLog.write("-------------------------------");
            EventLog.write("X: " + xCoord.ToString());
            EventLog.write("Y: " + yCoord.ToString());
            EventLog.write("Z: " + zCoord.ToString());

            m_perlin = new Perlin(perlinRepetitions);

            float unscaledPolyHeight = (float)m_perlin.OctavePerlin(
                 xCoord,
                 yCoord,
                 zCoord,
                 perlinOctaves,
                 perlinPersistence,
                 perlinIntitialAmplitude,
                 perlinInitialFrequency,
                 perlinLacunarity
                 );

            potentialGreatestPolyHeight = unscaledPolyHeight * scalingFactor;
            potentialLeastPolyHeight = unscaledPolyHeight * scalingFactor;
            scaledPolyHeight = unscaledPolyHeight * scalingFactor;

            if (potentialGreatestPolyHeight > m_greatestPolyHeight)
                m_greatestPolyHeight = potentialGreatestPolyHeight;

            if (potentialLeastPolyHeight <= m_greatestPolyHeight)
                m_leastPolyHeight = potentialLeastPolyHeight;

            heightMap[heightMapIndex++] = scaledPolyHeight;
            EventLog.write(System.DateTime.Now + ": Unscaled perlin height : " + unscaledPolyHeight);
            EventLog.write(System.DateTime.Now + ": Scaled perlin height : " + scaledPolyHeight);
        }

        for (int i = 0; i < heightMap.Length; i++)
        {
            PolygonSet set = new PolygonSet();
            set.Add(m_polygons[i]);
            extrudePolygonSet(set, (float)heightMap[i]);
        }

        enlargePoints(m_vertices, size);

        

        m_mesh = rebuildMesh(m_vertices, m_polygons);
        foreach (int triangle in m_mesh.triangles)
        {
            EventLog.write(triangle.ToString());
        }

        
        

        EventLog.write(System.DateTime.Now + ": MESH STATISTICS");
        EventLog.write(System.DateTime.Now + ": Total Number of Polys: " + m_polygons.Count);
        EventLog.write(System.DateTime.Now + ": Triangles array length: " + m_mesh.triangles.Length);
        EventLog.write(System.DateTime.Now + ": Vertices array length: " + m_mesh.vertices.Length);
        EventLog.write(System.DateTime.Now + ": Normals array length: " + m_mesh.normals.Length);
        EventLog.write(System.DateTime.Now + ": Greatest poly height: " + m_greatestPolyHeight);

        EventLog.printLog();
    }

    private void Update()
    {
        transform.RotateAround(sun.transform.position, Vector3.up, 20 * Time.deltaTime);
        transform.RotateAround(transform.position, transform.up, 20 * Time.deltaTime);
    }


    //HELPER METHODS

    private double[] getRandomDoubles(int size)
    {
        System.Random random = new System.Random(randomSeed);
        double[] result = new double[size];

        for (int i = 0; i < size; i++)
        {
            result[i] = 255 * random.NextDouble();
            EventLog.write("Random noise input at index " + i + " is " + result[i]);
        }

        return result;
    }

    private List<Vector3> shiftPoints(List<Vector3> points, float shiftX, float shiftY, float shiftZ)
    {
        List<Vector3> result = new List<Vector3>();

        foreach (Vector3 point in points)
        {
            Vector3 tempVert = point + Vector3.up * shiftX + Vector3.right * shiftY + Vector3.forward * shiftZ;
            result.Add(tempVert);
        }

        return result;
    }

    private void InitAsIcosohedron(float size)
    {
        EventLog.write(System.DateTime.Now + ": Creating icosohedron");
        m_polygons = new PolygonSet();
        m_vertices = new List<Vector3>();

        // Formula for calculating vertex angle? Need to figure out what this does...

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        // Initial 12 points of the icosohedron

        m_vertices.Add(new Vector3(-1.0f, t, 0).normalized);
        m_vertices.Add(new Vector3(1.0f, t, 0).normalized);
        m_vertices.Add(new Vector3(-1.0f, -t, 0).normalized);
        m_vertices.Add(new Vector3(1.0f, -t, 0).normalized);
        m_vertices.Add(new Vector3(0, -1.0f, t).normalized);
        m_vertices.Add(new Vector3(0, 1.0f, t).normalized);
        m_vertices.Add(new Vector3(0, -1.0f, -t).normalized);
        m_vertices.Add(new Vector3(0, 1.0f, -t).normalized);
        m_vertices.Add(new Vector3(t, 0, -1.0f).normalized);
        m_vertices.Add(new Vector3(t, 0, 1.0f).normalized);
        m_vertices.Add(new Vector3(-t, 0, -1.0f).normalized);
        m_vertices.Add(new Vector3(-t, 0, 1.0f).normalized);

        // Initial 20 sides of the icosohedron

        m_polygons.Add(new Polygon(0, 11, 5));
        m_polygons.Add(new Polygon(0, 5, 1));
        m_polygons.Add(new Polygon(0, 1, 7));
        m_polygons.Add(new Polygon(0, 7, 10));
        m_polygons.Add(new Polygon(0, 10, 11));
        m_polygons.Add(new Polygon(1, 5, 9));
        m_polygons.Add(new Polygon(5, 11, 4));
        m_polygons.Add(new Polygon(11, 10, 2));
        m_polygons.Add(new Polygon(10, 7, 6));
        m_polygons.Add(new Polygon(7, 1, 8));
        m_polygons.Add(new Polygon(3, 9, 4));
        m_polygons.Add(new Polygon(3, 4, 2));
        m_polygons.Add(new Polygon(3, 2, 6));
        m_polygons.Add(new Polygon(3, 6, 8));
        m_polygons.Add(new Polygon(3, 8, 9));
        m_polygons.Add(new Polygon(4, 9, 5));
        m_polygons.Add(new Polygon(2, 4, 11));
        m_polygons.Add(new Polygon(6, 2, 10));
        m_polygons.Add(new Polygon(8, 6, 7));
        m_polygons.Add(new Polygon(9, 8, 1));
    }

    private void Subdivide(int recursions)
    {
        EventLog.write(System.DateTime.Now + ": Subdividing surface.");
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < recursions; i++)
        {
            var newPolys = new PolygonSet();
            foreach (var poly in m_polygons)
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

                newPolys.Add(new Polygon(a, ab, ca));
                newPolys.Add(new Polygon(b, bc, ab));
                newPolys.Add(new Polygon(c, ca, bc));
                newPolys.Add(new Polygon(ab, bc, ca));
            }

            // Replace all our old polygons with the new set of
            // subdivided ones.
            m_polygons = newPolys;
        }
    }

    //helper methods
    private int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {
        EventLog.write(System.DateTime.Now + ": Getting mid-point index for " + m_vertices[indexA] + " and " + m_vertices[indexB]);

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

        Vector3 p1 = m_vertices[indexA];
        Vector3 p2 = m_vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        EventLog.write(System.DateTime.Now + ": Midpoint for " + m_vertices[indexA] + " and " + m_vertices[indexB] + " is " + middle);

        ret = m_vertices.Count;
        m_vertices.Add(middle);

        cache.Add(key, ret);
        return ret;
    }

    private void calculatePolygonNeighbors()
    {
        foreach (Polygon polygon in m_polygons)
        {
            foreach (Polygon otherPolygon in m_polygons)
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

    private List<int> cloneVertices(List<int> oldVerts)
    {
        List<int> result = new List<int>();

        foreach (int oldVert in oldVerts)
        {
            Vector3 clonedVert = m_vertices[oldVert];
            result.Add(m_vertices.Count);
            m_vertices.Add(clonedVert);
        }

        return result;
    }

    private void stitchPolygons(PolygonSet polys)
    {
        EdgeSet oldEdges = polys.createEdgeSet();

        List<int> oldVertices = oldEdges.GetUniqueVertices();

        List<int> newVertices = cloneVertices(oldVertices);

        EdgeSet newEdges = oldEdges.clone();

        for (int i = 0; i < oldEdges.Count; i++)
        {
            Edge oldEdge = oldEdges[i];
            Edge newEdge = newEdges[i];

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

            Polygon stitchPoly1 = new Polygon(oldEdge.m_sharedVertices[0],
                                              oldEdge.m_sharedVertices[1],
                                              newEdge.m_sharedVertices[0]);

            Polygon stitchPoly2 = new Polygon(oldEdge.m_sharedVertices[1],
                                              newEdge.m_sharedVertices[1],
                                              newEdge.m_sharedVertices[0]);

            oldEdge.m_innerPolygon.ReplaceNeighbor(oldEdge.m_outerPolygon, stitchPoly2);
            oldEdge.m_outerPolygon.ReplaceNeighbor(oldEdge.m_innerPolygon, stitchPoly1);

            m_polygons.Add(stitchPoly1);
            m_polygons.Add(stitchPoly2);
        }

        //Swap to the new vertices on the inner polys.

        foreach (Polygon poly in polys)
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

    private PolygonSet createHexSet(Polygon poly)
    {
        PolygonSet result = new PolygonSet();
        Polygon p1 = m_polygons[0];
        result.Add(p1);

        Polygon p2 = p1.m_neighbors[0];
        Polygon p6 = p1.m_neighbors[1];
        result.Add(p2);
        result.Add(p6);

        Polygon p3 = new Polygon(0, 0, 0);
        Polygon p4 = new Polygon(0, 0, 0);
        Polygon p5 = new Polygon(0, 0, 0);

        foreach (Polygon p2Neighbor in p2.m_neighbors)
        {
            foreach (Polygon p6Neighbor in p6.m_neighbors)
            {
                /*foreach (Polygon p2NeighborNeighbor in p2Neighbor.m_neighbors)
                {
                    if (p6Neighbor.m_neighbors.Contains(p2NeighborNeighbor) && (p2NeighborNeighbor != p6) && (p2NeighborNeighbor != p2))
                    {
                        p3 = p2Neighbor;
                        p5 = p6Neighbor;
                        p4 = p2NeighborNeighbor;
                        testSet.Add(p3);
                        testSet.Add(p5);
                        testSet.Add(p4);
                    }
                }*/
                foreach (Polygon p2NeighborNeighbor in p2Neighbor.m_neighbors)
                {
                    foreach (Polygon p6NeighborNeighbor in p6Neighbor.m_neighbors)
                    {
                        if (p2NeighborNeighbor == p6NeighborNeighbor)
                        {
                            foreach (Polygon unionPoly in result)
                            {
                                if (p2NeighborNeighbor.m_neighbors.Contains(unionPoly))
                                    continue;

                                p3 = p2Neighbor;
                                p5 = p6Neighbor;
                                p4 = p2NeighborNeighbor;
                            }
                        }
                    }
                }
            }
        }

        result.Add(p3);
        result.Add(p5);
        result.Add(p4);

        return result;
    }

    private void extrudePolygonSet(PolygonSet polys, float height)
    {
        stitchPolygons(polys);

        List<int> verts = polys.getUniqueVertices();

        foreach (int vert in verts)
        {
            Vector3 v = m_vertices[vert];

            v = v.normalized * (v.magnitude + height);

            m_vertices[vert] = v;
        }
    }

    private void insetPolygonSet(PolygonSet polys, float interpolation)
    {
        stitchPolygons(polys);

        List<int> verts = polys.getUniqueVertices();

        //Calculate the average center of all the vertices
        //in these polygons.

        Vector3 center = Vector3.zero;

        foreach (int vert in verts)
            center += m_vertices[vert];

        center /= verts.Count;

        //Pull each vertex towards the center, then correct
        //its height so that it's as far from the center of
        //the planet as it was before.

        foreach (int vert in verts)
        {
            Vector3 v = m_vertices[vert];

            float height = v.magnitude;

            v = Vector3.Lerp(v, center, interpolation);

            v = v.normalized * height;

            m_vertices[vert] = v;
        }
    }

    private List<Vector3> enlargePoints(List<Vector3> points, float size)
    {
        List<Vector3> tempPoints = new List<Vector3>();

        foreach (Vector3 point in points)
        {
            Vector3 newVertex = point * size;
            tempPoints.Add(point);
        }

        return points;
    }

    private Mesh rebuildMesh(List<Vector3> verts, List<Polygon> polys)
    {
        Mesh result = GetComponent<MeshFilter>().mesh = m_mesh = new Mesh();

        result.vertices = verts.ToArray();
        result.triangles = buildTriangles(polys);
        System.Array.Sort<int>(result.triangles);
        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();
        m_mesh.RecalculateTangents();
        GetComponent<MeshCollider>().sharedMesh = m_mesh;

        return result;
    }

    //takes the indices stored in each polygon and copies them, in order, into a list of triangle points
    private int[] buildTriangles(List<Polygon> polygons)
    {
        int[] result = new int[polygons.Count * 3];
        for (int i = 0; i < m_polygons.Count * 3;)
        {
            for (int j = 0; j < polygons.Count; j++)
            {
                for (int k = 0; k < polygons[j].m_vertices.Count; k++)
                {
                    result[i++] = polygons[j].m_vertices[k];
                }
            }
        }
        return result;
    }
}
