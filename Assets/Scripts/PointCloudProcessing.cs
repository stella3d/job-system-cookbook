using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Random = UnityEngine.Random;

public class PointCloudProcessing : MonoBehaviour
{
    public int pointCount = 10000;
    const float k_PointCloudRadius = 5f;

    NativeArray<Vector3> m_PointCloud;
    NativeArray<Vector3> m_NormalizedPointCloud;

    NativeArray<float> m_SquareMagnitudes;

    GeneratePointCloudJob m_GenPointCloudJob;
    CalculateDistancesJob m_DistancesJob;
    NormalizationJob m_NormalizeJob;

    JobHandle m_GeneratePointsJobHandle;
    JobHandle m_DistancesJobHandle;
    JobHandle m_NormalizeJobHandle;


    protected void Start()
    {
        m_PointCloud = new NativeArray<Vector3>(pointCount, Allocator.Persistent);
        m_NormalizedPointCloud = new NativeArray<Vector3>(pointCount, Allocator.Persistent);
        m_SquareMagnitudes = new NativeArray<float>(pointCount, Allocator.Persistent);
    }

    // in most cases we would of course not be generating a point cloud, but
    // taking one in.  this simulates a large point cloud being constantly updated
    struct GeneratePointCloudJob : IJobParallelFor
    {
        public NativeArray<Vector3> pointCloud;

        [ReadOnly]
        public Vector3 center;

        [ReadOnly]
        public float radius;

        public void Execute(int i)
        {
            pointCloud[i] = center + Vector3.one * (i / 100) * radius;
        }
    }

    // turns that point cloud into normalized versions from 0-1
    struct NormalizationJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> points;

        public NativeArray<Vector3> normalizedPoints;

        public void Execute(int i)
        {
            normalizedPoints[i] = points[i].normalized;
        }
    }

    // calculate the square magnitudes of the non-normalized points
    struct CalculateDistancesJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> points;

        public NativeArray<float> squareMagnitudes;

        public void Execute(int i)
        {
            squareMagnitudes[i] = points[i].sqrMagnitude;
        }
    }

    public void LateUpdate()
    {
        m_DistancesJobHandle.Complete();
        m_NormalizeJobHandle.Complete();
    }

    public void Update()
    {
        m_GenPointCloudJob = new GeneratePointCloudJob()
        {
            pointCloud = m_PointCloud,
            center = Random.insideUnitSphere * 2,
            radius = k_PointCloudRadius
        };

        m_DistancesJob = new CalculateDistancesJob()
        {
            points = m_PointCloud,
            squareMagnitudes = m_SquareMagnitudes
        };

        m_NormalizeJob = new NormalizationJob()
        {
            points = m_PointCloud,
            normalizedPoints = m_NormalizedPointCloud,
        };

        m_GeneratePointsJobHandle = m_GenPointCloudJob.Schedule(m_PointCloud.Length, 64);
        m_DistancesJobHandle = m_DistancesJob.Schedule(m_PointCloud.Length, 64, m_GeneratePointsJobHandle);
        m_NormalizeJobHandle = m_NormalizeJob.Schedule(m_PointCloud.Length, 64, m_GeneratePointsJobHandle);
    }

    private void OnDestroy()
    {
        m_NormalizedPointCloud.Dispose();
        m_PointCloud.Dispose();
        m_SquareMagnitudes.Dispose();
    }
}