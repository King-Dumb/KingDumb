using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;

public class Warrior : CharacterInfo, IPlayerClass
{

    public GameObject defaultSwordObject;
    public GameObject throwingSwordObject;

    private BoxCollider warriorSwordCollider;

    private DefaultSword defaultSword;
    private ThrowingSword throwingSword;

    public GameObject newUltimateEffect1; // SwordVolleyGold
    public GameObject newUltimateEffect2; // MegaExplosionYellow
    public GameObject newUltimateEffect3; // SwordTornado

    public GameObject ultimateEffectInstance1;
    public GameObject ultimateEffectInstance2;
    public GameObject ultimateEffectInstance3;

    public LayerMask collisionLayer;

    private bool isFlyingMonsterInstantDeath = false;
    private int maxAttackableMonster = 5;

    private float skillDamage;
    private float ultimateDamage;

    public Warrior()
    {
        // _classType = "Warrior";
        // _maxHp = 120;
        // _hp = 120;
        // _attackDamage = 10f;
        // _defencePower = 5f;
        // _attackDuration = 1f;
        // _skillDuration = 3f;
        // _moveSpeed = 5f;
        // _runningSpeed = _moveSpeed * 2f;

        _classType = "Warrior";
        _maxHp = _hp = 120; // 기준 최대 체력, 채력
        _baseMoveSpeed = _moveSpeed = 4.5f; // 기준 이동 속도, 이동 속도
        _runningSpeed = 1.5f * _moveSpeed; // 달리기 속도
        _baseAttackDamage = _attackDamage = 30; // 기준 공격력, 공격력
        _defencePower = 5; // 방어력
        _attackDuration = 0.3f; // 공격속도(초)
        _skillDuration = 3f; // 스킬 쿨타임(초)
        _ultimateDuration = 10f; // 궁 쿨타임(초)
        _level = 1; // 레벨
        _skillPoint = 1; // 스킬 포인트
        _gold = 0; // 골드
    }

    private void Start()
    {
        InitializeComponents();

        skillDamage = 45f;
        ultimateDamage = 90f;

        // Default Attack의 Sword 참조
        defaultSword = defaultSwordObject.GetComponent<DefaultSword>();
        warriorSwordCollider = defaultSwordObject.GetComponent<BoxCollider>();
        defaultSword.playerPhotonViewId = _ownerPhotonViewID;
        defaultSword.defaultAttackDamage = _attackDamage;

        // Skill Attack의 Sword 참조
        throwingSword = throwingSwordObject.GetComponent<ThrowingSword>();
        throwingSword.playerPhotonViewId = _ownerPhotonViewID;
        throwingSword.throwingDamage = skillDamage;

        if (photonView.IsMine)
        {
            LoadSkill();
        }

        // 스킬 불러와지고 다시 설정
        _hp = _maxHp;
    }

    // public int tempNum = 1;

    private void Update()
    {
        // //Debug.Log(_defencePower);

        // // 디버깅용
        // if (Input.GetKeyDown(KeyCode.F12))
        // {
        //     Die();
        // }

        // 스킬트리 디버깅용
        // if (Input.GetKeyDown(KeyCode.U))
        // {
        //     //Debug.Log($"{tempNum}번 노드가 실행됩니다.");
        //     GetComponent<WarriorSkillTreeManager>().activateNode(tempNum++);
        // }
    }

    #region 전사 공격
    public void Attack()
    {
        photonView.RPC("AttackRpc", RpcTarget.All);
    }

    [PunRPC]
    public void AttackRpc()
    {
        // defaultSwordObject.transform.GetChild(0).gameObject.SetActive(true);
        warriorSwordCollider.enabled = true;
    }

    public void FinishDefaultAttack()
    {
        photonView.RPC("FinishDefaultAttackRpc", RpcTarget.All);
    }

    [PunRPC]
    public void FinishDefaultAttackRpc()
    {
        // defaultSwordObject.transform.GetChild(0).gameObject.SetActive(false);
        warriorSwordCollider.enabled = false;
    }

    public void Skill()
    {

    }

    public void ThrowSword()
    {
        throwingSwordObject.gameObject.SetActive(true);
    }

    public void Ultimate()
    {
        photonView.RPC("UltimateRpc", RpcTarget.All);
    }

    [PunRPC]
    public void UltimateRpc()
    {
        StartCoroutine(UltimateAttackCoroutine());
    }

    private IEnumerator UltimateAttackCoroutine()
    {
        Vector3 ultimateTargetPosition = transform.position;
        ultimateTargetPosition += transform.forward * 5f;

        ultimateEffectInstance1 = GameManager.Instance.Instantiate(newUltimateEffect1.name, ultimateTargetPosition, Quaternion.identity);
        ultimateEffectInstance2 = GameManager.Instance.Instantiate(newUltimateEffect2.name, ultimateTargetPosition, Quaternion.identity);

        yield return new WaitForSeconds(1.8f);

        ultimateTargetPosition.y += 0.8f;
        ultimateEffectInstance3 = GameManager.Instance.Instantiate(newUltimateEffect3.name, ultimateTargetPosition, Quaternion.Euler(-90f, 0f, 0f));

        Collider[] hitColliders = Physics.OverlapSphere(ultimateTargetPosition, 8f, collisionLayer);
        for (int i = 0; i < Math.Min(maxAttackableMonster, hitColliders.Length); i++)
        {
            //Debug.Log("현재 궁극기 피격 몬스터의 마릿수 : " + Math.Min(maxAttackableMonster, hitColliders.Length));
            Collider hitCollider = hitColliders[i];

            IMonster targetMonster = hitCollider.gameObject.GetComponent<IMonster>();

            if (targetMonster != null)
            {
                float flyUltimateDamage = ultimateDamage;

                if (isFlyingMonsterInstantDeath)
                {
                    if (hitCollider.gameObject.GetComponent<FlyingSkeleton>() != null)
                    {
                        flyUltimateDamage = 3000f; // 즉사 데미지
                    }
                }

                //Debug.Log($"전사 궁극기로 몬스터를 공격합니다. 공격량 : {_attackDamage}, 공중 몬스터 즉사 여부 : {isFlyingMonsterInstantDeath}");
                Vector3 collisionPosition = hitCollider.ClosestPoint(ultimateTargetPosition);
                targetMonster.OnDamage(flyUltimateDamage, false, collisionPosition, _ownerPhotonViewID);
            }

        }

        yield return new WaitForSeconds(0.6f);

        GameManager.Instance.Destroy(ultimateEffectInstance1);
        GameManager.Instance.Destroy(ultimateEffectInstance2);
        GameManager.Instance.Destroy(ultimateEffectInstance3);
    }

    // private void OnDrawGizmos()
    // {
    //     Vector3 ultimateTargetPosition = transform.position;
    //     ultimateTargetPosition += transform.forward * 5f;
    //     ultimateTargetPosition.y += 0.8f;

    //     Gizmos.color = Color.magenta;
    //     Gizmos.DrawWireSphere(ultimateTargetPosition, 6f);
    // }

    // 전사 궁극기 임시 수정 -> 시간 되면 코루틴 제거로 수정
    private void OnDisable()
    {
        if (ultimateEffectInstance1 != null && ultimateEffectInstance1.activeSelf)
        { GameManager.Instance.Destroy(ultimateEffectInstance1); }
        if (ultimateEffectInstance2 != null && ultimateEffectInstance2.activeSelf)
        { GameManager.Instance.Destroy(ultimateEffectInstance2); }
        if (ultimateEffectInstance3 != null && ultimateEffectInstance3.activeSelf)
        { GameManager.Instance.Destroy(ultimateEffectInstance3); }
    }


    #endregion

    #region 전사 스킬트리

    // 평타 공격력 강화
    [PunRPC]
    public float AttackDamageUp(float damage)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("AttackDamageUp", RpcTarget.Others, damage);
        }

        _attackDamage += damage;
        defaultSword.defaultAttackDamage = _attackDamage;
        //Debug.Log($"전사의 평타 공격력이 증가합니다. 현재 공격력: {_attackDamage}");
        return _attackDamage;
    }

    // 방어력 증가
    [PunRPC]
    public float DefencePowerUp(float amount)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("DefencePowerUp", RpcTarget.Others, amount);
        }

        _defencePower += amount;
        //Debug.Log($"전사의 방어력이 증가합니다. 현재 방어력: {_defencePower}");
        return _defencePower;
    }

    // 최대 체력 증가
    [PunRPC]
    public float IncreaseMaxHP(float percentage)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseMaxHP", RpcTarget.Others, percentage);
        }

        _maxHp *= (1 + percentage);
        //Debug.Log($"전사의 최대 체력이 증가합니다. 현재 최대 체력 : {_maxHp}");
        return _maxHp;
    }

    // 달리기 시 이동속도 증가
    [PunRPC]
    public float IncreaseRunningSpeed(float amount)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseRunningSpeed", RpcTarget.Others, amount);
        }

        _runningSpeed += amount;
        //Debug.Log($"전사의 달리기 이동속도가 증가합니다. 현재 이동 속도 : {_runningSpeed}");
        return _runningSpeed;
    }

    // 적 처치 시 영구적으로 방어력 0.2 증가 >> 처치할 때마다 계속 올라가는 거임?? ㅇㅇ
    [PunRPC]
    public float IncreaseDefencePowerOnMonsterKill(float amount)
    {
        //Debug.Log("전사 구독 실행");
        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseDefencePowerOnMonsterKill", RpcTarget.Others, amount);
        }

        _playerController.SetIncreaseDefensePowerAmount(amount);
        return amount;
    }

    // 부메랑 공격의 크기 증가
    [PunRPC]
    public float IncreaseThrowingSwordScale(float amount)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseThrowingSwordScale", RpcTarget.Others, amount);
        }

        throwingSwordObject.transform.localScale = new Vector3(amount, 1f, amount); // 3f

        //Debug.Log($"전사 스킬 공격의 크기가 증가합니다. 현재 스킬 공격 크기 : {throwingSwordObject.transform.localScale}");

        return amount;
    }

    // 부메랑 공격의 사거리 증가
    [PunRPC]
    public float IncreaseThrowingSwordDistance(float amount)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseThrowingSwordScale", RpcTarget.Others, amount);
        }

        throwingSword.curveAmount += amount; // 10f

        //Debug.Log($"전사 스킬 공격의 사거리가 증가합니다. 현재 스킬 공격 사거리 : {throwingSword.curveAmount}");
        return amount;
    }

    // 평타 공격 속도가 50% 느려지지만 평타 공격력이 50% 증가
    [PunRPC]
    public float SlowStrongDefaultAttack(float speedPercentage, float damagePercentage)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SlowStrongDefaultAttack", RpcTarget.Others, speedPercentage, damagePercentage);
        }

        _attackDuration *= (1 + speedPercentage);
        _attackDamage *= (1 + damagePercentage);
        defaultSword.defaultAttackDamage = _attackDamage;

        //Debug.Log($"전사의 공격 속도가 50% 증가하고 공격력이 50% 증가합니다. 현재 공격 속도 : {_attackDuration}, 현재 공격력: {_attackDamage}");

        return _attackDamage;
    }

    // 스킬 공격의 쿨타임 감소
    [PunRPC]
    public float DecreaseSkillCoolTime(float amount)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("DecreaseSkillCoolTime", RpcTarget.Others, amount);
        }

        _skillDuration -= amount;
        _playerController.SetCooldownDuration(_skillDuration, PlayerController.AttackType.Skill);
        //Debug.Log($"전사의 스킬 쿨타임이 감소합니다. 현재 스킬 쿨타임 : {_skillDuration}");
        return _skillDuration;
    }

    // 궁극기 공격 시 공중 몬스터 즉사
    [PunRPC]
    public bool EnableFlyingMonsterInstantDeath(bool isEnabled)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("EnableFlyingMonsterInstantDeath", RpcTarget.Others, isEnabled);
        }
        //Debug.Log($"전사 궁극기의 공중 몬스터 즉사가 설정되었습니다. 현재 상태 : {isFlyingMonsterInstantDeath}");
        isFlyingMonsterInstantDeath = isEnabled;
        return isFlyingMonsterInstantDeath;
    }

    // 궁극기 공격의 게이지 요구량 감소 >> 영구 감소? ㅇㅇ >> duration..?
    [PunRPC]
    public float DecreaseUltimateDuration(float amount)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("DecreaseUltimateDuration", RpcTarget.Others, amount);
        }
        _ultimateDuration -= amount;
        //Debug.Log($"전사 궁극기 쿨타임 요구량이 감소됩니다. 현재 값 : {_ultimateDuration}");
        return amount;
    }

    // 궁극기 공격이 4갈래로 나감 -> 궁극기 최대 피해 몬스터 5마리에서 10마리로 변경
    [PunRPC]
    public int IncreaseMaxAttackableMonster(int amount)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseMaxAttackableMonster", RpcTarget.Others, amount);
        }
        maxAttackableMonster += amount;
        //Debug.Log($"전사 궁극기의 공격 가능한 몬스터 숫자가 증가합니다. 현재 값 : {maxAttackableMonster}");
        return amount;
    }

    // 적 처치 시 스킬 공격의 쿨타임 1초 감소
    [PunRPC]
    public float DecreaseSkillCoolTimeOnMonsterKill(float amount)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("DecreaseSkillCoolTimeOnMonsterKill", RpcTarget.Others, amount);
        }

        _playerController.SetCooldownReductionAmount(amount);
        return amount;
    }

    #endregion
}
