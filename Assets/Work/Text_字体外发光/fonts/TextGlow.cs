using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextGlow : BaseMeshEffect
{
    [SerializeField]
    private Color m_EffectColor = new Color(0f, 0f, 0f, 0.5f);
    [SerializeField]
    private Vector2 m_EffectDistance = new Vector2(1f, -1f);
    [SerializeField]
    private float m_Scale = 1f;
    [SerializeField]
    private bool m_UseGraphicAlpha = true;

    private const float kMaxEffectDistance = 600f;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        effectDistance = m_EffectDistance;
        base.OnValidate();
    }

#endif
    /// <summary>
    /// Color for the effect
    /// </summary>
    public Color effectColor
    {
        get { return m_EffectColor; }
        set
        {
            m_EffectColor = value;
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

    /// <summary>
    /// How far is the shadow from the graphic.
    /// </summary>
    public Vector2 effectDistance
    {
        get { return m_EffectDistance; }
        set
        {
            if (value.x > kMaxEffectDistance)
                value.x = kMaxEffectDistance;
            if (value.x < -kMaxEffectDistance)
                value.x = -kMaxEffectDistance;

            if (value.y > kMaxEffectDistance)
                value.y = kMaxEffectDistance;
            if (value.y < -kMaxEffectDistance)
                value.y = -kMaxEffectDistance;

            if (m_EffectDistance == value)
                return;

            m_EffectDistance = value;

            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

    /// <summary>
    /// Should the shadow inherit the alpha from the graphic?
    /// </summary>
    public bool useGraphicAlpha
    {
        get { return m_UseGraphicAlpha; }
        set
        {
            m_UseGraphicAlpha = value;
            if (graphic != null)
                graphic.SetVerticesDirty();
        }
    }

    protected void ApplyShadowZeroAlloc(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
    {
        UIVertex vt;

        var neededCapacity = verts.Count + end - start;
        if (verts.Capacity < neededCapacity)
            verts.Capacity = neededCapacity;

        for (int i = start; i < end; ++i)
        {
            vt = verts[i];
            verts.Add(vt);

            Vector3 v = vt.position;
            v.x += x;
            v.y += y;
            vt.position = v;
            var newColor = color;
            if (m_UseGraphicAlpha)
                newColor.a = (byte)((newColor.a * verts[i].color.a) / 255);
            vt.color = newColor;
            verts[i] = vt;
        }
    }

    /// <summary>
    /// Duplicate vertices from start to end and turn them into shadows with the given offset.
    /// </summary>
    /// <param name="verts">Vert list to copy</param>
    /// <param name="color">Shadow color</param>
    /// <param name="start">The start index in the verts list</param>
    /// <param name="end">The end index in the vers list</param>
    /// <param name="x">The shadows x offset</param>
    /// <param name="y">The shadows y offset</param>
    protected void ApplyShadow(List<UIVertex> verts, Color32 color, int start, int end, float x, float y)
    {
        ApplyShadowZeroAlloc(verts, color, start, end, x, y);
    }

    /*
        AB
        CD
        两个三角形，分别是ABD DCA
    */
    protected void ApplyScale(List<UIVertex> m_VetexList, int nOriCount, Color32 color, float fScale)
    {
        int i = 0;
        while (i < nOriCount)
        {
            UIVertex v1 = m_VetexList[i];
            UIVertex v2 = m_VetexList[i + 1];
            UIVertex v3 = m_VetexList[i + 2];
            UIVertex v4 = m_VetexList[i + 3];
            UIVertex v5 = m_VetexList[i + 4];
            UIVertex v6 = m_VetexList[i + 5];

            m_VetexList.Add(v1);
            m_VetexList.Add(v2);
            m_VetexList.Add(v3);
            m_VetexList.Add(v4);
            m_VetexList.Add(v5);
            m_VetexList.Add(v6);

            Vector3 max = m_VetexList[i + 1].position;
            Vector3 min = m_VetexList[i + 4].position;
            Bounds mBonuds = new Bounds();
            mBonuds.SetMinMax(min, max);
            mBonuds.Expand(fScale);
            v1.position = new Vector3(mBonuds.min.x, mBonuds.max.y, 0);
            v2.position = new Vector3(mBonuds.max.x, mBonuds.max.y, 0);
            v3.position = new Vector3(mBonuds.max.x, mBonuds.min.y, 0);
            v4.position = new Vector3(mBonuds.max.x, mBonuds.min.y, 0);
            v5.position = new Vector3(mBonuds.min.x, mBonuds.min.y, 0);
            v6.position = new Vector3(mBonuds.min.x, mBonuds.max.y, 0);
            v1.color = color;
            v2.color = color;
            v3.color = color;
            v4.color = color;
            v5.color = color;
            v6.color = color;

            m_VetexList[i] = v1;
            m_VetexList[i + 1] = v2;
            m_VetexList[i + 2] = v3;
            m_VetexList[i + 3] = v4;
            m_VetexList[i + 4] = v5;
            m_VetexList[i + 5] = v6;
            i += 6;
        }
    }

    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        var output = ListPool<UIVertex>.Get();
        vh.GetUIVertexStream(output);

        ApplyShadow(output, effectColor, 0, output.Count, effectDistance.x, effectDistance.y);
        ApplyScale(output, output.Count, effectColor, m_Scale);
        vh.Clear();
        vh.AddUIVertexTriangleStream(output);
        ListPool<UIVertex>.Release(output);
    }
}
