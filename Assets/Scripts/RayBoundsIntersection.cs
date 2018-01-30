using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class RayBoundsIntersection : BaseJobObjectExample
{
    NativeArray<Bounds> m_NativeBounds;
    NativeArray<int> m_RayIntersectionResults;

    RayIntersectionJob m_RayIntersectionJob;
    RayIntersectionListJob m_RayIntersectionListJob;

    JobHandle m_RayIntersectionJobHandle;
    JobHandle m_RayIntersectionListJobHandle;

    protected virtual void Start()
    {
        m_NativeBounds = new NativeArray<Bounds>(m_ObjectCount, Allocator.Persistent);
        m_RayIntersectionResults = new NativeArray<int>(m_ObjectCount, Allocator.Persistent);

        m_Objects = SetupUtils.PlaceRandomCubes(m_ObjectCount, m_ObjectPlacementRadius);

        for (int i = 0; i < m_ObjectCount; i++)
        {
            m_Renderers[i] = m_Objects[i].GetComponent<Renderer>();
            m_NativeBounds[i] = m_Renderers[i].bounds;
        }
    }

    // assemble an array representing which input Bounds in the parallel array was intersected
    struct RayIntersectionJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Bounds> boundsArray;

        // we can't use bools in NativeArray<T>, since the type must be blittable,
        // so i'm representing boolean results as 0 or 1
        [WriteOnly]
        public NativeArray<int> results;

        public Ray ray;

        public void Execute(int i)
        {
            if (boundsArray[i].IntersectRay(ray))
                results[i] = 1;
            else
                results[i] = 0;
        }
    }

    // use a non-parallel job to assemble a friendlier results array
    struct RayIntersectionListJob : IJob
    {
        [ReadOnly]
        public NativeArray<Bounds> boundsArray;

        [ReadOnly]
        public NativeArray<int> boundsIntersected;

        [WriteOnly]
        public NativeArray<Bounds> results;

        public void Execute()
        {
            int resultIndex = 0;
            for (int i = 0; i < boundsArray.Length; i++)
            {
                if (resultIndex == results.Length)
                    break;

                if (boundsIntersected[i] == 1)
                {
                    results[resultIndex] = boundsArray[i];
                    resultIndex++;
                }
            }
        }
    }

    public void Update()
    {
        // generate a new Ray to test against every frame
        var point = UnityEngine.Random.insideUnitSphere;
        var testRay = new Ray(Vector3.zero - point, Vector3.right + Vector3.up + point);

        m_RayIntersectionJob = new RayIntersectionJob()
        {
            ray = testRay,
            results = m_RayIntersectionResults,
            boundsArray = m_NativeBounds
        };

        // instead of keeping a results array between frames and clearing it, here we
        // use the TempJob allocator for an array we'll dispose quickly (within 4 frames)
        var results = new NativeArray<Bounds>(new Bounds[10], Allocator.TempJob);

        m_RayIntersectionListJob = new RayIntersectionListJob()
        {
            boundsIntersected = m_RayIntersectionResults,
            boundsArray = m_NativeBounds,
            results = results
        };

        m_RayIntersectionJobHandle = m_RayIntersectionJob.Schedule(m_NativeBounds.Length, 64);
        m_RayIntersectionListJobHandle = m_RayIntersectionListJob.Schedule(m_RayIntersectionJobHandle);
    }

    public void LateUpdate()
    {
        m_RayIntersectionListJobHandle.Complete();

        var results = m_RayIntersectionListJob.results;
        var resultCount = GetResultCount(results);

        if (resultCount > 0)
            Debug.Log(resultCount + " total intersections, first result: " + results[0]);

        // make sure to dispose any temp allocations made for this job
        m_RayIntersectionListJob.results.Dispose();
    }

    public int GetResultCount(NativeArray<Bounds> bounds)
    {
        var zeroBounds = new Bounds();
        for (int i = 0; i < bounds.Length; i++)
        {
            if (bounds[i] == zeroBounds)
            {
                return i;
            }
        }

        return 0;
    }

    private void OnDestroy()
    {
        m_NativeBounds.Dispose();
        m_RayIntersectionResults.Dispose();
    }
}