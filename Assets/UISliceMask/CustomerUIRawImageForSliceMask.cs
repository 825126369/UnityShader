using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RawImage))]
[DisallowMultipleComponent]
public class CustomerUIRawImageForSliceMask : BaseUISoftSliceMasked
{
    [SerializeField] private Material m_CommonMat;
    private RawImage mImage;

    private Material mMat = null;
    protected override void Start()
    {
        base.Start();
        mImage = GetComponent<RawImage>();
        if (m_CommonMat != null)
        {
            mMat = m_CommonMat;
        }
        else
        {
            mMat = new Material(Shader.Find("Customer/CustomerUIImageSliceMasked"));
        }

        UpdateMask();
        UpdateSelf();
        mImage.material = mMat;
    }

    void LateUpdate()
    {
        UpdateMask();
        UpdateSelf();
    }

    void UpdateSelf()
    {
        mMat.SetFloat("nSliceCount", nSliceCount);
        mMat.SetFloat("nTiledSliceCount", nTiledSliceCount);
        mMat.SetVectorArray("_SliceClipRect", _ClipRectList);
        mMat.SetVectorArray("_SliceAlphaMask_ST", uvScaleOffsetList);
        mMat.SetVectorArray("_TiledCount", _TiledCountList);
        mMat.SetTexture("_MyAlphaMask", m_mask.mainTexture);
    }

}