using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EffectType { Energy, Darkness, Frost }
public abstract class EffectHolder : MonoBehaviour
{
    protected Dictionary<EffectType, GameObject> effectPrefabs;

    // 효과 설정
    public abstract void InitializeEffects();

    // 효과 발동
    public virtual void ActivateEffect(EffectType effectType, Vector3 position, Vector3 direction)
    {
    }

    public virtual void ActivateEffect(EffectType effectType, Vector3 position, Vector3 direction, int photonViewID)
    {
    }

    public virtual void Attack(EffectType effectType, Vector3 position, Vector3 direction, int photonViewID)
    {
    }
}

