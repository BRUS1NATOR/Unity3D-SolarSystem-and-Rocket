using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RocketParticles : MonoBehaviour
{
    public ParticleSystem particles;
    public Rocket rocket;

    private float particlesSpeed;
    private void Start()
    {
        particlesSpeed = particles.startSpeed;
    }
    private void FixedUpdate()
    {
        particles.startSpeed = rocket.throttle;
    }
}
