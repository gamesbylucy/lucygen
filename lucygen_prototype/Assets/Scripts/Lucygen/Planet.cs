using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Planet : MonoBehaviour {

    //PUBLIC MEMEBERS

    public int i_subdivisions = 3;
    public int i_size = 1;
    public int i_randomSeed = 0;
    public int i_perlinRepetitions = -1;
    public int i_perlinOctaves = 6;

    public double d_scalingFactor = 1;
    public double d_perlinPersistence = .5f;
    public double d_perlinAmplitude = 128;
    public double d_perlinFrequency = 4;
    public double d_perlinLacunarity = 2;

    public Sun sun;

    //PRIVATE MEMEBERS

    private PolygonSet ml_polygonSet;
    private List<Vector3> ml_vertices;
    private Mesh m_mesh;
    private static Perlin m_perlin;
    private double md_greatestPolyHeight = 0;
    private double md_leastPolyHeight = 0;
    private double[] mad_randomNoise;

    //UNITY NATIVE

    public void Awake()
    {

    }

    public void Start()
    {
        InitAsIcosohedron(i_size);
        Subdivide(i_subdivisions);
        CalculatePolygonNeighbors();

        mad_randomNoise = GetRandomDoubles(ml_polygonSet.Count * 3);

        List<Vector3> shiftedVertices = new List<Vector3>();
        
        int randomIndex = 0;
        double potentialGreatestPolyHeight = 0;
        double potentialLeastPolyHeight = 0;
        double scaledPolyHeight = 0;
        double[] heightMap = new double[ml_polygonSet.Count];
        int heightMapIndex = 0;

        foreach (Polygon poly in ml_polygonSet)
        {
            double xCoord = mad_randomNoise[randomIndex++];
            double yCoord = mad_randomNoise[randomIndex++];
            double zCoord = mad_randomNoise[randomIndex++];

            EventLog.Add("Inputs into the perlin function");
            EventLog.Add("-------------------------------");
            EventLog.Add("X: " + xCoord.ToString());
            EventLog.Add("Y: " + yCoord.ToString());
            EventLog.Add("Z: " + zCoord.ToString());

            m_perlin = new Perlin(i_perlinRepetitions);

            float unscaledPolyHeight = (float)m_perlin.OctavePerlin
            (
                xCoord,
                yCoord,
                zCoord,
                i_perlinOctaves,
                d_perlinPersistence,
                d_perlinAmplitude,
                d_perlinFrequency,
                d_perlinLacunarity
            );

            potentialGreatestPolyHeight = unscaledPolyHeight * d_scalingFactor;
            potentialLeastPolyHeight = unscaledPolyHeight * d_scalingFactor;
            scaledPolyHeight = unscaledPolyHeight * d_scalingFactor;

            if (potentialGreatestPolyHeight > md_greatestPolyHeight)
                md_greatestPolyHeight = potentialGreatestPolyHeight;

            if (potentialLeastPolyHeight <= md_greatestPolyHeight)
                md_leastPolyHeight = potentialLeastPolyHeight;

            heightMap[heightMapIndex++] = scaledPolyHeight;
            EventLog.Add("Unscaled perlin height : " + unscaledPolyHeight);
            EventLog.Add("Scaled perlin height : " + scaledPolyHeight);
        }

        /*for (int i = 0; i < heightMap.Length; i++)
        {
            PolygonSet set = new PolygonSet();
            set.Add(ml_polygonSet[i]);
            //InsetPolygonSet(set, 0.1f);
            ExtrudePolygonSet(set, (float)heightMap[i]);
            //InsetPolygonSet(set, 1f);
        }*/

        ExtrudeRandomPolygons(2, ml_polygonSet, 0, 1);

        //EnlargePoints(ml_vertices, i_size);

        RebuildMesh(ml_polygonSet, ml_vertices);

        EventLog.Add("TRIANGLE VERTEX ORDER");
        EventLog.Add("----------------------");
        foreach (int triangle in m_mesh.triangles)
        {
            EventLog.Add(triangle.ToString());
        }

        EventLog.Add("MESH STATISTICS");
        EventLog.Add("Total Number of Polys: " + ml_polygonSet.Count);
        EventLog.Add("Mesh triangles array length: " + m_mesh.triangles.Length);
        EventLog.Add("Mesh vertices array length: " + m_mesh.vertices.Length);
        EventLog.Add("Mesh normals array length: " + m_mesh.normals.Length);
        EventLog.Add("Greatest poly height: " + md_greatestPolyHeight);

        EventLog.WriteFile();
    }

    private void Update()
    {
        transform.RotateAround(sun.transform.position, Vector3.up, 20 * Time.deltaTime);
        transform.RotateAround(transform.position, transform.up, 20 * Time.deltaTime);
    }



    //HELPER METHODS

    private void ExtrudeRandomPolygons(int numPolygons, PolygonSet polygons, float minHeight, float maxHeight)
    {
        System.Random random = new System.Random();

        PolygonSet extrudedPolygons = new PolygonSet();
        extrudedPolygons.Add(polygons[random.Next(0, polygons.Count - 1)]);

        extrudedPolygons = GetRandomClusterSet(numPolygons, extrudedPolygons.Count, extrudedPolygons, extrudedPolygons[0]);

        ExtrudePolygonSet(extrudedPolygons, (float)(random.NextDouble() * (maxHeight - minHeight) + minHeight));
        InsetPolygonSet(extrudedPolygons, 1f);
    }

    private PolygonSet GetRandomClusterSet(int maxNumPolygons, int currentNumPolygons, PolygonSet polygons, Polygon currentPolygon)
    {
        if (currentNumPolygons == maxNumPolygons)
        {
            return polygons;
        }
        else
        {
            System.Random random = new System.Random();

            Polygon newPolygon = currentPolygon.ml_neighborPolygons[random.Next(0, 2)];

            while (polygons.Contains(newPolygon))
            {
                newPolygon = currentPolygon.ml_neighborPolygons[random.Next(0, 2)];
            }

            polygons.Add(newPolygon);

            return GetRandomClusterSet(maxNumPolygons, currentNumPolygons + 1, polygons, newPolygon);
        }
    }

    private double[] GetRandomDoubles(int size)
    {
        System.Random random = new System.Random(i_randomSeed);
        double[] result = new double[size];

        for (int i = 0; i < size; i++)
        {
            result[i] = 255 * random.NextDouble();
            EventLog.Add("Random noise input at index " + i + " is " + result[i]);
        }

        return result;
    }

    private List<Vector3> ShiftPoints(List<Vector3> points, float shiftX, float shiftY, float shiftZ)
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
        EventLog.Add("Creating icosohedron");
        ml_polygonSet = new PolygonSet();
        ml_vertices = new List<Vector3>();

        // Formula for calculating vertex angle? Need to figure out what this does...

        float t = (1.0f + Mathf.Sqrt(5.0f)) / 2.0f;

        // Initial 12 points of the icosohedron

        ml_vertices.Add(new Vector3(-1.0f, t, 0).normalized);
        ml_vertices.Add(new Vector3(1.0f, t, 0).normalized);
        ml_vertices.Add(new Vector3(-1.0f, -t, 0).normalized);
        ml_vertices.Add(new Vector3(1.0f, -t, 0).normalized);
        ml_vertices.Add(new Vector3(0, -1.0f, t).normalized);
        ml_vertices.Add(new Vector3(0, 1.0f, t).normalized);
        ml_vertices.Add(new Vector3(0, -1.0f, -t).normalized);
        ml_vertices.Add(new Vector3(0, 1.0f, -t).normalized);
        ml_vertices.Add(new Vector3(t, 0, -1.0f).normalized);
        ml_vertices.Add(new Vector3(t, 0, 1.0f).normalized);
        ml_vertices.Add(new Vector3(-t, 0, -1.0f).normalized);
        ml_vertices.Add(new Vector3(-t, 0, 1.0f).normalized);

        // Initial 20 sides of the icosohedron

        ml_polygonSet.Add(new Polygon(0, 11, 5));
        ml_polygonSet.Add(new Polygon(0, 5, 1));
        ml_polygonSet.Add(new Polygon(0, 1, 7));
        ml_polygonSet.Add(new Polygon(0, 7, 10));
        ml_polygonSet.Add(new Polygon(0, 10, 11));
        ml_polygonSet.Add(new Polygon(1, 5, 9));
        ml_polygonSet.Add(new Polygon(5, 11, 4));
        ml_polygonSet.Add(new Polygon(11, 10, 2));
        ml_polygonSet.Add(new Polygon(10, 7, 6));
        ml_polygonSet.Add(new Polygon(7, 1, 8));
        ml_polygonSet.Add(new Polygon(3, 9, 4));
        ml_polygonSet.Add(new Polygon(3, 4, 2));
        ml_polygonSet.Add(new Polygon(3, 2, 6));
        ml_polygonSet.Add(new Polygon(3, 6, 8));
        ml_polygonSet.Add(new Polygon(3, 8, 9));
        ml_polygonSet.Add(new Polygon(4, 9, 5));
        ml_polygonSet.Add(new Polygon(2, 4, 11));
        ml_polygonSet.Add(new Polygon(6, 2, 10));
        ml_polygonSet.Add(new Polygon(8, 6, 7));
        ml_polygonSet.Add(new Polygon(9, 8, 1));
    }

    private void Subdivide(int recursions)
    {
        EventLog.Add("Subdividing surface.");
        var midPointCache = new Dictionary<int, int>();

        for (int i = 0; i < recursions; i++)
        {
            var newPolys = new PolygonSet();
            foreach (var poly in ml_polygonSet)
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
            ml_polygonSet = newPolys;
        }
    }

    //helper methods
    private int GetMidPointIndex(Dictionary<int, int> cache, int indexA, int indexB)
    {
        EventLog.Add("Getting mid-point index for " + ml_vertices[indexA] + " and " + ml_vertices[indexB]);

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

        Vector3 p1 = ml_vertices[indexA];
        Vector3 p2 = ml_vertices[indexB];
        Vector3 middle = Vector3.Lerp(p1, p2, 0.5f).normalized;

        EventLog.Add("Midpoint for " + ml_vertices[indexA] + " and " + ml_vertices[indexB] + " is " + middle);

        ret = ml_vertices.Count;
        ml_vertices.Add(middle);

        cache.Add(key, ret);
        return ret;
    }

    private void CalculatePolygonNeighbors()
    {
        foreach (Polygon polygon in ml_polygonSet)
        {
            foreach (Polygon otherPolygon in ml_polygonSet)
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

    private List<int> CloneVertices(List<int> oldVerts)
    {
        List<int> result = new List<int>();

        foreach (int oldVert in oldVerts)
        {
            Vector3 clonedVert = new Vector3(ml_vertices[oldVert].x, ml_vertices[oldVert].y, ml_vertices[oldVert].z);
            result.Add(ml_vertices.Count);
            ml_vertices.Add(clonedVert);
        }

        return result;
    }

    //triangle version
    private void StitchPolygons(PolygonSet polys)
    {
        EdgeSet oldEdges = polys.CreateEdgeSet();

        List<int> oldVertices = oldEdges.GetUniqueVertices();

        List<int> newVertices = CloneVertices(oldVertices);

        EdgeSet newEdges = oldEdges.Clone();

        for (int i = 0; i < oldEdges.Count; i++)
        {
            Edge oldEdge = oldEdges[i];
            Edge newEdge = newEdges[i];

            //Make sure that the vertices in newEdge
            //are the ones we recently cloned, instead
            //of the old vertices.

            for (int j = 0; j < 2; j++)
            {
                int oldVertex = oldEdge.mli_sharedVertices[j];
                int index = oldVertices.IndexOf(oldVertex);
                newEdge.mli_sharedVertices[j] = newVertices[index];
            }

            //Create new polys along the stitched edge. These
            //will connect the original poly to its former neighbor.

            Polygon stitchPoly1 = new Polygon(oldEdge.mli_sharedVertices[0],
                                              oldEdge.mli_sharedVertices[1],
                                              newEdge.mli_sharedVertices[0]);

            Polygon stitchPoly2 = new Polygon(oldEdge.mli_sharedVertices[1],
                                              newEdge.mli_sharedVertices[1],
                                              newEdge.mli_sharedVertices[0]);

            oldEdge.m_innerPolygon.ReplaceNeighbor(oldEdge.m_outerPolygon, stitchPoly2);
            oldEdge.m_outerPolygon.ReplaceNeighbor(oldEdge.m_innerPolygon, stitchPoly1);

            ml_polygonSet.Add(stitchPoly1);
            ml_polygonSet.Add(stitchPoly2);
        }

        //Swap to the new vertices on the inner polys.

        foreach (Polygon poly in polys)
        {
            for (int i = 0; i < 3; i++)
            {
                int vertID = poly.mli_vertices[i];

                if (!oldVertices.Contains(vertID))
                    continue;

                int vertIndex = oldVertices.IndexOf(vertID);
                poly.mli_vertices[i] = newVertices[vertIndex];
            }
        }
    }

    private PolygonSet GetHexSet(Polygon poly)
    {
        PolygonSet result = new PolygonSet();
        Polygon p1 = ml_polygonSet[0];
        result.Add(p1);

        Polygon p2 = p1.ml_neighborPolygons[0];
        Polygon p6 = p1.ml_neighborPolygons[1];
        result.Add(p2);
        result.Add(p6);

        Polygon p3 = new Polygon(0, 0, 0);
        Polygon p4 = new Polygon(0, 0, 0);
        Polygon p5 = new Polygon(0, 0, 0);

        foreach (Polygon p2Neighbor in p2.ml_neighborPolygons)
        {
            foreach (Polygon p6Neighbor in p6.ml_neighborPolygons)
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
                foreach (Polygon p2NeighborNeighbor in p2Neighbor.ml_neighborPolygons)
                {
                    foreach (Polygon p6NeighborNeighbor in p6Neighbor.ml_neighborPolygons)
                    {
                        if (p2NeighborNeighbor == p6NeighborNeighbor)
                        {
                            foreach (Polygon unionPoly in result)
                            {
                                if (p2NeighborNeighbor.ml_neighborPolygons.Contains(unionPoly))
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

    private void ExtrudePolygonSet(PolygonSet polys, float height)
    {
        StitchPolygons(polys);

        List<int> uniqueVertices = polys.GetUniqueVertices();

        foreach (int uniqueVertex in uniqueVertices)
        {
            Vector3 vertex = ml_vertices[uniqueVertex];

            vertex = vertex.normalized * (vertex.magnitude + height);

            ml_vertices[uniqueVertex] = vertex;
        }
    }

    private void InsetPolygonSet(PolygonSet polys, float interpolation)
    {
        StitchPolygons(polys);

        List<int> verts = polys.GetUniqueVertices();

        //Calculate the average center of all the vertices
        //in these polygons.

        Vector3 center = Vector3.zero;

        foreach (int vert in verts)
            center += ml_vertices[vert];

        center /= verts.Count;

        //Pull each vertex towards the center, then correct
        //its height so that it's as far from the center of
        //the planet as it was before.

        foreach (int vert in verts)
        {
            Vector3 v = ml_vertices[vert];

            float height = v.magnitude;

            v = Vector3.Lerp(v, center, interpolation);

            v = v.normalized * height;

            ml_vertices[vert] = v;
        }
    }

    private List<Vector3> EnlargePoints(List<Vector3> points, float size)
    {
        List<Vector3> tempPoints = new List<Vector3>();

        foreach (Vector3 point in points)
        {
            Vector3 newVertex = point * size;
            tempPoints.Add(point);
        }

        return points;
    }

    private void RebuildMesh(PolygonSet polygons, List<Vector3> vertices)
    {
        GetComponent<MeshFilter>().mesh = m_mesh = new Mesh();

        List<int> triangles = new List<int>();

        foreach (Polygon poly in polygons)
        {
            foreach (int vertexIndex in poly.mli_vertices)
            {
                triangles.Add(vertexIndex);
            }
        }

        m_mesh.vertices = vertices.ToArray();
        m_mesh.triangles = triangles.ToArray();

        m_mesh.RecalculateBounds();
        m_mesh.RecalculateNormals();
        m_mesh.RecalculateTangents();

        GetComponent<MeshCollider>().sharedMesh = m_mesh;
    }
}
