// RandomMapMaker.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class RandomMapMaker : MonoBehaviour
{

    private float _seedX, _seedZ;

    [SerializeField]
    private int _width = 50;
    [SerializeField]
    private int _depth = 50;

    [SerializeField]
    private bool _needToCollider = false;

    [SerializeField]
    private float _maxHeight = 10;

    [SerializeField]
    private bool _isPerlinNoiseMap = true;

    [SerializeField]
    private float _relief = 15f;

    [SerializeField]
    private bool _isSmoothness = false;

    [SerializeField]
    private float _mapSize = 1f;

    private GameObject[,] cubeList = null;

    private void Init()
    {
        if (cubeList == null || cubeList.GetLength(0) != _width || cubeList.GetLength(1) != _depth)
        {
            if (cubeList != null)
            {
                for (int x = 0; x < _width; x++)
                {
                    for (int z = 0; z < _depth; z++)
                    {
                        GameObject cube = cubeList[x, z];
                        DestroyImmediate(cube);
                    }
                }

                while(transform.childCount > 0)
                {
                    GameObject cube = transform.GetChild(0).gameObject;
                    DestroyImmediate(cube);
                }
            }

            cubeList = new GameObject[_width, _depth];
            for (int x = 0; x < _width; x++)
            {
                for (int z = 0; z < _depth; z++)
                {
                    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.name = "cube[" + x + "," + z + "]";
                    cubeList[x, z] = cube;
                }
            }
        }
    }

    private void Start()
    {
        Init();
        transform.localScale = new Vector3(_mapSize, _mapSize, _mapSize);

        _seedX = Random.value * 100f;
        _seedZ = Random.value * 100f;

        for (int x = 0; x < _width; x++)
        {
            for (int z = 0; z < _depth; z++)
            {
                GameObject cube = cubeList[x, z];
                cube.transform.localPosition = new Vector3(x, 0, z);
                cube.transform.SetParent(transform);
                if (!_needToCollider)
                {
                    Destroy(cube.GetComponent<BoxCollider>());
                }

                SetY(cube);
            }
        }
    }

    private void SetY(GameObject cube)
    {
        float y = 0;

        if (_isPerlinNoiseMap)
        {
            float xSample = (cube.transform.localPosition.x + _seedX) / _relief;
            float zSample = (cube.transform.localPosition.z + _seedZ) / _relief;
            float noise = Mathf.PerlinNoise(xSample, zSample);
            y = _maxHeight * noise;
        }

        else
        {
            y = Random.Range(0, _maxHeight);
        }

        if (!_isSmoothness)
        {
            y = Mathf.Round(y);
        }

        cube.transform.localPosition = new Vector3(cube.transform.localPosition.x, y, cube.transform.localPosition.z);

        Color color = Color.black;
        if (y > _maxHeight * 0.3f)
        {
            ColorUtility.TryParseHtmlString("#019540FF", out color);
        }
        else if (y > _maxHeight * 0.2f)
        {
            ColorUtility.TryParseHtmlString("#2432ADFF", out color);
        }
        else if (y > _maxHeight * 0.1f)
        {
            ColorUtility.TryParseHtmlString("#D4500EFF", out color);
        }
        cube.GetComponent<MeshRenderer>().material.color = color;
    }
}