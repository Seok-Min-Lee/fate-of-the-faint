using System.Collections.Generic;
using UnityEngine;

public class EntityParticleManager : MonoBehaviour
{
    private Dictionary<EntityParticleKey, EntityParticle> dictionary = new Dictionary<EntityParticleKey, EntityParticle>();
    private void Start()
    {
        foreach (EntityParticle window in GetComponentsInChildren<EntityParticle>())
        {
            if (!dictionary.ContainsKey(window.Key))
            {
                dictionary.Add(window.Key, window);
            }
        }
    }
    public void Play(EntityParticleKey key)
    {
        if (!dictionary.ContainsKey(key))
        {
            return;
        }

        dictionary[key].Play();
    }
}
