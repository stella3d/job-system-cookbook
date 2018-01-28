using System;
using UnityEngine;

public class BaseJobObjectExample : MonoBehaviour
{
    public Func<int, GameObject[]> placeObjects;

    [SerializeField]
    protected int m_ObjectCount = 10000;

    protected GameObject[] m_Objects;
    protected Transform[] m_Transforms;
    protected Renderer[] m_Renderers;

    protected virtual void Awake()
    {
        if (placeObjects == null)
            placeObjects = SetupUtils.PlaceRandomCubes;

        m_Objects = new GameObject[m_ObjectCount];
        m_Objects = placeObjects(m_ObjectCount);

        m_Transforms = new Transform[m_ObjectCount];
        m_Renderers = new Renderer[m_ObjectCount];

        for (int i = 0; i < m_ObjectCount; i++)
        {
            m_Transforms[i] = m_Objects[i].transform;
            m_Renderers[i] = m_Objects[i].GetComponent<Renderer>();
        }
    }
}