using System.Collections;
using System.Collections.Generic;
using UnityEngine;


class BadEdge
{
    public BadEdge(Vector3 v1, Vector3 v2)
    {
        m_points = new System.Tuple<Vector3, Vector3>(v1, v2);
    }
    public override int GetHashCode()
    {
        int hash = m_points.Item1.GetHashCode() + m_points.Item2.GetHashCode();
        return hash;
    }
    public override bool Equals(object obj)
    {
        return Equals(obj as BadEdge);
    }

    public bool Equals(BadEdge be2)
    {
        bool equals =
            m_points.Item1 == be2.m_points.Item1 && m_points.Item2 == be2.m_points.Item2 ||
            m_points.Item2 == be2.m_points.Item1 && m_points.Item1 == be2.m_points.Item2;
        return equals;
    }

    public Vector3 StartPoint
    {
        get { return m_points.Item1; }
    }
    public Vector3 EndPoint
    {
        get { return m_points.Item2; }
    }

    private System.Tuple<Vector3, Vector3> m_points;
}

public class BadEdgesProcessor
{
    public static LinkedList<BadContour> FindMeshBadContours(Mesh mesh)
    {
        LinkedList<BadContour> bad_contours = new LinkedList<BadContour>();

        for (int submesh_indx = 0; submesh_indx < mesh.subMeshCount; ++submesh_indx)
        {
            int[] indices = mesh.GetIndices(submesh_indx);
            int triangle_count = indices.Length / 3;

            List<Vector3> vertices = new List<Vector3>();
            mesh.GetVertices(vertices);

            // find bad edges;
            Dictionary<BadEdge, int> edge_counts = new Dictionary<BadEdge, int>();
            Debug.Assert(indices.Length % 3 == 0, "Indices should make triangles :)");
            for(int triangle_indx = 0; triangle_indx < triangle_count; ++triangle_indx)
            {
                for(int i = 0, j = 1; i < 3; ++i, ++j)
                {
                    int indx_start = triangle_indx * 3;
                    int edge_vertex_indx_1 = indices[indx_start + i];
                    int edge_vertex_indx_2 = indices[indx_start + j % 3];
                    BadEdge tested_bad_edge = new BadEdge(
                        vertices[edge_vertex_indx_1], 
                        vertices[edge_vertex_indx_2]);
                    if (edge_counts.ContainsKey(tested_bad_edge))
                        ++edge_counts[tested_bad_edge];
                    else
                        edge_counts[tested_bad_edge] = 1;
                }
            }

            List<BadEdge> bad_edges = new List<BadEdge>();
            foreach(var edge in edge_counts)
            {
                if (edge.Value != 2)
                {
                    bad_edges.Add(edge.Key);
                }
            }
            Debug.Log("Bad edge count: " + bad_edges.Count);
            if (bad_edges.Count == 0)
                continue;

            float eps = 0.001f;
            var cur_edge = bad_edges[0];
            var cur_point = cur_edge.StartPoint;
            BadContour cur_contour = new BadContour();
            cur_contour.AddNextPoint(cur_point);
            while(true)
            {
                cur_point = cur_point == cur_edge.StartPoint ?
                            cur_edge.EndPoint : cur_edge.StartPoint;
                cur_contour.AddNextPoint(
                    cur_point);

                bad_edges.Remove(cur_edge);

                cur_edge = bad_edges.Find(
                (obj) =>
                    {
                        bool found = obj.StartPoint == cur_point
                            || obj.EndPoint == cur_point;
                        return found;
                    });
                if (cur_edge != null)
                {
                    continue;
                }
                bad_contours.AddLast(cur_contour);
                if (bad_edges.Count == 0)
                    break;
                cur_edge = bad_edges[0];
                cur_contour = new BadContour();
                cur_point = cur_edge.StartPoint;
                cur_contour.AddNextPoint(cur_point);
            }
        }
        return bad_contours;
    }
}
