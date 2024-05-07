using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitViewDestroyableParticles : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float duration;

    private void Start()
    {
        StartCoroutine(PlayAndDestroy());
    }

    private IEnumerator PlayAndDestroy()
    {
        particles.Play();
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
