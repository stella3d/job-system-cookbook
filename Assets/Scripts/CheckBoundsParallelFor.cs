using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

// this is optimized for checking against a static set of Bounds,
// but can easily be adapted to handle when they change
public class CheckBoundsParallelFor : BaseJobObjectExample
{
    [SerializeField]
    protected float m_ObjectPlacementRadius = 100f;

    [SerializeField]
    protected Bounds m_LastResult;

    NativeArray<Vector3> m_Positions;
    NativeArray<Bounds> m_NativeBounds;

    BoundsContainsPointJob m_Job;
    BoundsIntersectionJob m_IntersectionJob;

    JobHandle m_JobHandle;
    JobHandle m_IntersectionJobHandle;

    protected virtual void Start()
    {
        m_Positions = new NativeArray<Vector3>(m_ObjectCount, Allocator.Persistent);
        m_NativeBounds = new NativeArray<Bounds>(m_ObjectCount, Allocator.Persistent);

        m_Objects = new GameObject[m_ObjectCount];
        m_Objects = SetupUtils.PlaceRandomCubes(m_ObjectCount, m_ObjectPlacementRadius);

        m_Transforms = new Transform[m_ObjectCount];
        m_Renderers = new Renderer[m_ObjectCount];

        for (int i = 0; i < m_ObjectCount; i++)
        {
            m_Transforms[i] = m_Objects[i].transform;
            m_Renderers[i] = m_Objects[i].GetComponent<Renderer>();
            m_NativeBounds[i] = m_Renderers[i].bounds;
        }
    }

    struct BoundsContainsPointJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Bounds> boundsArray;

        public Vector3 point;

        public Bounds resultBounds;

        // The code actually running on the job
        public void Execute(int i)
        {
            Bounds testAgainst = boundsArray[i];
            if (testAgainst.Contains(point))
            {
                Debug.Log("point " + point + " is in Bounds: " + testAgainst);
                resultBounds = testAgainst; 
            }
        }
    }

    struct BoundsIntersectionJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Bounds> boundsArray;

        public Bounds boundsToCheck;

        public Bounds resultBounds;

        // The code actually running on the job
        public void Execute(int i)
        {
            Bounds testAgainst = boundsArray[i];
            if (boundsToCheck.Intersects(testAgainst))
            {
                Debug.Log(boundsToCheck + " intersects with: " + testAgainst);
                resultBounds = testAgainst;
            }
        }
    }

    // in real code you'd want to schedule the job early instead of this
    public void LateUpdate()
    {
        m_JobHandle.Complete();
        m_IntersectionJobHandle.Complete();

        m_LastResult = m_IntersectionJob.resultBounds;
    }

    public void Update()
    {
        var point = Random.insideUnitSphere * 100f;

        // check if a point intersects any of the cube's bounds
        m_Job = new BoundsContainsPointJob()
        {
            point = point,
            boundsArray = m_NativeBounds,
        };

        // check if a bounding box outside that point intersects any cubes
        m_IntersectionJob = new BoundsIntersectionJob()
        {
            boundsToCheck = new Bounds(point, Vector3.one),
            boundsArray = m_NativeBounds
        };

        m_JobHandle = m_Job.Schedule(m_Positions.Length, 64);
        m_IntersectionJobHandle = m_IntersectionJob.Schedule(m_NativeBounds.Length, 64);
    }

    private void OnDestroy()
    {
        m_Positions.Dispose();
        m_NativeBounds.Dispose();
    }
}