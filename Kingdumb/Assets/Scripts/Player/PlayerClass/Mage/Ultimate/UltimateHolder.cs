using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateHolder : EffectHolder
{
    [SerializeField] public GameObject energyUltimatePrefab1;
    [SerializeField] public GameObject energyUltimatePrefab2;
    [SerializeField] public GameObject darknessChargePrefab;
    [SerializeField] public GameObject darknessUltimatePrefab1;
    [SerializeField] public GameObject darknessUltimatePrefab2;
    [SerializeField] public GameObject frostUltimatePrefab1;
    [SerializeField] public GameObject frostUltimatePrefab2;

    private int _ownerPhotonViewID;

    private float _ultimateDamage = 10;

    public override void InitializeEffects()
    {
        effectPrefabs = new Dictionary<EffectType, GameObject>
        {
            { EffectType.Energy, energyUltimatePrefab1 },
            { EffectType.Darkness, darknessUltimatePrefab1 },
            { EffectType.Frost, frostUltimatePrefab1 }
        };
    }

    public override void Attack(EffectType effectType, Vector3 position, Vector3 direction, int sourceViewID)
    {
        _ownerPhotonViewID = sourceViewID;
        switch (effectType)
        {
            case EffectType.Energy:
                EnergyUltimate(position, direction);
                break;
            case EffectType.Darkness:
                DarknessUltimate(position, direction);
                break;
            case EffectType.Frost:
                FrostUltimate(position, direction);
                break;

        }
    }

    private void EnergyUltimate(Vector3 position, Vector3 direction)
    {
        Vector3 rightDirection = gameObject.transform.right.normalized;
        Vector3 upPosition = Vector3.up;
        GameObject megaBlast1 = GameManager.Instance.Instantiate(energyUltimatePrefab1.name,
            position + direction * 5 + upPosition * 10 + rightDirection * (-3), Quaternion.identity);
        GameObject megaBlast2 = GameManager.Instance.Instantiate(energyUltimatePrefab2.name,
            position + direction * 5 + upPosition * 10 + rightDirection * 3, Quaternion.identity);
        StartCoroutine(EnergyUltimateDamage(position + direction * 5 + upPosition * 10, direction));

        GameManager.Instance.Destroy(megaBlast1, 3.0f);
        GameManager.Instance.Destroy(megaBlast2, 3.0f);
    }

    private IEnumerator EnergyUltimateDamage(Vector3 position, Vector3 direction)
    {
        float energyRadius = 30f; // 공격 범위 반경
        yield return new WaitForSeconds(2.0f);
        Collider[] hitColliders = Physics.OverlapSphere(position, energyRadius);
        //Debug.Log("범위에 잡힌 객체 수: " + hitColliders.Length);
        
                    foreach (Collider collider in hitColliders)
            {
                // 특정 태그를 가진 객체만 처리
                if (collider.CompareTag("Monster"))
                {
                    IDamageable target = collider.GetComponent<IDamageable>();
                    //Debug.Log("공격할target: " + target);
                    if (target != null)
                    {
                        Vector3 effectPosition = collider.transform.position;
                        target.OnDamage(_ultimateDamage * 5, true, effectPosition, _ownerPhotonViewID);
                    }
                }
            }
        


    }

    private void DarknessUltimate(Vector3 position, Vector3 direction)
    {
        StartCoroutine(DarknessBlast(position, direction));
    }

    public IEnumerator DarknessBlast(Vector3 position, Vector3 direction)
    {
        Vector3 upPosition = Vector3.up;
        GameObject chargeEffect = GameManager.Instance.Instantiate(darknessChargePrefab.name,
            position + direction * 7 + upPosition * 2,  Quaternion.identity);
        yield return new WaitForSeconds(3.0f);
        GameManager.Instance.Destroy(chargeEffect);
        GameObject darknessUltimate1 = GameManager.Instance.Instantiate(darknessUltimatePrefab1.name,
            position + direction * 7 + upPosition * 2, Quaternion.identity);
        yield return new WaitForSeconds(0.01f);
        GameObject darknessUltimate2 = GameManager.Instance.Instantiate(darknessUltimatePrefab2.name,
            position + direction * 7 + upPosition * 2, Quaternion.identity);

        float darknessRadius = 10f;
        Vector3 explosionCenter = position + direction * 7 + upPosition * 2;
        Collider[] hitColliders = Physics.OverlapSphere(explosionCenter, darknessRadius);
        //Debug.Log("범위에 잡힌 객체 수: " + hitColliders.Length);
        try
        {
            foreach (Collider collider in hitColliders)
            {
                // 특정 태그를 가진 객체만 처리
                if (collider.CompareTag("Monster"))
                {
                    IDamageable target = collider.GetComponent<IDamageable>();
                    Debug.Log("공격할target: " + target);
                    if (target != null)
                    {
                        // 중심과의 거리 계산
                        float distanceToCenter = Vector3.Distance(explosionCenter, collider.transform.position);
                        float distanceFactor = Mathf.Clamp01(1 - (distanceToCenter / darknessRadius));

                        // 데미지 계산: 중심에 가까울수록 100% 데미지, 멀수록 감소
                        int baseDamage = (int)(_ultimateDamage * 15);
                        int damage = Mathf.RoundToInt(baseDamage * distanceFactor);

                        // 데미지 적용
                        Vector3 effectPosition = collider.transform.position;
                        target.OnDamage(damage, true, effectPosition, _ownerPhotonViewID);

                        Debug.Log($"Target {collider.name} received {damage} damage (Distance factor: {distanceFactor})");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private void FrostUltimate(Vector3 position, Vector3 direction)
    {
        StartCoroutine(FrostBlast(position, direction));
    }

    public IEnumerator FrostBlast(Vector3 position, Vector3 direction)
    {
        Vector3 upPosition = Vector3.up;
        GameObject frostUltimate = GameManager.Instance.Instantiate(frostUltimatePrefab1.name,
            position + direction * 5 + upPosition * 10, Quaternion.Euler(-270, 0, 0));

        float freezeRadius = 10f; // 얼리는 범위 반경
        float duration = 10.0f; // 효과 지속 시간
        float interval = 1.0f; // 감지 간격 (1초)
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            // Default Layer에 있는 모든 오브젝트 탐지
            Collider[] hitColliders = Physics.OverlapSphere(position + direction * 6, freezeRadius);

            foreach (Collider collider in hitColliders)
            {
                // 특정 태그를 가진 객체만 처리
                if (collider.CompareTag("Monster"))
                {
                    IMonster monster = collider.GetComponent<IMonster>();
                    IDamageable target = collider.GetComponent<IDamageable>();
                    if (monster != null)
                    {
                        monster.DebuffSlow(1f, 0.7f); // 적을 1초 동안 얼리기
                        Vector3 effectPosition = collider.transform.position;
                        target.OnDamage(_ultimateDamage, true, effectPosition, _ownerPhotonViewID);
                        GameObject hitEffect = GameManager.Instance.Instantiate(frostUltimatePrefab2.name, effectPosition, Quaternion.identity);
                        GameManager.Instance.Destroy(hitEffect, 1.0f);
                    }
                }
            }

            yield return new WaitForSeconds(1.0f);
            elapsedTime += interval;
        }
        Destroy(frostUltimate);
    }

    public float UltimateDamageUp()
    {
        _ultimateDamage *= 2;
        return _ultimateDamage;
    }
}
