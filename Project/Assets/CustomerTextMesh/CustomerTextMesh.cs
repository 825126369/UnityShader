using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class CustomerTextMesh : MonoBehaviour
{
    public string m_Text;
    public Color32 m_Color = Color.white;
    public Font m_Font;
    public TextAlignment mTextAlignment;
    public float m_CharacterSize = 1.0f;
    
    private MeshFilter mMeshFilter;
    private MeshRenderer mMeshRenderer;
    private Mesh m_Mesh;

    [System.NonSerialized]
    public int vertexCount;
    [System.NonSerialized]
    public Vector3[] vertices = new Vector3[0];
    [System.NonSerialized]
    public Vector2[] uvs0 = new Vector2[0];
    [System.NonSerialized]
    public Color32[] colors32 = new Color32[0];
    [System.NonSerialized]
    public int[] triangles = new int[0];

    public Action mProperityChangedEvent;

    private CustomerTextMeshAutoSize mCustomerTextMeshAutoSize;

    private void Awake()
    {
        Init();
    }

    private void OnEnable()
    {
        UpdateMesh();
    }

    private void Init()
    {
        if (m_Mesh == null || mMeshFilter == null)
        {
            mMeshFilter = GetComponent<MeshFilter>();
            mMeshRenderer = GetComponent<MeshRenderer>();
            m_Mesh = new Mesh();

            mCustomerTextMeshAutoSize = GetComponent<CustomerTextMeshAutoSize>();

            if (m_Font!= null)
            {
                mMeshRenderer.sharedMaterial = m_Font.material;
            }
        }
    }

#if UNITY_EDITOR
    void EditorInit()
    {
        Init();
        UpdateMesh();
    }

    private void OnValidate()
    {
        if (m_Mesh != null && mMeshFilter == null)
        {
            UpdateMesh();
        }
    }
#endif

    public string text
    {
        get
        {
            return m_Text;
        }

        set
        {
            if (value != m_Text)
            {
                m_Text = value;

                if (mCustomerTextMeshAutoSize)
                {
                    mCustomerTextMeshAutoSize.Build();
                }

                int nLength = GetValidLength();
                ResetMeshSize(nLength);
                UpdateMesh();
            }
        }
    }

    public Font font
    {
        get
        {
            return m_Font;
        }
    }

    public Mesh mesh
    {
        get
        {
            return m_Mesh;
        }
    }

    public float characterSize
    {
        get
        {
            return m_CharacterSize;
        }

        set
        {
            if (value != m_CharacterSize)
            {
                m_CharacterSize = value;
                UpdateMesh();
            }
        }
    }

    public TextAlignment alignment
    {
        get
        {
            return mTextAlignment;
        }
    }

    private float GetWidth()
    {
        float textWidth = 0.0f;
        for (int i = 0; i < m_Text.Length; i++)
        {
            char c = m_Text[i];
            CharacterInfo mCharacterInfo;
            if (m_Font.GetCharacterInfo(c, out mCharacterInfo))
            {
                textWidth += mCharacterInfo.advance;
            }
        }
        return textWidth;
    }

    private int GetValidLength()
    {
        int nLength = 0;
        for (int i = 0; i < m_Text.Length; i++)
        {
            char c = m_Text[i];
            CharacterInfo mCharacterInfo;
            if (m_Font.GetCharacterInfo(c, out mCharacterInfo))
            {
                nLength++;
            }
        }
        return nLength;
    }

    public void ForceUpdateMesh()
    {
        UpdateMesh();
    }

    private void UpdateMesh()
    {
        if (m_Font == null) return;

        int nLength = GetValidLength();
        ResetMeshSize(nLength);

        ResetVertexs();
        ClearUnusedVertices();
        
        m_Mesh.vertices = vertices;
        m_Mesh.uv = uvs0;
        m_Mesh.colors32 = colors32;
        m_Mesh.RecalculateBounds();

        mMeshFilter.sharedMesh = m_Mesh;

        if (mProperityChangedEvent != null)
        {
            mProperityChangedEvent();
        }
    }

    private void AddVertexs(Vector3 pos, Vector2 uv)
    {
        pos *= m_CharacterSize;
        Color32 color32 = Color.white * m_Color;

        vertices[vertexCount] = pos;
        uvs0[vertexCount] = uv;
        colors32[vertexCount] = color32;

        vertexCount++;
    }
        
    private void ResetVertexs()
    {
        float posX = 0f;
        float fWidth = GetWidth();

        if (mTextAlignment == TextAlignment.Left)
        {
            posX = 0f;
        }
        else if (mTextAlignment == TextAlignment.Center)
        {
            posX = -fWidth / 2f;
        }
        else if (mTextAlignment == TextAlignment.Right)
        {
            posX = -fWidth;
        }

        vertexCount = 0;
        for (int i = 0, nLength = m_Text.Length; i < nLength; i++)
        {
            char c = m_Text[i];
            CharacterInfo mCharacterInfo;
            if (m_Font.GetCharacterInfo(c, out mCharacterInfo))
            {
                Vector2 uvBottomLeft = mCharacterInfo.uvBottomLeft;
                Vector2 posBottomLeft = new Vector2(mCharacterInfo.minX, mCharacterInfo.minY);
                posBottomLeft.x += posX;
                AddVertexs(posBottomLeft, uvBottomLeft);

                Vector2 uvTopLeft = mCharacterInfo.uvTopLeft;
                Vector2 posuvTopLeft = new Vector2(mCharacterInfo.minX, mCharacterInfo.maxY);
                posuvTopLeft.x += posX;
                AddVertexs(posuvTopLeft, uvTopLeft);


                Vector2 uvTopRight = mCharacterInfo.uvTopRight;
                Vector2 posuvTopRight = new Vector2(mCharacterInfo.maxX, mCharacterInfo.maxY);
                posuvTopRight.x += posX;
                AddVertexs(posuvTopRight, uvTopRight);

                Vector2 uvBottomRight = mCharacterInfo.uvBottomRight;
                Vector2 posBottomRight = new Vector2(mCharacterInfo.maxX, mCharacterInfo.minY);
                posBottomRight.x += posX;
                AddVertexs(posBottomRight, uvBottomRight);

                posX += mCharacterInfo.advance;
            }
        }
    }

    public void ClearUnusedVertices()
    {
        int length = vertices.Length - vertexCount;

        if (length > 0)
            Array.Clear(vertices, vertexCount, length);
    }

    public void ResetMeshSize(int fSize)
    {
        int nOriLength = vertices.Length / 4;
        int nNowLength = fSize;

        int nOriTrianglesLength = triangles.Length;

        if (nNowLength > nOriLength)
        {
            int nLength = nNowLength * 4;
            Array.Resize<Vector3>(ref vertices, nLength);
            Array.Resize<Color32>(ref colors32, nLength);
            Array.Resize<Vector2>(ref uvs0, nLength);
            Array.Resize<int>(ref triangles, nNowLength * 6);
            
            for (int i = nOriLength; i < nNowLength; i++)
            {
                int nVertexBeginIndex = i * 4;
                int nTriangleBeginIndex = i * 6;
                
                triangles[nTriangleBeginIndex + 0] = nVertexBeginIndex + 0;
                triangles[nTriangleBeginIndex + 1] = nVertexBeginIndex + 1;
                triangles[nTriangleBeginIndex + 2] = nVertexBeginIndex + 2;

                triangles[nTriangleBeginIndex + 3] = nVertexBeginIndex + 2;
                triangles[nTriangleBeginIndex + 4] = nVertexBeginIndex + 3;
                triangles[nTriangleBeginIndex + 5] = nVertexBeginIndex + 0;
            }

            m_Mesh.vertices = vertices;
            m_Mesh.uv = uvs0;
            m_Mesh.colors32 = colors32;
            m_Mesh.triangles = triangles;
        }
    }

}
