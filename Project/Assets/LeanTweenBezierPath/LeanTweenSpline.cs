using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

[ExecuteAlways]
public class LeanTweenSpline : MonoBehaviour
{
    public LineRenderer mLineRenderer;
    private LineRenderer mLineRenderer1;
    private List<GameObject> posObj = new List<GameObject>();
    public bool bChanged = true;
    List<Vector3> posList = new List<Vector3>();

    private void Start()
    {
        mLineRenderer1 = GetComponent<LineRenderer>();
    }

    private int ltd = -1;
    private int nIndex = 0;
    private void Update()
    {
        if (bChanged)
        {
            bChanged = false;
            posObj.Clear();
            posList.Clear();
            mLineRenderer.positionCount = 0;
            nIndex = 0;
            if (ltd > 0 && LeanTween.isTweening(ltd))
            {
                LeanTween.cancel(ltd);
                ltd = -1;
            }

            if (!mLineRenderer) return;

            for (int i = 0; i < mLineRenderer.transform.childCount; i++)
            {
                posObj.Add(mLineRenderer.transform.GetChild(i).gameObject);
            }

            mLineRenderer.positionCount = posObj.Count;
            for (int i = 0; i < posObj.Count; i++)
            {
                mLineRenderer.SetPosition(i, posObj[i].transform.position);
            }
            
            for (int i = 0; i < posObj.Count; i++)
            {
                posList.Add(posObj[i].transform.position);
            }

            mLineRenderer1.positionCount = 500;
            ltd = LeanTween.moveSpline(gameObject, posList.ToArray(), 10.0f).setLoopCount(-1).setOnUpdate((Vector3 pos) =>
            {
                if (nIndex < 500)
                {
                    mLineRenderer1.SetPosition(nIndex, transform.position);
                    nIndex++;
                }

            }).id;
        }
    }

}

