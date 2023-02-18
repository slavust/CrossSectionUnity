using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CrossSection : MonoBehaviour
{
    private const float DEFAULT_SECTION_SIZE = 10.0f;

    public List<CrossSectionInfo> GenerateCrossPlanesList()
    {
        List<CrossSectionInfo> cross_sections = new List<CrossSectionInfo>();

        CrossSectionInfo p1 = new CrossSectionInfo();
        p1.m_normal = transform.up;
        p1.m_position = transform.position;

        CrossSectionInfo p2 = new CrossSectionInfo();
        p2.m_normal = transform.forward;
        p2.m_position = transform.position;

        CrossSectionInfo p3 = new CrossSectionInfo();
        p3.m_normal = transform.right;
        p3.m_position = transform.position;

        cross_sections.Add(p1);
        cross_sections.Add(p2);
        cross_sections.Add(p3); ;

        return cross_sections;
    }

    void OnDrawGizmos()
    {
        Vector3[] ax1 = { new Vector3(0, 0, 0), new Vector3(1, 0, 0) };
        Vector3[] ax2 = { new Vector3(0, 0, 0), new Vector3(0, 1, 0) };
        Vector3[] ax3 = { new Vector3(0, 0, 0), new Vector3(0, 0, 1) };

        Vector3[] uax1 = { new Vector3(0, 0, 1), new Vector3(1, 0, 1) };
        Vector3[] uax2 = { new Vector3(0, 0, 1), new Vector3(0, 1, 1) };

        Vector3[] sax1 = { new Vector3(1, 0, 0), new Vector3(1, 0, 1) };
        Vector3[] sax2 = { new Vector3(0, 1, 0), new Vector3(0, 1, 1) };

        Vector3[] dax1 = { new Vector3(1, 0, 0), new Vector3(1, 1, 0) };
        Vector3[] dax2 = { new Vector3(0, 1, 0), new Vector3(1, 1, 0) };

        List<Vector3[]> lines = new List<Vector3[]>();
        lines.Add(ax1);
        lines.Add(ax2);
        lines.Add(ax3);

        lines.Add(uax1);
        lines.Add(uax2);

        lines.Add(sax1);
        lines.Add(sax2);

        lines.Add(dax1);
        lines.Add(dax2);

        foreach(var line in lines)
        {
            Gizmos.color = new Color(1, 1, 0);
            Gizmos.DrawLine(
                transform.TransformPoint(line[0] * DEFAULT_SECTION_SIZE),
                transform.TransformPoint(line[1] * DEFAULT_SECTION_SIZE)
               );
        }
    }

    Mesh CreatePlaneMesh(float size, Vector3 width_dir, Vector3 height_dir)
    {
        const int num_segments = 5;

        width_dir = width_dir * size / num_segments;
        height_dir = height_dir * size / num_segments;

        Vector2 u_dir = new Vector2(1.0f / num_segments, 0);
        Vector2 v_dir = new Vector2(0, 1.0f / num_segments);

        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        for(int i = 0; i < num_segments+1; ++i)
        {
            for(int j = 0; j < num_segments+1; ++j)
            {
                Vector3 vertex_pos = height_dir * i + width_dir * j;
                vertices.Add(vertex_pos);

                Vector2 uv = v_dir * i + u_dir * j;
                uvs.Add(uv);
            }
        }

        List<int> indices = new List<int>();
        for(int i = 0; i < num_segments; ++i)
        {
            for(int j = 0; j < num_segments; ++j)
            {
                int v1 = i * (num_segments + 1) + j;
                int v2 = v1 + 1;
                int v3 = v1 + (num_segments + 1);
                int v4 = v3 + 1;

                indices.Add(v1);
                indices.Add(v2);
                indices.Add(v3);

                indices.Add(v2);
                indices.Add(v4);
                indices.Add(v3);
            }
        }

        Mesh plane_mesh = new Mesh();
        plane_mesh.vertices = vertices.ToArray();
        plane_mesh.triangles = indices.ToArray();
        plane_mesh.SetUVs(0, uvs.ToArray());

        return plane_mesh;
    }

    void CreateCrossSectionsRenderable()
    {
        CombineInstance[] combine = new CombineInstance[3];
        combine[0].mesh = CreatePlaneMesh(DEFAULT_SECTION_SIZE, new Vector3(1, 0, 0), new Vector3(0, 1, 0));
        combine[1].mesh = CreatePlaneMesh(DEFAULT_SECTION_SIZE, new Vector3(0, 1, 0), new Vector3(0, 0, 1));
        combine[2].mesh = CreatePlaneMesh(DEFAULT_SECTION_SIZE, new Vector3(0, 0, 1), new Vector3(1, 0, 0));
        MeshFilter filter = gameObject.GetComponent<MeshFilter>(); ;
        if (filter == null)
            filter = gameObject.AddComponent<MeshFilter>();
        filter.sharedMesh = new Mesh();
        filter.sharedMesh.CombineMeshes(combine, true, false);
        filter.sharedMesh.RecalculateNormals();
        MeshRenderer renderer = gameObject.GetComponent<MeshRenderer>();
        if (renderer == null)
            renderer = gameObject.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = m_cross_section_material;
        transform.gameObject.SetActive(true);
    }

    public void SetRenderQueue(int render_queue)
    {
        GetComponent<MeshRenderer>().sharedMaterial.renderQueue = render_queue;
    }

    public void Start()
    {
        m_cross_section_material = new Material(Shader.Find("CrossSections/_CrossSection"));
        m_cross_section_material.SetTexture("_SectionTex", HatchingTexture == null ? Texture2D.whiteTexture : HatchingTexture); ;
        m_cross_section_material.SetTextureScale("_SectionTex", TextureScale);
        m_cross_section_material.SetColor("_SectionColor", HatchingColor);
        Reset();
        CreateCrossSectionsRenderable();
    }
    public void Reset()
    {
    }

    void OnEnable()
    {
        Start();
    }

    void OnDisable()
    {
        Reset();
    }

    void Update()
    {
    }

    void OnValidate()
    {
        if (m_cross_section_material == null)
            return;
        m_cross_section_material.SetTexture("_SectionTex", HatchingTexture == null ? Texture2D.whiteTexture : HatchingTexture);
        m_cross_section_material.SetTextureScale("_SectionTex", TextureScale);
        m_cross_section_material.SetColor("_SectionColor", HatchingColor);
    }



    public Vector2 TextureScale = new Vector2(1.0f, 1.0f);
    public Texture HatchingTexture = null;
    public Color HatchingColor = new Color(1, 1, 1, 1);

    private Material m_cross_section_material = null;
}