using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmashDamageField : MonsterDamageField
{
    public ParticleSystem explosionEffect;

    public ParticleSystem fieldEffect;

    void OnEnable()
    {
        explosionEffect.Play();
        fieldEffect.Play();
    }

    void OnDisble()
    {
        explosionEffect.Stop();
        fieldEffect.Stop();
    }
}
