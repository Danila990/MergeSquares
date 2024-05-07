using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Vector2 = System.Numerics.Vector2;

public class ParticlesController : MonoBehaviour
{
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private ParticleSystem particleSystemRoot;
    [SerializeField] private ParticleSystem particleSystemAppear;
    private ParticleSystem.Particle [] particles;
    private float allStartTime = 0;

    public void RunAppear()
    {
        particleSystemAppear.gameObject.SetActive(true);
    }
    
    public IEnumerator FlyToTarget(float duration, Transform target, Color startColor, Color endColor, Action callback)
    {
        allStartTime = Time.time;
        particleSystemRoot.gameObject.SetActive(true);

        var main = particleSystem.main;
        // main.startColor = new Color(startColor.r, startColor.g, startColor.b, startColor.a);
        
        while (true)
        {
            particles = new ParticleSystem.Particle [particleSystem.particleCount];
            particleSystem.GetParticles(particles);

            if (allStartTime + (duration / 2) > Time.time)
            {
                yield return null;
            }
            else
            {
                for (int i = 0; i < particles.GetUpperBound(0); i++)
                {
                    var t = Time.deltaTime / (duration / 2);
                    float ForceToAdd = (particles[i].startLifetime - particles[i].remainingLifetime) *
                                       (10 * Vector3.Distance(target.position, particles[i].position));
                    particles[i].velocity = (target.position - particles[i].position).normalized * ForceToAdd;
                    particles[i].position = Vector3.Lerp(particles[i].position, target.position,
                        t);
                    
                    // particles[i].startColor = Color32.Lerp(particles[i].startColor, endColor, t);
                }
                particleSystem.SetParticles(particles, particles.Length);
            }

            if (allStartTime + duration < Time.time)
            {
                break;
            }

            yield return null;
        }
        particleSystemRoot.gameObject.SetActive(false);

        callback.Invoke();
    }
}
