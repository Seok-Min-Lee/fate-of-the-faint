using System;
using UnityEngine;
public enum EntityParticleKey
{
    None,
    Heal,
    Block,
    Power,
    Buff,
    Debuff,
}
public class EntityParticle : MonoBehaviour
{
    [SerializeField] private EntityParticleKey key;
    public EntityParticleKey Key => key;

    private ParticleSystem particle
    {
        get
        {
            if (_particle == null)
            {
                _particle = GetComponent<ParticleSystem>();
            }

            return _particle;
        }
    }
    private ParticleSystem _particle;
    public void Play()
    {
        particle.Play();
    }
}