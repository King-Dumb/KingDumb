using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackHolder : EffectHolder
{
    [SerializeField] public GameObject energyAttackPrefab;
    [SerializeField] public GameObject darknessAttackPrefab;
    [SerializeField] public GameObject frostAttackPrefab;

    private float _attackDamage = 20;
    private float _splashDamage = 8;
    private float _projectileSpeed = 12;
    private bool _isTrackable = false;

    public override void InitializeEffects()
    {
        effectPrefabs = new Dictionary<EffectType, GameObject>
        {
            { EffectType.Energy, energyAttackPrefab },
            { EffectType.Darkness, darknessAttackPrefab },
            { EffectType.Frost, frostAttackPrefab }
        };
    }

    public override void Attack(EffectType effectType, Vector3 position, Vector3 direction, int ownerPhotonViewID)
    {
        // if (effectPrefabs[effectType] == null)
        // {
        //     Debug.Log("프리팹이 정상적으로 저장되지 않음");
        // }
        // else
        // {
        //     //Debug.Log(effectPrefabs[effectType].name);
        // }
        GameObject attackProjectile = GameManager.Instance.Instantiate(effectPrefabs[effectType].name, position + direction, Quaternion.identity);

        attackProjectile.GetComponent<FireBall>().SettingBall(direction, ownerPhotonViewID, 
            _attackDamage, _splashDamage, _projectileSpeed, _isTrackable);
    }

    public void SetAttackDamage(float damage)
    {
        _attackDamage = damage;
    }

    public float SplashDamageUp(float damage)
    {
        _splashDamage += damage;
        return _splashDamage;
    }

    public float ProjectileSpeedUp(float speed)
    {
        _projectileSpeed += speed;
        return _projectileSpeed;    
    }

    public bool TrackingActivate()
    {
        _isTrackable = true;
        return _isTrackable;
    }
}
