using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class SlicesAndStridesExample : MonoBehaviour
{
    NativeArray<Vector4> m_PointCloud;

    NativeArray<float> m_Distances;

    ConfidenceAverageSliceJob m_ConfidenceProcessingJob;
    ChangePoints m_ChangePointsJob;

    DistanceParallelJob m_DistanceParallelJob;
    AverageHorizontalDistanceJob m_AverageGroundDistanceJob;

    JobHandle m_ParallelDistanceJobHandle;
    JobHandle m_DistanceJobHandle;

    JobHandle m_ConfidenceJobHandle;
    JobHandle m_ChangeJobHandle;

    float m_AverageConfidence;

    protected void Start()
    {
        // this persistent memory setup assumes our vertex count will not expand
        m_PointCloud = new NativeArray<Vector4>(10000, Allocator.Persistent);
        m_Distances = new NativeArray<float>(10000, Allocator.Persistent);

        for (int i = 0; i < m_PointCloud.Length; i++)
        {
            m_PointCloud[i] = RandomVec4();
        }
    }

    Vector4 RandomVec4()
    {
        var vec3 = UnityEngine.Random.insideUnitSphere;
        var w = UnityEngine.Random.value;
        return new Vector4(vec3.x, vec3.y, vec3.z, w);
    }

    struct ChangePoints : IJobParallelFor
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
            if (i % 2 == 0 && sinTimeRandom < 0.3f || sinTimeRandom > 0.9f)
                return;

            var x = random + i * 0.001f - sinTimeRandom;
            var y = cosTimeRandom;
            var z = sinTimeRandom;
            var w = Mathf.Clamp01(0.1f + random + 0.00001f * i);
            points[i] = new Vector4(x, y, z, w);
        }
    }

    struct ConfidenceAverageSliceJob : IJob
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

    struct AverageHorizontalDistanceJob : IJob
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

    struct AverageHeightJob : IJob
    {
        [ReadOnly]
        public NativeSlice<float> y;

        [ReadOnly]
        public float compareValue;

        [WriteOnly]
        public NativeArray<float> average;

        public void Execute()
        {
            float sumY = 0f;
            for (int i = 0; i < y.Length; i++)
                sumY += Mathf.Abs(compareValue - y[i]);

            average[0] = sumY / y.Length;
        }
    }

    public void Update()
    {
        var slice = new NativeSlice<Vector4>(m_PointCloud, 0);

        m_ConfidenceProcessingJob = new ConfidenceAverageSliceJob()
        {
            // sample every other confidence point
            sampleStride = 2,
            // this stride is all the "w" values of the vectors
            confidence = slice.SliceWithStride<float>(12),  
            average = new NativeArray<float>(1, Allocator.TempJob),
        };

        m_DistanceParallelJob = new DistanceParallelJob()
        {
            x = slice.SliceWithStride<float>(0),   // all x values of vectors
            z = slice.SliceWithStride<float>(8),   // all z values of vectors
            compareValue = Vector2.zero,
            distances = m_Distances
        };

        m_AverageGroundDistanceJob = new AverageHorizontalDistanceJob()
        {
            distances = m_Distances,
            average = new NativeArray<float>(3, Allocator.TempJob)
        };

        m_ConfidenceJobHandle = m_ConfidenceProcessingJob.Schedule(m_ChangeJobHandle);

        m_ParallelDistanceJobHandle = m_DistanceParallelJob
            .Schedule(m_Distances.Length, 128, m_ChangeJobHandle);

        m_DistanceJobHandle = m_AverageGroundDistanceJob.Schedule(m_ParallelDistanceJobHandle);

        updateCount++;
    }

    static int updateCount;

    public void LateUpdate()
    {
        m_ConfidenceJobHandle.Complete();
        m_DistanceJobHandle.Complete();

        // this is just to only log info every n updates
        if (updateCount % 120 == 0 || updateCount == 1)
        {
            var distances = m_AverageGroundDistanceJob.average;
            var groundDistance = distances[0];
            var distanceMin = distances[1];
            var distanceMax = distances[2];

            Debug.Log("distance average: " + groundDistance);
            Debug.Log("distance min: " + distanceMin + " , max: " + distanceMax);

            var conf = m_ConfidenceProcessingJob.average[0];
            Debug.Log("confidence average: " + conf);
        }

        // dispose any temporary job allocations
        m_ConfidenceProcessingJob.average.Dispose();
        m_AverageGroundDistanceJob.average.Dispose();


        // change some of the points before next frame to simulate pointcloud
        var changeNoise = Mathf.Clamp01(UnityEngine.Random.value + 0.1f);

        m_ChangePointsJob = new ChangePoints()
        {
            points = m_PointCloud,
            sinTimeRandom = Mathf.Sin(Time.time) * changeNoise,
            cosTimeRandom = Mathf.Cos(Time.time) * changeNoise,
            random = changeNoise,
            frameCount = Time.frameCount
        };

        m_ChangeJobHandle = m_ChangePointsJob.Schedule(m_PointCloud.Length, 64);
    }

    private void OnDestroy()
    {
        m_PointCloud.Dispose();
    }

    private void OnPlayModeExit()
    {
        m_PointCloud.Dispose();
    }
}
