using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

public class AccelerationParallelFor : BaseJobObjectExample
{
    public Vector3 m_Acceleration = new Vector3(0.0002f, 0.0001f, 0.0002f);
    public Vector3 m_AccelerationMod = new Vector3(.0001f, 0.0001f, 0.0001f);

    NativeArray<Vector3> m_Positions;
    NativeArray<Vector3> m_Velocities;

    PositionUpdateJob m_Job;
    AccelerationJob m_AccelJob;
    JobHandle m_JobHandle;
    JobHandle m_AccelJobHandle;

    protected void Start()
    {
        m_Positions = new NativeArray<Vector3>(m_ObjectCount, Allocator.Persistent);
        m_Velocities = new NativeArray<Vector3>(m_ObjectCount, Allocator.Persistent);

        for (int i = 0; i < m_ObjectCount; i++)
        {
            m_Positions[i] = m_Transforms[i].position;
        }
    }

    struct PositionUpdateJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<Vector3> velocity;

        public NativeArray<Vector3> position;

        public float deltaTime;

        // The code actually running on the job
        public void Execute(int i)
        {
            position[i] += velocity[i] * deltaTime;
        }
    }

    struct AccelerationJob : IJobParallelFor
    {
        public NativeArray<Vector3> velocity;

        public Vector3 acceleration;
        public Vector3 accelerationMod;

        public float deltaTime;

        // The code actually running on the job
        public void Execute(int i)
        {
            velocity[i] += (acceleration + i * accelerationMod) * deltaTime;
        }
    }

    public void LateUpdate()
    {
        m_JobHandle.Complete();

        for (int i = 0; i < m_ObjectCount; i++)
        {
            // only update the transform if something is looking at it
            // just an optimization so the performance depends more on the jobs
            if(m_Renderers[i].isVisible)
                m_Transforms[i].position = m_Job.position[i];
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
            position = m_Positions,
            velocity = m_Velocities,
        };

        m_AccelJobHandle = m_AccelJob.Schedule(m_Positions.Length, 64);
        m_JobHandle = m_Job.Schedule(m_Positions.Length, 64, m_AccelJobHandle);
    }

    private void OnDestroy()
    {
        m_Positions.Dispose();
        m_Velocities.Dispose();
    }
}