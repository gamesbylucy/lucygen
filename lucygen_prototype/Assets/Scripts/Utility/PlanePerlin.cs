using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanePerlin : MonoBehaviour {

    Perlin perlin = new Perlin();

	// Use this for initialization
	void Start () {
        
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnDrawGizmos()
    {
        for (float i = 0; i < 1; i = i + .1f)
        {
            for (float j = 0; j < 1; j = j + .1f)
            {
                for (float k = 0; k < 1; k = k + .1f)
                {
                    float height = (float)perlin.OctavePerlin(i, 0, k, 6, .5f);
                    Vector3 newVert = new Vector3(i, height, k);
                    Gizmos.DrawSphere(newVert, .01f);
                }
            }
        }
    }
}
