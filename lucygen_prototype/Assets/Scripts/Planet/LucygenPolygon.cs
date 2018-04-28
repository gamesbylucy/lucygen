using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LucygenPolygon {

    public List<int> m_Vertices;

    public LucygenPolygon(int a, int b, int c)
    {
        m_Vertices = new List<int>() { a, b, c };
    }

    public void assignFace(int a, int b, int c)
    {
        m_Vertices = new List<int>() { a, b, c };
    }
}
