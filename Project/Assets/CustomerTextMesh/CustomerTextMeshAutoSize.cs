using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(CustomerTextMesh))]
public class CustomerTextMeshAutoSize : MonoBehaviour
{
    [SerializeField]
    private float m_maxWidth;
    [SerializeField]
    private float m_maxCharacterSize;

    private CustomerTextMesh m_textMesh;

    // Use this for initialization
    void Start () {
        m_textMesh = GetComponent<CustomerTextMesh>();
        Build();
    }

    public float maxWitdh
    {
        get { return m_maxWidth; }
        set
        {
            m_maxWidth = value;
            Build();
        }
    }

    public float maxCharacterSize
    {
        get { return m_maxCharacterSize; }
        set
        {
            m_maxCharacterSize = value;
            Build();
        }
    }

    public void Build()
    {
        float width = 0;
        foreach (char symbol in m_textMesh.text)
        {
            CharacterInfo info;
            if (m_textMesh.font.GetCharacterInfo(symbol, out info))
            {
                width += info.advance;
            }
        }

        if (width > 0)
        {
            width *= m_textMesh.transform.lossyScale.x;

            // width 如果等于0 会报错： 所以 得把 text 事先赋值
            float preferCharacterSize = m_maxWidth / width;
            m_textMesh.characterSize = m_maxCharacterSize < preferCharacterSize ? m_maxCharacterSize : preferCharacterSize;
        }
    }

    void Update()
    {
        Build();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        float XLeft = 0f;

        if (m_textMesh.mTextAlignment == TextAlignment.Left)
        {
            XLeft = transform.position.x;
        }
        else if (m_textMesh.mTextAlignment == TextAlignment.Center)
        {
            XLeft = transform.position.x - m_maxWidth / 2f;
        }
        else
        {
            XLeft = transform.position.x - m_maxWidth;
        }
        
        float yPos = transform.position.y;
        Gizmos.DrawLine(new Vector3(XLeft, yPos, transform.position.z), new Vector3(XLeft + m_maxWidth, yPos, transform.position.z));
    }
#endif

}
