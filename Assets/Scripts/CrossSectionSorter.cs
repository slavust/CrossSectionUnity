using System.Collections.Generic;
using System.Collections;
using UnityEngine;

[ExecuteInEditMode]
public class CrossSectionSorter : Singleton<CrossSectionSorter>
{
    public Camera camera = null;
    private List<CrossSection> m_frame_cross_sections = new List<CrossSection>();

    public void NorifyFrameCrossSection(CrossSection cross_section)
    {
        m_frame_cross_sections.Add(cross_section);
    }


    private class CrossSectionComparer : IComparer<CrossSection>
    {
        private Camera m_camera = null;
        public CrossSectionComparer(Camera camera)
        {
            m_camera = camera;
        }
        int IComparer<CrossSection>.Compare(CrossSection x, CrossSection y)
        {
            float x_distance = Vector3.Distance(x.transform.position, m_camera.transform.position);
            float y_distance = Vector3.Distance(y.transform.position, m_camera.transform.position);

            if (x_distance > y_distance)
                return -1;
            if (x_distance < y_distance)
                return 1;
            return 0;
        }
    }

    Dictionary<CrossSection, List<CrossableModel>> m_crossable_models_by_sections = new Dictionary<CrossSection, List<CrossableModel>>();
    public void NotifyFrameCrossableModel(CrossableModel crossable_model)
    {
        var cross_section = crossable_model.CrossSectionObject;
        if (cross_section == null)
            return;
        if (m_crossable_models_by_sections.ContainsKey(cross_section))
            m_crossable_models_by_sections[cross_section].Add(crossable_model);
        else
        {
            var model_list = new List<CrossableModel>();
            model_list.Add(crossable_model);
            m_crossable_models_by_sections[cross_section] = model_list;
        }
    }

    void SortModelsByCrossSections( )
    {
        int i = 0;
        foreach(var section in m_crossable_models_by_sections.Keys)
        {
            var render_queue = (int)UnityEngine.Rendering.RenderQueue.Geometry + i;
            foreach (var crossable_model in m_crossable_models_by_sections[section])
                crossable_model.SetRenderQueue(render_queue); // is ok (tested)
            section.SetRenderQueue(render_queue+1);
            i += 2;
        }
        m_crossable_models_by_sections.Clear();
    }

    void LateUpdate()
    {
        SortModelsByCrossSections();
    }
}