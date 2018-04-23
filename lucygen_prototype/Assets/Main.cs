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
        if (_map != null && _map.SelectedCenter != null)
        {
            _selector.transform.localPosition = new Vector3(_map.SelectedCenter.point.x, _map.SelectedCenter.point.y, 1);
        }
        if (Regenerate)
        {
            Regenerate = false;
            Awake();
        }
    }

	void Awake ()
	{
        IslandShape.PERLIN_CHECK_VALUE = PerlinCheckValue;

        _selector = GameObject.Find("Selector");

        Random.seed = Seed;
            
        _map = new Map();

        GameObject.Find("Main Camera").GetComponentInChildren<Camera>().Map = _map;

        new MapTexture(_textureScale).AttachTexture(GameObject.Find("Map"), _map);
	}
}