using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class crossableModel : MonoBehaviour
{
    public GameObject CutPlaneObject1 = null;
    public GameObject CutPlaneObject2 = null;
    public GameObject CutPlaneObject3 = null;


    private List<GameObject> GetCutPlaneObjectsList()
    {
        List<GameObject> cross_sections = new List<GameObject>();
        if (CutPlaneObject1 != null)
            cross_sections.Add(CutPlaneObject1);
        if (CutPlaneObject2 != null)
            cross_sections.Add(CutPlaneObject2);
        if (CutPlaneObject3 != null)
            cross_sections.Add(CutPlaneObject3);
        return cross_sections;
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateBadContoursMaterial();
        if (m_bad_contours_game_object == null)
        {
            m_bad_contours_game_object = new GameObject("bad_contour");
            m_bad_contours_game_object.transform.SetParent(gameObject.transform, false);
        }
    }

    void Reset()
    {
        foreach (GameObject child in m_bad_contours_game_object.transform)
            DestroyImmediate(child.gameObject);
        DestroyImmediate(m_bad_contours_game_object);
        m_bad_contours_game_object = null;
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

    List<CrossSection> GenerateCrossSectionList()
    {
        List<CrossSection> cross_sections = new List<CrossSection>();
        foreach(var cut_plane in GetCutPlaneObjectsList())
        {
            var cross_section = new CrossSection();
            cross_section.m_position = transform.worldToLocalMatrix.MultiplyPoint(cut_plane.transform.position);
            cross_section.m_normal = Vector3.Normalize(transform.worldToLocalMatrix.MultiplyVector(cut_plane.transform.up));
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

        List<CrossSection> cross_sections = GenerateCrossSectionList();

        UpdateBadContoursMaterial();

        foreach (Transform child in m_bad_contours_game_object.transform)
            Destroy(child.gameObject);

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

    void UpdateSurfaceMaterial()
    {
        Vector4[] cut_plane_world_positions = new Vector4[3];
        Vector4[] cut_plane_world_normals = new Vector4[3];

        List<GameObject> cross_plane_objects = GetCutPlaneObjectsList();

        for (int i = 0; i < cross_plane_objects.Count; ++i)
        {
            cut_plane_world_positions[i] = 
                cross_plane_objects[i].transform.position;
            cut_plane_world_normals[i] =
                cross_plane_objects[i].transform.up;
        }

        GetComponent<Renderer>().sharedMaterial.SetVectorArray(
            "_CrossPlanePositions",
            cut_plane_world_positions);
        GetComponent<Renderer>().sharedMaterial.SetVectorArray(
            "_CrossPlaneVisibleNormals",
            cut_plane_world_normals);
        GetComponent<Renderer>().sharedMaterial.SetInt(
            "_CrossPlaneCount",
            cross_plane_objects.Count);
    }

    // Update is called once per frame
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
