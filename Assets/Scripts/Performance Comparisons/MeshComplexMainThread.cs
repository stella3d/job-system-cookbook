using UnityEngine;

public class MeshComplexMainThread : MonoBehaviour
{
    [Range(0.05f, 1f)]
    [SerializeField]
    protected float m_Strength = 0.25f;

    Vector3[] m_ModifiedVertices;
    Vector3[] m_ModifiedNormals;

    MeshFilter m_MeshFilter;
    Mesh m_Mesh;

    protected void Start()
    {
        m_MeshFilter = gameObject.GetComponent<MeshFilter>();
        m_Mesh = m_MeshFilter.mesh;
        m_Mesh.MarkDynamic();

        m_ModifiedVertices = m_Mesh.vertices;
        m_ModifiedNormals = m_Mesh.normals;
    }

    public void Update()
    {
        var sinTime = Mathf.Sin(Time.time);
        var cosTime = Mathf.Cos(Time.time);

        var strength = m_Strength / 5f;

        for (int i = 0; i < m_Mesh.vertexCount; i++)
        {
            var vertex = m_ModifiedVertices[i];
            var normal = m_ModifiedNormals[i];

            var perlin = Mathf.PerlinNoise(vertex.z, vertex.y * vertex.x);
            perlin *= strength * 2;
            var noise = normal * perlin;
            var sine = normal * sinTime * strength;

            vertex = vertex - sine + noise;

            m_ModifiedVertices[i] = vertex;

            m_ModifiedNormals[i] += Vector3.one * cosTime * perlin;
        }

        m_Mesh.vertices = m_ModifiedVertices;
        m_Mesh.normals = m_ModifiedNormals;
    }

}
