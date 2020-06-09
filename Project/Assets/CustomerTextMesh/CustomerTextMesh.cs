using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[ExecuteInEditMode]
public class CustomerTextMesh : MonoBehaviour
{
    public string m_Text;
    public Font m_Font;
    public TextAlignment mTextAlignment;
    public int FontSize;
    
    private MeshFilter mMeshFilter;
    private MeshRenderer mMeshRenderer;
    private Mesh mMesh;
    
    private List<Vector3> vertices = new List<Vector3>();
    private List<Vector2> uvs0 = new List<Vector2>();
    private List<int> triangles = new List<int>();

    private void Start()
    {
        mMeshFilter = GetComponent<MeshFilter>();
        mMeshRenderer = GetComponent<MeshRenderer>();
        mMesh = new Mesh();
    }

    private void Update()
    {
        UpdateMesh();
        UpdateMaterial();
    }

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
                UpdateMesh();
            }
        }
    }

    private float GetWidth()
    {
        float textWidth = 0.0f;
        for(int i = 0; i < m_Text.Length; i++)
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

    private void UpdateMesh()
    {
        ResetVertexs();
        ResetTriangles();

        mMesh.Clear(false);
        mMesh.SetVertices(vertices);
        mMesh.SetUVs(0, uvs0);
        mMesh.SetTriangles(triangles, 0);
        mMesh.RecalculateBounds();

        mMeshFilter.sharedMesh = mMesh;
    }

    private void UpdateMaterial()
    {
        mMeshRenderer.sharedMaterial = m_Font.material;
    }

    private void AddVertexs(Vector3 pos, Vector2 uv)
    {
        vertices.Add(pos);
        uvs0.Add(uv);
    }


    private void ResetVertexs()
    {
        vertices.Clear();
        uvs0.Clear();

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

    private void ResetTriangles()
    {
        int nOriLength = triangles.Count / 6;
        int nNowLength = GetValidLength();
        if (nNowLength > nOriLength)
        {
            for(int i = nOriLength; i < nNowLength; i++)
            {
                int nBeginIndex = i * 4;
                triangles.Add(nBeginIndex + 0);
                triangles.Add(nBeginIndex + 1);
                triangles.Add(nBeginIndex + 2);

                triangles.Add(nBeginIndex + 2);
                triangles.Add(nBeginIndex + 3);
                triangles.Add(nBeginIndex + 0);
            }
        }
        else if (nNowLength < nOriLength)
        {
            triangles.RemoveRange(nNowLength, nOriLength - nNowLength);
        }
    }

}
