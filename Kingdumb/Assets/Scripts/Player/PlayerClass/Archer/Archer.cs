using System;
using System.Collections;
using Photon.Pun;
using UnityEngine;

public class Archer : CharacterInfo, IPlayerClass
{
    public Transform generateTransform;
    public GameObject arrowPrefab;
    public GameObject chargingArrowPrefab;
    public GameObject chargingEffectPrefab1;
    public GameObject chargingEffectPrefab2;
    public GameObject magicArrowPrefab;
    public GameObject skillEffectPrefab;
    public GameObject ultimatePrefab;
    public GameObject portalPrefab;
    public ChargingBarController chargingBarController;

    private int _arrowCount;
    private int _attackCount;
    private int _skillCount;
    private float _skillDamage;
    private bool _isMagic;
    private bool _isDestroy;
    private bool _isKnockBack;
    private bool _isTracing;

    private GameObject chargingEffectInstance;
    private ArcherSkillTreeManager skillTreeManager;

    private bool isCharging;
    private float chargeMultiplier; // 차징 시간에 따른 배율 계수
    private float prevChargeMultiplier; // 이펙트 갱신의 판단에 사용할 기록용 배율 계수
    private float chargeSkillTreeMultiplier; // 스킬 트리 전용 배율 계수의 배율
    private float chargeStartTime; // 차징 시작 시간
    private float chargeDuration; // 차징 지속 시간
    private float maxChargingTime; // 차징 최대 시간
    private float fullCharge; // 최대 차징 배수
    private float middleCharge; // 중간 차징 배수
    public bool IsSkillActive { get; private set; } = false; // 스킬이 활성화가 되어있는지

    public Archer()
    {
        _classType = "Archer";
        _maxHp = _hp = 100; // 기준 최대 체력, 채력
        _baseMoveSpeed = _moveSpeed = 5f; // 기준 이동 속도, 이동 속도
        _runningSpeed = 2 * _moveSpeed; // 달리기 속도
        _baseAttackDamage = _attackDamage = 12; // 기준 공격력, 공격력
        _defencePower = 0; // 방어력
        _attackDuration = 0.6f; // 공격속도(초)
        _skillDuration = 20f; // 스킬 쿨타임(초)
        _ultimateDuration = 30f; // 궁 쿨타임(초)
        _level = 1; // 레벨
        _skillPoint = 1; // 스킬 포인트
    }

    void Start()
    {
        InitializeComponents();
        maxChargingTime = 1f;
        chargeMultiplier = 1.0f;
        prevChargeMultiplier = 1.0f;
        chargeSkillTreeMultiplier = 1.0f;
        _arrowCount = 0;
        _attackCount = 1;
        _skillCount = 3;
        _skillDamage = 1.3f;
        _isMagic = false;
        _isDestroy = true;
        _isKnockBack = false;
        _isTracing = false;
        fullCharge = 3f;
        middleCharge = 1.5f;
        
        skillTreeManager = GetComponentInChildren<ArcherSkillTreeManager>();

        if (photonView.IsMine)
        {
            LoadSkill();
        }
        else
        {
            LoadSkillRequestToOwner();
        }
    }

    void Update()
    {
        if (isCharging && chargeDuration <= maxChargingTime)
        {
            UpdateChargeEffect(); // 차징 이펙트 업데이트 호출
            chargingBarController.UpdateChargeBar(chargeDuration);
        }
    }

    // public float SkillEffectDurationTime { get; private set; }

    // 차징 시간에 따른 데미지 배율 계산 메서드
    private float GetChargeMultiplier(float chargeDuration)
    {
        if (chargeDuration >= maxChargingTime)
        {
            return fullCharge * chargeSkillTreeMultiplier; // 최대 차징시간 이상일 때 배율 3배
        }
        else if (chargeDuration >= maxChargingTime / 2)
        {
            return middleCharge * chargeSkillTreeMultiplier; // 위 값의 1/2 이상일 때 배율 1.5배
        }
        else
        {
            return 1.0f; // 기본 배율
        }
    }

    // 차징 시작 메서드
    public void StartCharging()
    {
        isCharging = true;
        chargeDuration = 0;
        chargeStartTime = Time.time; // 차징 시작 시간을 기록
        chargingBarController.SetMaxChargeTime(maxChargingTime);
        chargingBarController.StartCharging();
    }

    // 차징 이펙트 업데이트 메서드
    private void UpdateChargeEffect()
    {
        chargeDuration = Time.time - chargeStartTime;
        chargeMultiplier = GetChargeMultiplier(chargeDuration);

        // 차징 단계가 변경될 때만 이펙트를 갱신
        if (chargeMultiplier != prevChargeMultiplier)
        {
            prevChargeMultiplier = chargeMultiplier; // 현재 차징 단계로 업데이트
            Vector3 position = generateTransform.position;
            photonView.RPC("ArcherChargingEffectBroadcast", RpcTarget.All, position, chargeMultiplier);
        }
    }

    [PunRPC]
    public void ArcherChargingEffectBroadcast(Vector3 position, float updatedChargeMultiplier)
    {
        chargeMultiplier = updatedChargeMultiplier;

        GameObject effect = null;
        if (chargeMultiplier == middleCharge * chargeSkillTreeMultiplier)
        {
            effect = chargingEffectPrefab1;
        }
        else if (chargeMultiplier == fullCharge * chargeSkillTreeMultiplier)
        {
            effect = chargingEffectPrefab2;
        }

        // 현재 활성화된 이펙트와 다른 경우에만 교체
        if (effect != null && (chargingEffectInstance == null || chargingEffectInstance.name != effect.name + "(Clone)"))
        {
            if (chargingEffectInstance != null)
            {
                GameManager.Instance.Destroy(chargingEffectInstance);
            }
            chargingEffectInstance = GameManager.Instance.Instantiate(effect.name, position, Quaternion.identity);
            chargingEffectInstance.transform.SetParent(transform, true);
            chargingEffectInstance.transform.localPosition = new Vector3(-0.1f, 0.6f, 0.2f);
        }
    }

    // 차징 취소 메서드
    public void CancelCharging()
    {
        isCharging = false;
        chargeMultiplier = 1.0f;
        chargingBarController.UpdateChargeBar(0);
        chargingBarController.StopCharging();

        if (photonView != null)
        {
            photonView.RPC("ArcherCancelChargingEffect", RpcTarget.All);
        }
        else
        {
            ArcherCancelChargingEffect();
        }
    }

    [PunRPC]
    public void ArcherCancelChargingEffect()
    {
        if (chargingEffectInstance != null)
        {
            GameManager.Instance.Destroy(chargingEffectInstance);
            chargingEffectInstance = null;
        }
    }

    public void Attack()
    {

        if (IsSkillActive)
        {
            _arrowCount = _skillCount;
        }
        else
        {
            _arrowCount = _attackCount;
        }

        StartCoroutine(ContinuousArrowAttack());

        // 차징 공격 후 차징 초기화
        if (isCharging)
        {
            CancelCharging();
        }
    }

    public void Skill()
    {
        Vector3 position = generateTransform.position;

        IsSkillActive = true;
        // 마법화살이 활성화 되었다는 이펙트
        if (photonView != null)
        {
            photonView.RPC("ArcherSkillEffectBroadcast", RpcTarget.All, position);
        }
        else
        {
            ArcherSkillEffectBroadcast(position);
        }
    }

    [PunRPC]
    public void ArcherSkillEffectBroadcast(Vector3 position)
    {
        GameObject skillEffect = GameManager.Instance.Instantiate(skillEffectPrefab.name, position, Quaternion.Euler(-90, 0, 0));
        skillEffect.transform.SetParent(transform, true);
        skillEffect.transform.localPosition = new Vector3(0f, 0.1f);
        GameManager.Instance.Destroy(skillEffect, 10f);
        Invoke(nameof(EndSkill), 10f);
    }

    private void EndSkill()
    {
        IsSkillActive = false;
    }

    private IEnumerator ContinuousArrowAttack()
    {
        bool localIsSkillActive = IsSkillActive; // 현재 상태를 저장

        for (int i = 0; i < _arrowCount; i++)
        {
            // 여기에 디버그 로그 추가해서 원인 알아보기
            Vector3 direction = Camera.main.transform.forward.normalized;
            if (direction.y < 0)
            {
                direction = new Vector3(direction.x, 0.001f, direction.z).normalized;
            }
            
            // 캐릭터의 바라보는 방향(direction)으로부터 일정 거리 앞에 화살을 생성
            Vector3 position = generateTransform.position;

            GameObject useArrowPrefab;
            
            float arrowDmg;

            if (localIsSkillActive)
            {
                // 스킬 활성화 시 마법화살
                useArrowPrefab = magicArrowPrefab;
                arrowDmg = _baseAttackDamage * _skillDamage;
                // 캐릭터의 바라보는 방향(direction)으로부터 일정 거리 앞에 화살을 생성
                position += direction * 1f;
            }
            else if (isCharging && chargingEffectInstance != null)
            {
                // 차징 활성화가 되었고 배율 적용 시(차징 이펙트 적용 시) 차징화살
                useArrowPrefab = chargingArrowPrefab;
                arrowDmg = _baseAttackDamage * chargeMultiplier;
            }
            else
            {
                // 기본 화살
                useArrowPrefab = arrowPrefab;
                arrowDmg = _baseAttackDamage;
            }

            if (photonView != null)
            {
                photonView.RPC("ArcherArrowBroadcast", RpcTarget.All, useArrowPrefab.name, position, direction, arrowDmg);
            }
            else
            {
                ArcherArrowBroadcast(useArrowPrefab.name, position, direction, arrowDmg);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    #region 궁수 화살 발사
    [PunRPC]
    public void ArcherArrowBroadcast(string useArrowPrefabName, Vector3 position, Vector3 direction, float damage)
    {
        GameObject arrow = GameManager.Instance.Instantiate(useArrowPrefabName, position, Quaternion.LookRotation(direction));        
        if (arrow.TryGetComponent<IArrow>(out var arrowType))
        {
            // Debug.Log($"공격력: {_baseAttackDamage}\n차징 배율: {chargeMultiplier}\n현재 화살 데미지 설정: {damage}");
            arrowType.SetDamage((int) damage);
            arrowType.SetDirection(direction);
            arrowType.SetOwnerPhotonViewID(_ownerPhotonViewID);

            // 만약 스킬트리에서 방어구 관통이 활성화 되었다면 물리 관통 화살 활성화
            if (_isMagic && arrow.TryGetComponent<MagicArrow>(out var magicArrow))
            {
                magicArrow.SetIsMagic(_isMagic);
            }

            // 만약 스킬트리에서 차지샷 업그레이드가 활성화 되었다면 화살 충돌 시 없어지지 않음
            if (!_isDestroy && arrow.TryGetComponent<Arrow>(out var chargeArrow))
            {
                chargeArrow.SetIsDestroy(_isDestroy);
            }

            // 만약 스킬트리에서 넉백 업그레이드가 활성화 되었다면 활성화
            if (_isKnockBack && arrow.TryGetComponent<Arrow>(out var generalArrow))
            {
                generalArrow.SetIsKnockBack(_isKnockBack);
            }

            // 만약 스킬트리에서 추적 업그레이드가 활성화 되었다면 활성화
            if (_isTracing && arrow.TryGetComponent<Arrow>(out var tracingArrow))
            {
                tracingArrow.SetIsTracing(_isTracing);
            }
        }
    }
    #endregion

    public void Ultimate()
    {
        _playerController.IsStop(true);

        // 카메라의 방향을 따라가되 위치의 y값만 변경
        Vector3 generatePos = generateTransform.position;
        Vector3 ultimateDirection = Camera.main.transform.forward;

        StartCoroutine(UltimateCoroutine(generatePos, ultimateDirection));
    }

    private IEnumerator UltimateCoroutine(Vector3 generatePos, Vector3 dir)
    {
        // 포탈 생성 위치
        Vector3 spawnPortalPos = new(generatePos.x + dir.x * 2f, generatePos.y + 2f, generatePos.z + dir.z * 2f);
        // 화살 생성 위치
        Vector3 spawnArrowPos = new(generatePos.x + dir.x * 10f, generatePos.y + 2f, generatePos.z + dir.z * 10f);

        if (photonView != null)
        {
            photonView.RPC("ArcherPortalBroadcast", RpcTarget.All, spawnPortalPos, dir);
        }
        else
        {
            ArcherPortalBroadcast(spawnPortalPos, dir);
        }

        // 포탈이 나타난 후 대기
        yield return new WaitForSeconds(1.5f);

        if (photonView != null)
        {
            photonView.RPC("ArcherUltimateBroadcast", RpcTarget.All, spawnArrowPos, dir);
        }
        else
        {
            ArcherUltimateBroadcast(spawnArrowPos, dir);
        }

        _playerController.IsStop(false);
    }

    [PunRPC]
    public void ArcherPortalBroadcast(Vector3 position, Vector3 direction)
    {
        // 포탈 생성
        GameManager.Instance.Instantiate(portalPrefab.name, position, Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up));
    }

    [PunRPC]
    public void ArcherUltimateBroadcast(Vector3 position, Vector3 direction)
    {
        Quaternion dir = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z), Vector3.up);
        // 궁극화살 생성
        GameObject ultimateArrow = GameManager.Instance.Instantiate(ultimatePrefab.name, position, dir);
        if (ultimateArrow.TryGetComponent<IArrow>(out var arrow))
        {
            arrow.SetDamage((int) _attackDamage * 0.3f);
            arrow.SetDirection(new Vector3(direction.x, 0, direction.z));
            arrow.SetOwnerPhotonViewID(_ownerPhotonViewID);
        }
    }

    #region 스킬트리
    [PunRPC]
    public float AttackDamageUp(float damage)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("AttackDamageUp", RpcTarget.Others, damage);
        }

        _baseAttackDamage += damage;
        _attackDamage = _baseAttackDamage;

        return _attackDamage;
    }

    [PunRPC]
    public float MoveSpeedUp(float speed)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("MoveSpeedUp", RpcTarget.Others, speed);
        }

        _baseMoveSpeed += speed;
        _moveSpeed = _baseMoveSpeed;
        _runningSpeed = _moveSpeed * 2;

        return _moveSpeed;
    }

    [PunRPC]
    public float DecreaseAttackDuration(float time)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("DecreaseAttackDuration", RpcTarget.Others, time);
        }

        _attackDuration -= time;

        return _attackDuration;
    }

    [PunRPC]
    public float DecreaseChargingDuration(float time)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("DecreaseChargingDuration", RpcTarget.Others, time);
        }

        maxChargingTime -= time;

        return maxChargingTime;
    }

    [PunRPC]
    public float IncreaseAttackCount(int count)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseAttackCount", RpcTarget.Others, count);
        }

        _attackCount += count;
        // Debug.Log("화살 수 증가" + _attackCount);

        return _attackCount;
    }

    [PunRPC]
    public float SkillDamageUp(float damage)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("SkillDamageUp", RpcTarget.Others, damage);
        }

        _skillDamage += damage;

        return _skillDamage;
    }

    [PunRPC]
    public void MakeSkillMagic(bool isMagic)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("MakeSkillMagic", RpcTarget.Others, isMagic);
        }

        _isMagic = isMagic;
    }

    [PunRPC]
    public void ChargeUpgrade(bool isDestroy)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("ChargeUpgrade", RpcTarget.Others, isDestroy);
        }

        _isDestroy = isDestroy;
        chargeSkillTreeMultiplier /= 2;
    }

    [PunRPC]
    public float IncreaseSkillAttackCount(int count)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseSkillAttackCount", RpcTarget.Others, count);
        }

        _skillCount += count;

        return _skillCount;
    }

    [PunRPC]
    public float DecreaseUltimateDuration(float time)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("DecreaseUltimateCooldown", RpcTarget.Others, time);
        }

        _ultimateDuration -= time;

        return _ultimateDuration;
    }

    [PunRPC]
    public void AddKnockBack(bool isKnockBack)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("AddKnockBack", RpcTarget.Others, isKnockBack);
        }

        _isKnockBack = isKnockBack;
    }

    [PunRPC]
    public void ApplyKillCooldownReduction()
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("ApplyKillCooldownReduction", RpcTarget.Others);
        }

        _playerController.SetUltimateCooldownReductionAmount(1f);
    }

    [PunRPC]
    public void AttackTrackingActivate()
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("AttackTrackingActivate", RpcTarget.Others);
        }

        _isTracing = true;
    }
}
#endregion