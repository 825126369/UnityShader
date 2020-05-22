using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionBlur : MonoBehaviour
{
    public float fSpeed = 10f;
    void Start()
    {
        
    }
    
    void Update()
    {
        transform.position -= new Vector3(0, Time.deltaTime * fSpeed, 0);
    }

}
