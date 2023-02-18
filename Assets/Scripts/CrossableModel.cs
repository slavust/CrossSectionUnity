using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CrossableModel : MonoBehaviour
{
    public CrossSection CrossSectionObject = null;

    void Start()
    {
        Reset();
        if (m_bad_contours_game_object == null)
        {
            m_bad_contours_game_object = new GameObject("_bad_contours");
            m_bad_contours_game_object.transform.SetParent(gameObject.transform, false);
        }
    }

    void Reset()
    {
        //ResetSurfaceMaterial();
        if(m_bad_contours_game_object == null)
        {
            var child_transform = transform.Find("_bad_contours"); ;
            if (child_transform != null)
                m_bad_contours_game_object = child_transform.gameObject;
        }
        if (m_bad_contours_game_object != null)
        { 
            DestroyImmediate(m_bad_contours_game_object);
            m_bad_contours_game_object = null;
        }
        m_bad_contours = null;
        m_object_mesh = null;
        m_bad_contour_stub_material = null;
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
            if (object_mesh != null && object_mesh != m_object_mesh)
                recalculate_initial_bad_contours = true;
            m_object_mesh = object_mesh;
        }
        if (recalculate_initial_bad_contours)
        {
            m_bad_contours = BadEdgesProcessor.FindMeshBadContours(m_object_mesh);

            List<CrossSectionInfo> cross_sections = CrossSectionObject.GenerateCrossPlanesList();
            cross_sections = TransformCrossSectionsToObjectSpace(cross_sections);

            if(m_bad_contour_stub_material == null)
                m_bad_contour_stub_material = new Material(Shader.Find("CrossSections/_BadContourStub"));

            List<CombineInstance> combine = new List<CombineInstance>();
            foreach (BadContour bad_Contour in m_bad_contours)
            {
                BadContour subcontour = bad_Contour.GenerateFrameBadContour(cross_sections);
                if (subcontour == null)
                    continue;
                var combine_inst = new CombineInstance();
                combine_inst.mesh = subcontour.GenerateStubMesh();
                combine.Add(combine_inst);
            }

            MeshFilter mesh_filter = m_bad_contours_game_object.GetComponent<MeshFilter>();
            if (mesh_filter == null)
                mesh_filter = m_bad_contours_game_object.AddComponent<MeshFilter>();
            mesh_filter.sharedMesh = new Mesh();
            mesh_filter.sharedMesh.CombineMeshes(combine.ToArray(), true, false);

            MeshRenderer renderer = m_bad_contours_game_object.GetComponent<MeshRenderer>();
            if (renderer == null)
                renderer = m_bad_contours_game_object.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = m_bad_contour_stub_material;
            m_bad_contours_game_object.SetActive(true);
        }
    }
    /*
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

        foreach (var material in GetComponent<MeshRenderer>().materials)
        {
            material.SetVectorArray(
                "_CrossPlanePositions",
                cut_plane_world_positions);
            material.SetVectorArray(
                "_CrossPlaneVisibleNormals",
                cut_plane_world_normals);
        }
    }*/

    void UpdateMaterials()
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

        foreach(var material in GetComponent<Renderer>().sharedMaterials)
        {
            material.SetVectorArray(
                "_CrossPlanePositions",
                cut_plane_world_positions);
            material.SetVectorArray(
                "_CrossPlaneVisibleNormals",
                cut_plane_world_normals);
        }

        m_bad_contour_stub_material.SetVectorArray(
            "_CrossPlanePositions",
            cut_plane_world_positions);
        m_bad_contour_stub_material.SetVectorArray(
            "_CrossPlaneVisibleNormals",
            cut_plane_world_normals);
    }

    void Update()
    {
        if (CrossSectionObject == null)
            return;
        UpdateBadContours();
        UpdateMaterials();
        CrossSectionSorter.Instance.NotifyFrameCrossableModel(this);
    }


    public void SetRenderQueue(int render_queue)
    {
        foreach(var material in GetComponent<MeshRenderer>().sharedMaterials)
            material.renderQueue = render_queue;
        m_bad_contour_stub_material.renderQueue = render_queue;
    }

    private Mesh m_object_mesh = null;
    private LinkedList<BadContour> m_bad_contours = null;
    private GameObject m_bad_contours_game_object = null;
    private Material m_bad_contour_stub_material = null;
}
