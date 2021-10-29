using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

[ExecuteAlways]
public class LeanTweenBezier : MonoBehaviour
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

            for(int i = 0; i < mLineRenderer.transform.childCount; i++ )
            {
                posObj.Add(mLineRenderer.transform.GetChild(i).gameObject);
            }
            
            if ((posObj.Count - 1) % 2 != 0) return;

            mLineRenderer.positionCount = posObj.Count;
            for (int i = 0; i < posObj.Count; i++)
            {
                mLineRenderer.SetPosition(i, posObj[i].transform.position);
            }

            int k = 0;
            for (int i = 0; i < posObj.Count; i++)
            {
                if (k == 1)
                {
                    posList.Add(posObj[i].transform.position);
                    posList.Add(posObj[i].transform.position);
                }
                else
                {
                    posList.Add(posObj[i].transform.position);
                }

                k++;
                if (k == 3)
                {
                    k = 0;
                    if (i < posObj.Count - 1)
                    {
                        i--;
                    }
                }
            }

            mLineRenderer1.positionCount = 500;
            Debug.Assert(posList.Count % 4 == 0, posList.Count + " | " + posObj.Count);
            ltd = LeanTween.move(gameObject, posList.ToArray(), 2.0f).setLoopCount(-1).setOnUpdate((Vector3 pos)=>
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

