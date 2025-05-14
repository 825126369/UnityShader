using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用来控制粒子和UI的层级显示
/// sortingOrder 越大显示层级越高
/// </summary>
public class ParticleLayerControl : MonoBehaviour {

    private List<Renderer> particleRenderers = null;
    public void SetOrderInLayer(int ordreInLayer)
    {
        if (particleRenderers != null)
        {
            for (int i = 0; i < particleRenderers.Count; i++)
            {
                Renderer r = particleRenderers[i];
                r.sortingOrder = ordreInLayer;
            }
        }
    }
	// Use this for initialization
	void Awake () {
        particleRenderers = new List<Renderer>();
        ParticleSystem[] subSystems = GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem sys in subSystems)
        {
            Renderer r = sys.GetComponent<Renderer>();
            particleRenderers.Add(r);
        }
    }
}
