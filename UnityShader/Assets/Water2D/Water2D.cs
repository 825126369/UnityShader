using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Water2D : MonoBehaviour 
{
    public float fWaveDuration = 2.0f;
    private Material mMaterial;
    private List<BoxCollider2D> mBoxList = new List<BoxCollider2D>();
    private const int nVectorBufferCount = 100;
    private List<Vector4> mCenterClickPointUVList = new List<Vector4>(nVectorBufferCount);
    private Vector4[] mCenterClickPointUVArray = new Vector4[nVectorBufferCount];
    private List<int> tableRemoveIndexs = new List<int>(nVectorBufferCount);
    private SpriteRenderer mSprite;
    
    void Start()
    {
        mSprite = transform.GetComponent(typeof(SpriteRenderer)) as SpriteRenderer;
        mMaterial = mSprite.sharedMaterial;

        SetNoiseTexture();
        mMaterial.SetTexture("_GradTex", GetGradTexture());
        mMaterial.SetTexture("_MainTex", mSprite.sprite.texture);

        GetComponentsInChildren(true, mBoxList);

        ToTrray();
        mMaterial.SetFloat("_WaveCenters_Num", mCenterClickPointUVList.Count);
        mMaterial.SetVectorArray("_WaveCenters", mCenterClickPointUVArray);

        mMaterial.SetFloat("_Aspect", mSprite.bounds.size.x / mSprite.bounds.size.y);
        mMaterial.SetFloat("_WaveDuration", fWaveDuration);
    }

    Texture2D GetGradTexture()
    {
        AnimationCurve waveform = new AnimationCurve(
            new Keyframe(0.00f, 0.50f, 0, 0),
            new Keyframe(0.05f, 1.00f, 0, 0),
            new Keyframe(0.15f, 0.80f, 0, 0),
            new Keyframe(0.35f, 0.30f, 0, 0),
            new Keyframe(0.45f, 0.60f, 0, 0),
            new Keyframe(0.55f, 0.40f, 0, 0),
            new Keyframe(0.65f, 0.55f, 0, 0),
            new Keyframe(0.75f, 0.46f, 0, 0),
            new Keyframe(0.85f, 0.52f, 0, 0),
            new Keyframe(0.99f, 0.50f, 0, 0)
        );

        Texture2D gradTexture = new Texture2D(2048, 1, TextureFormat.Alpha8, false);
        gradTexture.wrapMode = TextureWrapMode.Clamp;
        gradTexture.filterMode = FilterMode.Bilinear;
        for (int i = 0; i < gradTexture.width; i++)
        {
            float x = 1.0f / gradTexture.width * i;
            float a = waveform.Evaluate(x);
            gradTexture.SetPixel(i, 0, new Color(a, a, a, a));
        }

        gradTexture.Apply();

        return gradTexture;
    }

    void SetNoiseTexture()
    {
        float scale1 = 2f;
        float scale2 = 3;
        float scale3 = 6;
        Texture2D _NoiseTex1 = NoiseTextureCreater.GetNoiseTexture(scale1);
        Texture2D _NoiseTex2 = NoiseTextureCreater.GetNoiseTexture(scale2);
        Texture2D _NoiseTex = NoiseTextureCreater.GetNoiseTextureByScaleAdd(scale1, scale2, scale3);
        mMaterial.SetTexture("_NoiseTex1", _NoiseTex1);
        mMaterial.SetTexture("_NoiseTex2",_NoiseTex2);
        mMaterial.SetTexture("_NoiseTex", _NoiseTex);
    }

    void OnRenderImage(Texture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, mMaterial);
    }

    void ToTrray()
    {
        for(int i = 0; i < nVectorBufferCount; i++)
        {
            if (i < mCenterClickPointUVList.Count)
            {
                mCenterClickPointUVArray[i] = mCenterClickPointUVList[i];
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Ray mRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit2D info = Physics2D.GetRayIntersection(mRay, float.MaxValue);

            if (info.collider)
            {
                BoxCollider2D box = info.collider as BoxCollider2D;
                if (mBoxList.Contains(box))
                {
                    Vector3 worldPos = new Vector3(info.point.x, info.point.y, mSprite.transform.position.z);
                    Vector3 mLocalPos = worldPos - mSprite.bounds.min;
                    float clickUVX = mLocalPos.x / mSprite.bounds.size.x;
                    float clickUVY = mLocalPos.y / mSprite.bounds.size.y;
                    Vector4 centerUV = new Vector4(clickUVX, clickUVY, Time.time, 0);
                    mCenterClickPointUVList.Add(centerUV);
                    
                    ToTrray();
                    mMaterial.SetVectorArray("_WaveCenters", mCenterClickPointUVArray);
                    mMaterial.SetFloat("_WaveCenters_Num", mCenterClickPointUVList.Count);
                }
            }

        }
        
        for (int i = 0; i < mCenterClickPointUVList.Count; i++)
        {
            float fLastTime = mCenterClickPointUVList[i].z;
            if (Time.time - fLastTime >= fWaveDuration)
            {
                tableRemoveIndexs.Add(i);
            }
        }

        for (int i = tableRemoveIndexs.Count - 1; i >= 0; --i)
        {
            int nRemoveIndex = tableRemoveIndexs[i];
            mCenterClickPointUVList.RemoveAt(nRemoveIndex);
        }
        
        if (tableRemoveIndexs.Count > 0)
        {
            mMaterial.SetFloat("_WaveCenters_Num", mCenterClickPointUVList.Count);
            if (mCenterClickPointUVList.Count > 0)
            {
                ToTrray();
                mMaterial.SetVectorArray("_WaveCenters", mCenterClickPointUVArray);
            }

            tableRemoveIndexs.Clear();
        }

    }

}
