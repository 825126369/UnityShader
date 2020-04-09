using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PlaneShadow : MonoBehaviour
{
    public float fDistance = 1;
    public Vector3 LigthDir = -Vector3.up;
    public Vector3 PlaneNormalDir = Vector3.up;
    public Transform SureTargetShadowWorldPos = null;
    public GameObject goAni = null;

    public float _ShadowFalloff = 0.01f;
    public float _ShadowInvLen = 1.0f;
    public Vector4 _ShadowFadeParams = new Vector4(500f, 0.0f, 0.2f, 0.0f);
    
    Dictionary<Renderer, MaterialPropertyBlock> mMatList = new Dictionary<Renderer, MaterialPropertyBlock>();

    float fMaxJump = 2;
    float fJumpSpeed = 10;
    bool bJumpToUp = false;
    bool bJumpToDown = false;

    float fWalkSpeed = 5;

    private void Start()
    {
        Renderer[] renderList = transform.GetComponentsInChildren<Renderer>();
        foreach (var render in renderList)
        {
            MaterialPropertyBlock mBlock = new MaterialPropertyBlock();
            mMatList[render] = mBlock;
        }

    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.Space) && bJumpToUp == false && bJumpToDown == false)
        {
            bJumpToUp = true;
            bJumpToDown = false;
        }

        if (bJumpToUp)
        {
            goAni.transform.localPosition += new Vector3(0, fJumpSpeed * Time.deltaTime, 0);
            if (goAni.transform.localPosition.y >= fMaxJump)
            {
                bJumpToUp = false;
                bJumpToDown = true;
            }
        }else if (bJumpToDown)
        {
            goAni.transform.localPosition -= new Vector3(0, fJumpSpeed * Time.deltaTime, 0);
            if (goAni.transform.localPosition.y <= 0)
            {
                goAni.transform.localPosition = Vector3.zero;
                bJumpToUp = false;
                bJumpToDown = false;
            }
        }

        if (Input.GetKey(KeyCode.A) )
        {
            transform.localPosition += new Vector3(-fWalkSpeed * Time.deltaTime, 0, 0);
        }

        if (Input.GetKey(KeyCode.D))
        {
            transform.localPosition += new Vector3(fWalkSpeed * Time.deltaTime, 0, 0);
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.localPosition += new Vector3(0, 0, fWalkSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            transform.localPosition += new Vector3(0, 0, -fWalkSpeed * Time.deltaTime);
        }

    }

    // Update is called once per frame
    void LateUpdate()
    {
        SetShadowShader();
    }

    void SetShadowShader()
    {
        Vector4 worldpos = SureTargetShadowWorldPos.position;
        Vector4 _ShadowPlane = new Vector4(PlaneNormalDir.x, PlaneNormalDir.y, PlaneNormalDir.z, fDistance);
        Vector4 projdir = LigthDir;

        foreach (var keyValue in mMatList)
        {
            Renderer render = keyValue.Key;
            MaterialPropertyBlock mBlock = keyValue.Value;
            mBlock.SetVector("_WorldPos", worldpos);
            mBlock.SetVector("_ShadowProjDir", projdir);
            mBlock.SetVector("_ShadowPlane", _ShadowPlane);
            mBlock.SetVector("_ShadowFadeParams", _ShadowFadeParams);
            mBlock.SetFloat("_ShadowFalloff", _ShadowFalloff);
            mBlock.SetFloat("_ShadowInvLen", _ShadowInvLen);
            render.SetPropertyBlock(mBlock);
        }
    }

}
