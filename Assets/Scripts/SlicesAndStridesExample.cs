using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class SlicesAndStridesExample : MonoBehaviour
{
    [SerializeField]
    protected int m_PointCount = 10000;

    NativeArray<Vector4> m_PointCloud;
    NativeArray<float> m_Distances;

    NativeArray<float> m_DistanceResults;
    NativeArray<float> m_ConfidenceResults;

    UpdatePointCloudJob m_UpdatePointCloudJob;
    ConfidenceProcessingJob m_ConfidenceProcessingJob;
    DistanceParallelJob m_DistanceParallelJob;
    AverageGroundDistanceJob m_AverageGroundDistanceJob;

    JobHandle m_ParallelDistanceJobHandle;
    JobHandle m_DistanceJobHandle;
    JobHandle m_ConfidenceJobHandle;
    JobHandle m_PointCloudUpdateHandle;

    static int updateCount;

    // simulate a large, constantly changing point cloud
    struct UpdatePointCloudJob : IJobParallelFor
    {
        [WriteOnly]
        public NativeArray<Vector4> points;

        [ReadOnly]
        public float sinTimeRandom;

        [ReadOnly]
        public float cosTimeRandom;

        [ReadOnly]
        public float random;

        [ReadOnly]
        public int frameCount;

        public void Execute(int i)
        {
            var x = random + i * 0.001f - sinTimeRandom;
            var y = cosTimeRandom;
            var z = sinTimeRandom;
            var w = Mathf.Clamp01(0.2f + random + 0.00001f * i);
            points[i] = new Vector4(x, y, z, w);
        }
    }

    // calculate average confidence value for the current pointcloud
    struct ConfidenceProcessingJob : IJob
    {
        [ReadOnly]
        public NativeSlice<float> confidence;

        [WriteOnly]
        public NativeArray<float> average;

        [ReadOnly]
        public int sampleStride;

        public void Execute()
        {
            float total = 0f;
            int end = confidence.Length - sampleStride + 1;

            for (int i = 0; i < end; i += sampleStride)
                total += confidence[i];

            average[0] = sampleStride * total / confidence.Length;
        }
    }

    // calculate the horizontal distance of every point in parallel
    struct DistanceParallelJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeSlice<float> x;

        [ReadOnly]
        public NativeSlice<float> z;

        [ReadOnly]
        public Vector2 compareValue;

        [WriteOnly]
        public NativeArray<float> distances;

        public void Execute(int i)
        {
            var currentX = Mathf.Abs(compareValue.x - x[i]);
            var currentZ = Mathf.Abs(compareValue.y - z[i]);

            distances[i] = Mathf.Sqrt(currentX + currentZ);
        }
    }

    // once we've calculated distances, find mix, max, & average,
    // using a non-parallel job.  You could also do this in "buckets"
    // using another parallel job if your dataset was very large -
    // find the local min / max / sum for each bucket, then do this
    struct AverageGroundDistanceJob : IJob
    {
        [ReadOnly]
        public NativeArray<float> distances;

        [WriteOnly]
        public NativeArray<float> average;

        public void Execute()
        {
            float sum = 0f;
            float lowest = Single.MaxValue;
            float highest = Single.MinValue;

            for (int i = 0; i < distances.Length; i++)
            {
                var dist = distances[i];
                if (dist < lowest)
                    lowest = dist;
                if (dist > highest)
                    highest = dist;

                sum += dist;
            }

            average[0] = sum / distances.Length;
            average[1] = lowest;
            average[2] = highest;
        }
    }

    protected void Start()
    {
        // this persistent memory setup assumes our point count will not expand
        m_PointCloud = new NativeArray<Vector4>(m_PointCount, Allocator.Persistent);
        m_Distances = new NativeArray<float>(m_PointCount, Allocator.Persistent);

        m_DistanceResults = new NativeArray<float>(3, Allocator.Persistent);
        m_ConfidenceResults = new NativeArray<float>(1, Allocator.Persistent);

        for (int i = 0; i < m_PointCloud.Length; i++)
            m_PointCloud[i] = RandomVec4();
    }

    public void Update()
    {
        var slice = new NativeSlice<Vector4>(m_PointCloud, 0);

        m_ConfidenceProcessingJob = new ConfidenceProcessingJob()
        {
            // 2 = sample every other confidence point.  this value exists
            // to tune accuracy vs speed here, is not part of the job system
            sampleStride = 2,

            // this stride is all the "w" values of the vectors
            // 12 is the byte offset of the "w" field 
            confidence = slice.SliceWithStride<float>(12),
            average = m_ConfidenceResults
        };

        m_DistanceParallelJob = new DistanceParallelJob()
        {
            // all x values of vectors - x has 0 byte field offset
            x = slice.SliceWithStride<float>(0),
            
            // all z values of vectors - z has 8 byte field offset
            z = slice.SliceWithStride<float>(8),

            compareValue = Vector2.zero,
            distances = m_Distances
        };

        m_AverageGroundDistanceJob = new AverageGroundDistanceJob()
        {
            distances = m_Distances,
            average = m_DistanceResults
        };

        m_ConfidenceJobHandle = m_ConfidenceProcessingJob.Schedule(m_PointCloudUpdateHandle);

        m_ParallelDistanceJobHandle = m_DistanceParallelJob
            .Schedule(m_Distances.Length, 128, m_PointCloudUpdateHandle);

        m_DistanceJobHandle = m_AverageGroundDistanceJob.Schedule(m_ParallelDistanceJobHandle);
    }

    public void LateUpdate()
    {
        // make sure both job chains we started in Update complete
        m_ConfidenceJobHandle.Complete();
        m_DistanceJobHandle.Complete();

        PrintDebugInfo();

        // change our point cloud ahead of the next frame
        ScheduleNextPointUpdateJob();
    }

    void ScheduleNextPointUpdateJob()
    {
        // change some of the points before next frame to simulate pointcloud
        var turbulence = Mathf.Clamp01(UnityEngine.Random.value + 0.333f) - .333f;

        m_UpdatePointCloudJob = new UpdatePointCloudJob()
        {
            points = m_PointCloud,
            sinTimeRandom = Mathf.Sin(Time.time) * turbulence,
            cosTimeRandom = Mathf.Cos(Time.time) * turbulence,
            random = turbulence,
            frameCount = Time.frameCount
        };

        m_PointCloudUpdateHandle = m_UpdatePointCloudJob.Schedule(m_PointCloud.Length, 64);
    }

    void PrintDebugInfo()
    {
        updateCount++;
        // this is just to only log info every n updates
        if (updateCount % 150 == 0 || updateCount == 1)
        {
            var distances = m_AverageGroundDistanceJob.average;
            var confidence = m_ConfidenceProcessingJob.average[0];

            Debug.Log("distance average: " + distances[0]);
            Debug.Log("distance min: " + distances[1] + " , max: " + distances[2]);
            Debug.Log("confidence average: " + confidence);
        }
    }

    Vector4 RandomVec4()
    {
        var vec3 = UnityEngine.Random.insideUnitSphere;
        var w = UnityEngine.Random.value;
        return new Vector4(vec3.x, vec3.y, vec3.z, w);
    }

    private void OnDestroy()
    {
        // make sure we don't have running jobs
        if (!m_PointCloudUpdateHandle.IsCompleted)
            m_PointCloudUpdateHandle.Complete();

        // dispose our permanent allocations
        m_PointCloud.Dispose();
        m_Distances.Dispose();
        m_DistanceResults.Dispose();
        m_ConfidenceResults.Dispose();
    }

}
