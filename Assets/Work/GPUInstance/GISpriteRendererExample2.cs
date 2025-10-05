using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteAlways]
public class GISpriteRendererExample2 : MonoBehaviour
{
    public Material instancedMaterial;
    public Texture2D spriteTexture;
    public int instanceCount = 10000;
    private Mesh mesh;
    private Matrix4x4[] matrices;
    private Vector4[] colors;
    private Vector4[] _Flip;
    private Vector4[] transforms;

    GraphicsBuffer commandBuf;
    GraphicsBuffer.IndirectDrawIndexedArgs[] commandData;
    GraphicsBuffer positionBuffer;

    void Start()
    {
        if (SystemInfo.supportsComputeShaders)
        {
            Debug.Log("支持 ComputeBuffer 和 Compute Shader");
        }
        else
        {
            Debug.LogError("不支持 ComputeBuffer，使用降级方案");
        }
        
        if (instancedMaterial == null || spriteTexture == null)
        {
            return;
        }

        instancedMaterial.SetTexture("_MainTex", spriteTexture);

        mesh = new Mesh();
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0),
            new Vector3(1, 0, 0)
        };
        int[] tris = new int[6] { 0, 1, 2, 2, 1, 3 };
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0)
        };

        mesh.SetVertices(vertices);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateBounds();

        matrices = new Matrix4x4[instanceCount];
        colors = new Vector4[instanceCount];
        _Flip = new Vector4[instanceCount];
        transforms = new Vector4[instanceCount];

        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 pos = Random.insideUnitCircle * 50f; // 分布在 100x100 区域
            Vector2 scale = Vector2.one * Random.Range(0.5f, 1.5f);

            matrices[i] = Matrix4x4.identity;
            matrices[i].SetTRS(
                new Vector3(Random.Range(-10, 10), Random.Range(-10, 10), Random.Range(-10, 10)),
                Quaternion.identity,
                Vector3.one);

            colors[i] = Random.ColorHSV(0.5f, 1f, 1f, 1f, 0.8f, 1f, 1f, 1f);
            _Flip[i] = Vector4.one;
            transforms[i] = new Vector4(pos.x, pos.y, scale.x, scale.y);
        }

        positionBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, instanceCount, 64); // 每个矩阵16个float
        positionBuffer.SetData(matrices);

        commandBuf = new GraphicsBuffer( GraphicsBuffer.Target.IndirectArguments, instanceCount, GraphicsBuffer.IndirectDrawIndexedArgs.size);
        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[2];

    }

    void Update()
    {
        Debug.Assert(mesh != null, "mesh == null");
        Debug.Assert(instancedMaterial != null, "instancedMaterial == null");
        if (mesh == null || instancedMaterial == null)
            return;

        // 创建 MaterialPropertyBlock 并设置数组
        MaterialPropertyBlock props = new MaterialPropertyBlock();
        props.SetTexture("_MainTex", spriteTexture);
        props.SetVectorArray("unity_InstanceColor", colors);
        props.SetVectorArray("unity_InstanceTransform", transforms);
        props.SetVectorArray("unity_SpriteRendererColorArray", colors);
        props.SetVectorArray("unity_SpriteFlipArray", _Flip);
        props.SetMatrix("_ObjectToWorld", Matrix4x4.Translate(new Vector3(-4.5f, 0, 0)));
        props.SetBuffer("_Transforms", positionBuffer);

        RenderParams rp = new RenderParams();
        rp.material = instancedMaterial;
        rp.worldBounds = new Bounds(Vector3.zero, 10000 * Vector3.one);
        rp.matProps = props;

        commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[2];
        commandData[0].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[0].instanceCount = (uint)instanceCount;
        commandData[1].indexCountPerInstance = mesh.GetIndexCount(0);
        commandData[1].instanceCount = (uint)instanceCount;
        commandBuf.SetData(commandData);
        Graphics.RenderMeshIndirect(rp, mesh, commandBuf, commandData.Length);
    }

    void OnDisable()
    {
        commandBuf?.Release();
        commandBuf = null;

        if (mesh != null)
        {
            DestroyImmediate(mesh);
            mesh = null;
        }
    }
}