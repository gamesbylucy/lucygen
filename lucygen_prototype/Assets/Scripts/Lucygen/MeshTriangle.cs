using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTriangle {

    public int m_vertA, m_vertB, m_vertC;

    public int[] m_vertices;

    public MeshTriangle(int a, int b, int c)
    {
        m_vertA = a;
        m_vertB = b;
        m_vertC = c;
        m_vertices = [a, b, c];
    }
}
