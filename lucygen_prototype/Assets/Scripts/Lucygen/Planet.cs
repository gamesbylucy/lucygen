using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class Planet : MonoBehaviour {

    //PUBLIC MEMEBERS
    public int i_recursions = 3;
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
    private Icosphere icosphere;
    private Mesh m_mesh;
    private static Perlin m_perlin;
    private double md_greatestPolyHeight = 0;
    private double md_leastPolyHeight = 0;

    //UNITY NATIVE

    public void Awake()
    {

    }

    public void Start()
    {
        icosphere = new Icosphere(i_size, i_recursions);
        icosphere.polygons.CalculatePolygonNeighbors();

        double[] heightmap = GetPerlinHeightmap(icosphere);

        /*for (int i = 0; i < heightMap.Length; i++)
        {
            PolygonSet set = new PolygonSet();
            set.Add(planet.polygons[i]);
            //InsetPolygonSet(set, 0.1f);
            ExtrudePolygonSet(set, (float)heightMap[i]);
            //InsetPolygonSet(set, 1f);
        }*/

        ExtrudeRandomPolygons(2, icosphere.polygons, 0, 1);

        //EnlargePoints(planet.vertices, i_size);

        RebuildMesh(icosphere.polygons, icosphere.vertices);

        EventLog.Add("TRIANGLE VERTEX ORDER");
        EventLog.Add("----------------------");
        foreach (int triangle in m_mesh.triangles)
        {
            EventLog.Add(triangle.ToString());
        }

        EventLog.Add("MESH STATISTICS");
        EventLog.Add("Total Number of Polys: " + icosphere.polygons.Count);
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

    private double[] GetPerlinHeightmap(Icosphere icosphere)
    {
        double[] result = new double[icosphere.polygons.Count];
        double[] mad_randomNoise;

        mad_randomNoise = GetRandomDoubles(icosphere.polygons.Count * 3);

        double potentialGreatestPolyHeight = 0;
        double potentialLeastPolyHeight = 0;
        double scaledPolyHeight = 0;

        int heightMapIndex = 0;
        int randomIndex = 0;

        foreach (Polygon poly in icosphere.polygons)
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
            {
                md_greatestPolyHeight = potentialGreatestPolyHeight;
            }

            if (potentialLeastPolyHeight <= md_greatestPolyHeight)
            {
                md_leastPolyHeight = potentialLeastPolyHeight;
            }
               
            result[heightMapIndex++] = scaledPolyHeight;

            EventLog.Add("Unscaled perlin height : " + unscaledPolyHeight);
            EventLog.Add("Scaled perlin height : " + scaledPolyHeight);
        }

        return result;
    }

    //HELPER METHODS

    private void ExtrudeRandomPolygons(int numPolygons, PolygonSet polygons, float minHeight, float maxHeight)
    {
        System.Random random = new System.Random();

        PolygonSet extrudedPolygons = new PolygonSet();
        extrudedPolygons.Add(polygons[random.Next(0, polygons.Count - 1)]);

        extrudedPolygons = icosphere.polygons.GetRandomClusterSet(numPolygons);

        ExtrudePolygonSet(extrudedPolygons, (float)(random.NextDouble() * (maxHeight - minHeight) + minHeight));
        InsetPolygonSet(extrudedPolygons, 1f);
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

    //helper methods
   

    private List<int> CloneVertices(List<int> oldVerts)
    {
        List<int> result = new List<int>();

        foreach (int oldVert in oldVerts)
        {
            Vector3 clonedVert = new Vector3(icosphere.vertices[oldVert].x, icosphere.vertices[oldVert].y, icosphere.vertices[oldVert].z);
            result.Add(icosphere.vertices.Count);
            icosphere.vertices.Add(clonedVert);
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

            icosphere.polygons.Add(stitchPoly1);
            icosphere.polygons.Add(stitchPoly2);
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
        Polygon p1 = icosphere.polygons[0];
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
            Vector3 vertex = icosphere.vertices[uniqueVertex];

            vertex = vertex.normalized * (vertex.magnitude + height);

            icosphere.vertices[uniqueVertex] = vertex;
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
            center += icosphere.vertices[vert];

        center /= verts.Count;

        //Pull each vertex towards the center, then correct
        //its height so that it's as far from the center of
        //the planet as it was before.

        foreach (int vert in verts)
        {
            Vector3 v = icosphere.vertices[vert];

            float height = v.magnitude;

            v = Vector3.Lerp(v, center, interpolation);

            v = v.normalized * height;

            icosphere.vertices[vert] = v;
        }
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
