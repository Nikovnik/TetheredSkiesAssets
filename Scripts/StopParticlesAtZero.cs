using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopParticlesAtZero : MonoBehaviour
{
    ParticleSystem ps;
    ParticleSystem.MainModule part;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        part = ps.main;
    }

    private void OnTransformParentChanged()
    {
        part.loop = false;
    }
}
