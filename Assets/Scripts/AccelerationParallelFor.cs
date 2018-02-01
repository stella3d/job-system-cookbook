using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.Jobs;

public class AccelerationParallelFor : BaseJobObjectExample
{
    public Vector3 m_Acceleration = new Vector3(0.0002f, 0.0001f, 0.0002f);
    public Vector3 m_AccelerationMod = new Vector3(.0001f, 0.0001f, 0.0001f);

    NativeArray<Vector3> m_Velocities;
    TransformAccessArray m_TransformsAccessArray;
    
    PositionUpdateJob m_Job;
    AccelerationJob m_AccelJob;

    JobHandle m_PositionJobHandle;
    JobHandle m_AccelJobHandle;

    protected void Start()
    {
        m_Velocities = new NativeArray<Vector3>(m_ObjectCount, Allocator.Persistent);

        m_Objects = SetupUtils.PlaceRandomCubes(m_ObjectCount, m_ObjectPlacementRadius);

        for (int i = 0; i < m_ObjectCount; i++)
        {
            var obj = m_Objects[i];
            m_Transforms[i] = obj.transform;
            m_Renderers[i] = obj.GetComponent<Renderer>();
        }

        m_TransformsAccessArray = new TransformAccessArray(m_Transforms);
    }

    struct PositionUpdateJob : IJobParallelForTransform
    {
        [ReadOnly]
        public NativeArray<Vector3> velocity;  // the velocities from AccelerationJob

        public float deltaTime;

        public void Execute(int i, TransformAccess transform)
        {
            transform.position += velocity[i] * deltaTime;
        }
    }

    struct AccelerationJob : IJobParallelFor
    {
        public NativeArray<Vector3> velocity;

        public Vector3 acceleration;
        public Vector3 accelerationMod;

        public float deltaTime;

        public void Execute(int i)
        {
            // here, i'm intentionally using the index to affect acceleration (it looks cool),
            // but generating velocities probably wouldn't be tied to index normally.
            velocity[i] += (acceleration + i * accelerationMod) * deltaTime;
        }
    }

    public void Update()
    {
        m_AccelJob = new AccelerationJob()
        {
            deltaTime = Time.deltaTime,
            velocity = m_Velocities,
            acceleration = m_Acceleration,
            accelerationMod = m_AccelerationMod
        };

        m_Job = new PositionUpdateJob()
        {
            deltaTime = Time.deltaTime,
            velocity = m_Velocities,
        };

        m_AccelJobHandle = m_AccelJob.Schedule(m_ObjectCount, 64);
        m_PositionJobHandle = m_Job.Schedule(m_TransformsAccessArray, m_AccelJobHandle);
    }

    public void LateUpdate()
    {
        m_PositionJobHandle.Complete();
    }

    private void OnDestroy()
    {
        m_Velocities.Dispose();
        m_TransformsAccessArray.Dispose();
    }
}