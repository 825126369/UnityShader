using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class SortingLayerExposer : MonoBehaviour 
{
	public string SortingLayerName = "Default";
	public int SortingOrder = 0;
	
	void Update ()
	{
		gameObject.GetComponent<Renderer> ().sortingLayerName = SortingLayerName;
		gameObject.GetComponent<Renderer> ().sortingOrder = SortingOrder;
	}
}
