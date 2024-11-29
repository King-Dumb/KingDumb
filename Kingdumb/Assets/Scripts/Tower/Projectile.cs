using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject impactParticle;
    private GameObject projectileParticle;
    private GameObject muzzleParticle;

    public float projectileDamage;
    public float projectileSpeed;

    private Vector3 targetPosition;
    private Vector3 targetDirection;
    private int towerType;

    private bool isKnockBackOn = false;
    private bool isMagicAttack = false;
    private bool isSlowDebuffOn = false;
    private bool isSingleAttack = true;
    private bool isAttackFinished = false;

    private int towerPhotonViewId;
    public LayerMask collisionLayer;

    public void Initialize(float towerProjectileDamage, float towerProjectileSpeed, Vector3 targetPosition, int towerType, bool isHighestLevel, int photonViewID)
    {
        Debug.Log("Initialize 실행 시작");
        //Debug.Log($"현재 타워가 최종 강화 형태인지 확인 : {isHighestLevel}");
        projectileDamage = towerProjectileDamage;
        projectileSpeed = towerProjectileSpeed;

        //targetPosition = targetMonster.transform.position;
        targetPosition.y += 0.5f; // 바닥을 타겟으로 설정하지 않게 하기 위해 조금 띄움 -> TODO: 플라잉 몬스터와 보스 몬스터 확인 필요

        this.towerType = towerType;
        towerPhotonViewId = photonViewID;

        // 변수 초기화
        isKnockBackOn = false;
        isMagicAttack = false;
        isSlowDebuffOn = false;
        isSingleAttack = true;
        isAttackFinished = false;

        // isHighestLevel = true; // 디버깅용

        if (isHighestLevel)
        {
            switch (towerType)
            {
                // towerType => 0: 전사, 1: 궁수, 2: 마법사, 3: 힐러
                case 0:
                    isKnockBackOn = true;
                    isSingleAttack = false;
                    break;
                case 1:
                    isMagicAttack = true;
                    break;
                case 2:
                    isSlowDebuffOn = true;
                    isSingleAttack = false;
                    break;
            }
        }

        // 타겟 방향으로 이동할 벡터 계산
        targetDirection = (targetPosition - transform.position).normalized;

        //Debug.Log($"타겟 몬스터의 이름 : {targetMonster.name}");

        // OnEnable이 Initialize보다 먼저 실행되는 관계로 towerType 초기화 문제가 있어서 여기로 옮겨옴
        projectileParticle = GameManager.Instance.Instantiate("towerProjectileParticle" + towerType, transform.position, Quaternion.identity);
        projectileParticle.transform.parent = transform;
        Invoke("DestroyProjectileDamage", 2f);

        muzzleParticle = GameManager.Instance.Instantiate("towerMuzzleParticle" + towerType, transform.position, Quaternion.identity);
        GameManager.Instance.Destroy(muzzleParticle, 1.5f);

        ParticleSystem[] particleSystems = projectileParticle.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            // Debug.Log("파티클 꼬리 자르는 중");
            // ps.Clear();
            ps.Simulate(0, true, true);  // 0초 동안 시뮬레이션하여 초기화 (리셋)
            ps.Play();
        }
    }

    private void OnEnable()
    {
        // Debug.Log("Enable 실행 시작");
        // projectileParticle = GameManager.Instance.Instantiate("towerProjectileParticle" + towerType, transform.position, Quaternion.identity);
        // projectileParticle.transform.parent = transform;
        // Invoke("DestroyProjectileDamage", 2f);

        // muzzleParticle = GameManager.Instance.Instantiate("towerMuzzleParticle" + towerType, transform.position, Quaternion.identity);
        // GameManager.Instance.Destroy(muzzleParticle, 1.5f);

        // ParticleSystem[] particleSystems = projectileParticle.GetComponentsInChildren<ParticleSystem>();
        // foreach (ParticleSystem ps in particleSystems)
        // {
        //     // Debug.Log("파티클 꼬리 자르는 중");
        //     // ps.Clear();
        //     ps.Simulate(0, true, true);  // 0초 동안 시뮬레이션하여 초기화 (리셋)
        //     ps.Play();
        // }
    }

    private void OnDisable()
    {
        // ParticleSystem[] particleSystems = projectileParticle.GetComponentsInChildren<ParticleSystem>();
        // foreach (ParticleSystem ps in particleSystems)
        // {
        //     Debug.Log("파티클 꼬리 자르는 중");
        //     ps.Clear();
        //     // ps.Play();
        // }


    }

    private void Update()
    {
        // 이동
        transform.Translate(targetDirection * projectileSpeed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isAttackFinished)
        {
            return;
        }

        impactParticle = GameManager.Instance.Instantiate("towerImpactParticle" + towerType, transform.position, Quaternion.identity); // 이거 한번만 터지는지 확인
        GameManager.Instance.Destroy(impactParticle, 1f);

        // if (projectileParticle != null)
        // {
        //     projectileParticle.transform.parent = null; // 여기서 왜 null 오류 터지는지?? 확인이.. 필요...
        //     GameManager.Instance.Destroy(projectileParticle);
        // }

        //Debug.Log($"타워 투사체가 충돌한 대상 : {other.gameObject.name}, 타워 투사체의 위치 : {transform.position}");

        if (other.CompareTag("Monster"))
        {
            IMonster targetMonster = other.GetComponent<IMonster>();

            if (targetMonster != null)
            {
                if (isSingleAttack)
                {
                    //Debug.Log($"일반 공격 대상 : {other.gameObject.name}");
                    // if (isKnockBackOn)
                    // {
                    //     Vector3 dir = targetDirection.normalized * 0.00001f;
                    //     targetMonster.Knockback(dir); // 임시로 아무 인수나 넣어 둠
                    // }

                    // if (isSlowDebuffOn)
                    // {
                    //     targetMonster.DebuffSlow(0.7f, 3f); // float slowRate, float duration
                    // }

                    targetMonster.OnDamage(projectileDamage, isMagicAttack, transform.position, towerPhotonViewId);
                }
                else
                {
                    CreateOverlapSphere(2f);
                }


            }
        }

        // projectileParticle.transform.parent = null;
        // GameManager.Instance.Destroy(gameObject);
        // Destroy(gameObject);
    }

    private void CreateOverlapSphere(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, collisionLayer);

        foreach (Collider hitCollider in hitColliders)
        {
            IMonster targetMonster = hitCollider.gameObject.GetComponent<IMonster>();

            if (targetMonster != null)
            {
                //Debug.Log($"범위 공격 대상 : {hitCollider.gameObject.name}");
                if (isKnockBackOn)
                {
                    Vector3 dir = targetDirection.normalized * 2f;
                    targetMonster.Knockback(dir);
                }

                if (isSlowDebuffOn)
                {
                    targetMonster.DebuffSlow(0.7f, 3f); // float slowRate, float duration
                }

                targetMonster.OnDamage(projectileDamage, isMagicAttack, transform.position, towerPhotonViewId);
            }
        }
    }

    private void DestroyProjectileDamage()
    {
        projectileParticle.transform.parent = null;
        GameManager.Instance.Destroy(projectileParticle);
    }

    private void OnDrawGizmos()
    {
        Vector3 GizmoPosition = transform.position;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(GizmoPosition, 2f);
    }

}
