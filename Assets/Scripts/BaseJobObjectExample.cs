using System;
using UnityEngine;

public class BaseJobObjectExample : MonoBehaviour
{
    [SerializeField]
    protected int m_ObjectCount = 10000;

    [SerializeField]
    protected float m_ObjectPlacementRadius = 100f;

    protected GameObject[] m_Objects;
    protected Transform[] m_Transforms;
    protected Renderer[] m_Renderers;

    protected void Awake()
    {
        m_Objects = new GameObject[m_ObjectCount];
        m_Transforms = new Transform[m_ObjectCount];
        m_Renderers = new Renderer[m_ObjectCount];
    }
}