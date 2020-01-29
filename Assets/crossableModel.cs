using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class crossableModel : MonoBehaviour
{
    public CrossSection CrossSectionObject = null;

    // Start is called before the first frame update
    void Start()
    {
        Reset();
        UpdateBadContoursMaterial();
        if (m_bad_contours_game_object == null)
        {
            m_bad_contours_game_object = new GameObject("_bad_contours");
            m_bad_contours_game_object.transform.SetParent(gameObject.transform, false);
        }
    }

    void Reset()
    {
        ResetSurfaceMaterial();
        if(m_bad_contours_game_object == null)
        {
            var child_transform = transform.Find("_bad_contours");
            if (child_transform != null)
                m_bad_contours_game_object = child_transform.gameObject;
        }
        if (m_bad_contours_game_object != null)
        {
            foreach (GameObject child in m_bad_contours_game_object.transform)
                DestroyImmediate(child.gameObject);
            DestroyImmediate(m_bad_contours_game_object);
            m_bad_contours_game_object = null;
        }
    }

    void OnEnable()
    {
        Reset();
        Start();
    }

    void OnDisable()
    {
        Reset();
    }

    private int m_obj_counter = 0;
    void DebugDumpBadContour(BadContour bad_contour)
    {
        List<string> obj_lines = new List<string>();
        foreach(var point in bad_contour.PointsArray)
        {
            string line = "v " + point.x + " " + point.y + " " + point.z;
            obj_lines.Add(line);
        }
        string indices_line = "l";
        for(int indx = 0; indx <= bad_contour.PointCount; ++indx)
        {
            indices_line += " " + ((indx % bad_contour.PointCount) + 1);
        }
        obj_lines.Add(indices_line);
        System.IO.File.WriteAllLines(
            "/home/slavust/obj_tests/" + ++m_obj_counter + ".obj", 
            obj_lines);
    }

    List<CrossSectionInfo> TransformCrossSectionsToObjectSpace(List<CrossSectionInfo> cross_sections_world)
    {
        List<CrossSectionInfo> cross_sections = new List<CrossSectionInfo>();
        foreach(var cross_section_world in cross_sections_world)
        {
            var cross_section = new CrossSectionInfo();
            cross_section.m_position = transform.worldToLocalMatrix.MultiplyPoint(cross_section_world.m_position);
            cross_section.m_normal = Vector3.Normalize(transform.worldToLocalMatrix.MultiplyVector(cross_section_world.m_normal));
            cross_sections.Add(cross_section);
        }
        return cross_sections;
    }

    void UpdateBadContours()
    {
        bool recalculate_initial_bad_contours = false;
        if (GetComponent<MeshFilter>())
        {
            Mesh object_mesh = GetComponent<MeshFilter>().sharedMesh;
            if (object_mesh != m_object_mesh)
                recalculate_initial_bad_contours = true;
            m_object_mesh = object_mesh;
        }
        if (m_object_mesh && (recalculate_initial_bad_contours || m_bad_contours == null))
        {
            m_bad_contours = BadEdgesProcessor.FindMeshBadContours(m_object_mesh);
            Debug.Log("Bad contour count:" + m_bad_contours.Count);
        }

        List<CrossSectionInfo> cross_sections = CrossSectionObject.GenerateCrossPlanesList();
        cross_sections = TransformCrossSectionsToObjectSpace(cross_sections);

        UpdateBadContoursMaterial();

        foreach (Transform child in m_bad_contours_game_object.transform)
            DestroyImmediate(child.gameObject);

        foreach (BadContour bad_contour in m_bad_contours)
        {
            GameObject obj = new GameObject("bad_contour");
            obj.transform.SetParent(m_bad_contours_game_object.transform, false);
            BadContour subcontour = bad_contour.GenerateFrameBadContour(cross_sections);
            if (subcontour == null)
                continue;
            obj.AddComponent<MeshFilter>().sharedMesh =
                subcontour.GenerateStubMesh();
            var renderer = obj.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = m_bad_contour_stub_material;
        }
    }

    void UpdateBadContoursMaterial()
    {
        if (m_bad_contour_stub_material != null)
            return;
        m_bad_contour_stub_material = Resources.Load<Material>("Materials/bad_contour_stub");
        Debug.Assert(m_bad_contour_stub_material != null);
    }

    void ResetSurfaceMaterial()
    {
        Vector4[] cut_plane_world_positions = new Vector4[3];
        Vector4[] cut_plane_world_normals = new Vector4[3];

        for (int i = 0; i < 3; ++i)
        {
            cut_plane_world_positions[i] = new Vector4(0, 0, 0, 1);
            cut_plane_world_normals[i] =
                new Vector4(0, 0, Mathf.Pow(-1, i), 1);
        }

        foreach (var material in GetComponent<Renderer>().materials)
        {
            material.SetVectorArray(
                "_CrossPlanePositions",
                cut_plane_world_positions);
            material.SetVectorArray(
                "_CrossPlaneVisibleNormals",
                cut_plane_world_normals);
        }
    }

    void UpdateSurfaceMaterial()
    {
        Vector4[] cut_plane_world_positions = new Vector4[3];
        Vector4[] cut_plane_world_normals = new Vector4[3];

        List<CrossSectionInfo> cross_plane_objects = CrossSectionObject.GenerateCrossPlanesList();

        for (int i = 0; i < cross_plane_objects.Count; ++i)
        {
            cut_plane_world_positions[i] = 
                cross_plane_objects[i].m_position;
            cut_plane_world_normals[i] =
                cross_plane_objects[i].m_normal;
        }

        foreach(var material in GetComponent<Renderer>().materials)
        {
            material.SetVectorArray(
                "_CrossPlanePositions",
                cut_plane_world_positions);
            material.SetVectorArray(
                "_CrossPlaneVisibleNormals",
                cut_plane_world_normals);
        }
    }

    void Update()
    {
        UpdateBadContours();
        UpdateSurfaceMaterial();
    }

    private Mesh m_object_mesh = null;
    private LinkedList<BadContour> m_bad_contours = null;
    private GameObject m_bad_contours_game_object = null;
    private Material m_bad_contour_stub_material = null;
}
