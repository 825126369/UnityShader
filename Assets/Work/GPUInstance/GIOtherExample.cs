using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class InstancedSpriteRenderer : MonoBehaviour
{
    public Material instancedMaterial;
    public Texture2D spriteTexture;
    public int instanceCount = 10000;
    private Mesh mesh;
    private Matrix4x4[] matrices;
    private Color[] colors;

    void Start()
    {
        if (instancedMaterial == null || spriteTexture == null)
            return;

        // 创建单位正方形网格（Sprite 默认是 1x1）
        mesh = new Mesh();
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };
        int[] tris = new int[6] { 0, 1, 2, 2, 1, 3 };
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        mesh.SetVertices(vertices);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);

        // 分配实例数据
        matrices = new Matrix4x4[instanceCount];
        colors = new Color[instanceCount];

        // 随机生成位置和颜色
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 pos = Random.insideUnitCircle * 50f; // 分布在 100x100 区域
            Vector2 scale = Vector2.one * Random.Range(0.5f, 1.5f);

            // 将位置和缩放打包进一个 float4
            matrices[i] = Matrix4x4.identity;
            matrices[i].SetRow(0, new Vector4(scale.x, 0, 0, pos.x));      // x,y = 位置，z,w = 缩放
            matrices[i].SetRow(1, new Vector4(0, scale.y, 0, pos.y));

            colors[i] = Random.ColorHSV(0.5f, 1f, 1f, 1f, 0.8f, 1f, 1f, 1f);
        }
    }

    void Update()
    {
        if (mesh == null || instancedMaterial == null)
            return;

        // 使用 GPU Instancing 绘制所有实例
        Graphics.DrawMeshInstanced(
            mesh,
            0,                    // submesh index
            instancedMaterial,
            matrices,
            instanceCount,
            null,                 // 额外属性
            ShadowCastingMode.On,
            true                  // 接收阴影
        );
    }

    void OnDisable()
    {
        if (mesh != null)
        {
            DestroyImmediate(mesh);
            mesh = null;
        }
    }
}