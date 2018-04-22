using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class MeshComplexParallel : MonoBehaviour
{
    [Range(0.05f, 1f)]
    [SerializeField]
    protected float m_Strength = 0.1f;

    NativeArray<Vector3> m_Vertices;
    NativeArray<Vector3> m_Normals;

    Vector3[] m_ModifiedVertices;
    Vector3[] m_ModifiedNormals;

    MeshModJob m_MeshModJob;
    JobHandle m_JobHandle;

    Mesh m_Mesh;

    protected void Start()
    {
        m_Mesh = gameObject.GetComponent<MeshFilter>().mesh;
        m_Mesh.MarkDynamic();

        // this persistent memory setup assumes our vertex count will not expand
        m_Vertices = new NativeArray<Vector3>(m_Mesh.vertices, Allocator.Persistent);
        m_Normals = new NativeArray<Vector3>(m_Mesh.normals, Allocator.Persistent);

        m_ModifiedVertices = new Vector3[m_Vertices.Length];
        m_ModifiedNormals = new Vector3[m_Vertices.Length];
    }

    struct MeshModJob : IJobParallelFor
    {
        public NativeArray<Vector3> vertices;
        public NativeArray<Vector3> normals;

        public float sinTime;
        public float cosTime;

        public float strength;

        public void Execute(int i)
        {
            var vertex = vertices[i];

            var perlin = Mathf.PerlinNoise(vertex.z, vertex.y * vertex.x);
            perlin *= strength * 2;
            var noise = normals[i] * perlin;
            var sine = normals[i] * sinTime * strength;

            vertex = vertex - sine + noise;

            vertices[i] = vertex;

            normals[i] += Vector3.one * cosTime * perlin;
        }
    }

    public void Update()
    {
        m_MeshModJob = new MeshModJob()
        {
            vertices = m_Vertices,
            normals = m_Normals,
            sinTime = Mathf.Sin(Time.time),
            cosTime = Mathf.Cos(Time.time),
            strength = m_Strength / 5f  // map .05-1 range to smaller real strength
        };

        m_JobHandle = m_MeshModJob.Schedule(m_Vertices.Length, 64);
    }

    public void LateUpdate()
    {
        m_JobHandle.Complete();

        // copy our results to managed arrays so we can assign them
        m_MeshModJob.vertices.CopyTo(m_ModifiedVertices);
        m_MeshModJob.normals.CopyTo(m_ModifiedNormals);

        m_Mesh.vertices = m_ModifiedVertices;
        m_Mesh.normals = m_ModifiedNormals;
    }

    private void OnDestroy()
    {
        // make sure to Dispose() any NativeArrays when we're done
        m_Vertices.Dispose();
        m_Normals.Dispose();
    }
}