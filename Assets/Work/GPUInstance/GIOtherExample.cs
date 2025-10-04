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

        // ������λ����������Sprite Ĭ���� 1x1��
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

        // ����ʵ������
        matrices = new Matrix4x4[instanceCount];
        colors = new Color[instanceCount];

        // �������λ�ú���ɫ
        for (int i = 0; i < instanceCount; i++)
        {
            Vector3 pos = Random.insideUnitCircle * 50f; // �ֲ��� 100x100 ����
            Vector2 scale = Vector2.one * Random.Range(0.5f, 1.5f);

            // ��λ�ú����Ŵ����һ�� float4
            matrices[i] = Matrix4x4.identity;
            matrices[i].SetRow(0, new Vector4(scale.x, 0, 0, pos.x));      // x,y = λ�ã�z,w = ����
            matrices[i].SetRow(1, new Vector4(0, scale.y, 0, pos.y));

            colors[i] = Random.ColorHSV(0.5f, 1f, 1f, 1f, 0.8f, 1f, 1f, 1f);
        }
    }

    void Update()
    {
        if (mesh == null || instancedMaterial == null)
            return;

        // ʹ�� GPU Instancing ��������ʵ��
        Graphics.DrawMeshInstanced(
            mesh,
            0,                    // submesh index
            instancedMaterial,
            matrices,
            instanceCount,
            null,                 // ��������
            ShadowCastingMode.On,
            true                  // ������Ӱ
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