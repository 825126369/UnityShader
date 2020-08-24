using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BOp : MonoBehaviour
{
    void Start()
    {
        int a = -1;
        int b = 1;
        uint c = (uint)a;
        int d = (int)((uint)a - (uint)b);
        Debug.Log("c : " + c);
        Debug.Log("d : " + d);
        Debug.Log("intMax : " + int.MaxValue);
        Debug.Log("uintMax : " + uint.MaxValue);
    }
    
    void Update()
    {
        
    }
}
