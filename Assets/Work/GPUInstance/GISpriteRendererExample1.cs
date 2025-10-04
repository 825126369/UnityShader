using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class GISpriteRendererExample1 : MonoBehaviour
{
    public Material instancedMaterial;
    public Texture2D spriteTexture;
    public int instanceCount = 10000;
    private Mesh mesh;
    private Matrix4x4[] matrices;
    private Color[] colors;
    
    void OnEnable()
    {
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
        colors = new Color[instanceCount];

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
        }

    }

    void Update()
    {
        Debug.Assert(mesh != null, "mesh == null");
        Debug.Assert(instancedMaterial != null, "instancedMaterial == null");
        if (mesh == null || instancedMaterial == null)
            return;
        
        Graphics.DrawMeshInstanced(
            mesh,
            0,                    // submesh index
            instancedMaterial,
            matrices,
            instanceCount
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