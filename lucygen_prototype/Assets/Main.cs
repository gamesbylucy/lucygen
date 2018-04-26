using UnityEngine;
using System.Collections.Generic;
using Delaunay;
using Delaunay.Geo;
using System.Linq;
using Assets.Map;

public class Main : MonoBehaviour
{
    Map _map;
    const int _textureScale = 50;
    GameObject _selector;
    public bool Regenerate;
    public int Seed;
    public float PerlinCheckValue = 0.3f;

    void Update()
    {
        if (Regenerate)
        {
            Regenerate = false;
            Awake();
        }
    }

	void Awake ()
	{
        IslandShape.PERLIN_CHECK_VALUE = PerlinCheckValue;

        Random.InitState(Seed);
            
        _map = new Map();

        new MapTexture(_textureScale).AttachTexture(GameObject.Find("Map"), _map);
	}
}