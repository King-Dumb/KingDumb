using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Mage : CharacterInfo, IPlayerClass
{
    private AttackHolder attackHolder;
    private SkillHolder skillHolder;
    private UltimateHolder ultimateHolder;
    private MageSkillTreeManager skillTreeManager;
    public Transform fireTransform;

    private EffectType currentAttackType = EffectType.Energy;
    private EffectType currentSkillType = EffectType.Energy;
    private EffectType currentUltimateType = EffectType.Energy;

    public Mage()
    {
        _classType = "Mage";
        _maxHp = 90;
        _hp = 90;
        _attackDamage = 20;
        _defencePower = 0;
        _attackDuration = 1.2f;
        _skillDuration = 3f;
        _ultimateDuration = 20f;
        _baseMoveSpeed = _moveSpeed = 6f;
        _runningSpeed = _moveSpeed * 1.8f;
        _skillPoint = 1; // 스킬 포인트
        _level = 1;
    }

    private float _attackSplashDamage = 3;
    private float _skillDamage = 10;

    public void Attack()
    {
        Vector3 fireDirection = Camera.main.transform.forward.normalized;
        Vector3 firePosition = fireTransform.position;

        firePosition += Vector3.up * 0.7f;

        if (fireDirection.y < 0)
        {
            fireDirection = new Vector3(fireDirection.x, 0.001f, fireDirection.z).normalized;
        }
        if (photonView != null)
        {
            photonView.RPC("MageAttackBroadcast", RpcTarget.All, firePosition, fireDirection);
        }
        else
        {
            // 로컬인 경우
            // }
            attackHolder.SetAttackDamage(_attackDamage);
            attackHolder.Attack(currentAttackType, firePosition, fireDirection,
                _ownerPhotonViewID);
        }
    }

    // 공격 멀티 동기화
    [PunRPC]
    public void MageAttackBroadcast(Vector3 firePosition, Vector3 fireDirection)
    {
        // if (attackHolder == null)
        // {
        //     //Debug.Log("Attack Holder는 null");
        // }
        attackHolder.SetAttackDamage(_attackDamage);
        attackHolder.Attack(currentAttackType, firePosition, fireDirection,
            _ownerPhotonViewID);
    }

    public void Skill()
    {
        Vector3 fireDirection = gameObject.transform.forward.normalized;
        fireDirection += new Vector3(0, 0.05f, 0);

        if (photonView != null)
        {
            photonView.RPC("MageSkillBroadcast", RpcTarget.All, fireTransform.position, fireDirection);
        }
        else
        {
            skillHolder.Attack(currentSkillType, fireTransform.position, fireDirection,
                _ownerPhotonViewID);
        }
    }

    [PunRPC]
    public void MageSkillBroadcast(Vector3 firePosition, Vector3 fireDirection)
    {
        skillHolder.Attack(currentSkillType, fireTransform.position, fireDirection,
            _ownerPhotonViewID);
    }

    public void Ultimate()
    {
        //Debug.Log("마법사 궁");

        Vector3 fireDirection = gameObject.transform.forward.normalized;
        //ultimateHolder.ActivateEffect(currentUltimateType, fireTransform.position, fireDirection);

        StartCoroutine(SetMoveSpeedForSeconds(0f, 1.2f));

        if (photonView != null)
        {
            photonView.RPC("MageUltimateBroadcast", RpcTarget.All, fireTransform.position, fireDirection);
        }
    }

    private IEnumerator SetMoveSpeedForSeconds(float speed, float seconds)
    {
        _playerController.IsStop(true);
        //Debug.Log("움직임 속도 감소 시작");
        //float originSpeed = _moveSpeed;
        //_moveSpeed = speed;
        yield return new WaitForSeconds(seconds);
        //_moveSpeed = originSpeed;

        //Debug.Log("움직임 속도 끝");
        _playerController.IsStop(false);
    }

    [PunRPC]
    public void MageUltimateBroadcast(Vector3 firePosition, Vector3 fireDirection)
    {
        ultimateHolder.Attack(currentUltimateType, firePosition, fireDirection, _ownerPhotonViewID);
    }

    [PunRPC]
    public void ChangeSkillType(EffectType effectType)
    {
        currentUltimateType = effectType;
        currentAttackType = effectType;
        currentSkillType = effectType;


        if (effectType == EffectType.Darkness)
        {
            SetMaxHP(_maxHp - 10);
            AttackDamageUp(10f);
            SkillDamageUp(10f);
            AttackProjectileSpeedUp(3f);
        }

        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("ChangeSkillType", RpcTarget.Others, effectType);
        }
    }

    // private void OnEnable()
    // {
    //     //InitializeComponents();
    //     //Debug.Log("초기화 Start");
    //     attackHolder = GetComponentInChildren<AttackHolder>();
    //     skillHolder = GetComponentInChildren<SkillHolder>();
    //     ultimateHolder = GetComponentInChildren<UltimateHolder>();

    //     attackHolder.InitializeEffects();
    //     skillHolder.InitializeEffects();
    //     ultimateHolder.InitializeEffects();

    //     skillTreeManager = GetComponentInChildren<MageSkillTreeManager>();
    // }
    void Start()
    {
        InitializeComponents();
        //Debug.Log("하위 클래스 Start");
        attackHolder = GetComponentInChildren<AttackHolder>();
        skillHolder = GetComponentInChildren<SkillHolder>();
        ultimateHolder = GetComponentInChildren<UltimateHolder>();

        attackHolder.InitializeEffects();
        skillHolder.InitializeEffects();
        ultimateHolder.InitializeEffects();

        skillTreeManager = GetComponentInChildren<MageSkillTreeManager>();

        //ApplyKillCooldownReduction();

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

    }

    [PunRPC]
    public float AttackDamageUp(float damage)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("AttackDamageUp", RpcTarget.Others, damage);
        }

        _attackDamage += damage;

        // 모든 클라이언트에서 공격력을 증가시킴
        return _attackDamage;
    }

    [PunRPC]
    public float AttackSplashDamageUp(float damage)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("AttackSplashDamageUp", RpcTarget.Others, damage);
        }

        // 모든 클라이언트에서 공격력을 증가시킴
        return attackHolder.SplashDamageUp(damage);
    }

    [PunRPC]
    public float AttackSpeedUp(float damage)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("AttackSplashDamageUp", RpcTarget.Others, damage);
        }

        // 모든 클라이언트에서 스플래시 공격력을 증가시킴
        return attackHolder.SplashDamageUp(damage);
    }

    [PunRPC]
    public float AttackProjectileSpeedUp(float speed)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("AttackSplashDamageUp", RpcTarget.Others, speed);
        }

        // 모든 클라이언트에서 투사체 속도를 증가시킴
        return attackHolder.ProjectileSpeedUp(speed);
    }

    [PunRPC]
    public bool AttackTrackingActivate()
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("AttackTrackingActivate", RpcTarget.Others);
        }

        // 모든 클라이언트에서 투사체의 추적을 활성화 시킴
        return attackHolder.TrackingActivate();
    }

    [PunRPC]
    public float SkillDamageUp(float damage)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("SkillDamageUp", RpcTarget.Others, damage);
        }

        // 모든 클라이언트에서 공격력을 증가시킴
        return skillHolder.SkillDamageUp(damage);
    }

    [PunRPC]
    public void ApplyKillCooldownReduction()
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        //Debug.Log("쿨타임 감소 테스트 세팅");
        // 모든 클라이언트에서 쿨타임 감소를 시킴(필요한가?)
        _playerController.SetCooldownReductionAmount(0.5f);
        SetMaxHP(_maxHp - 10);
        //Debug.Log(this._maxHp);
    }

    [PunRPC]
    public Vector3 IncreaseSkillRange()
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseSkillRange", RpcTarget.Others);
        }

        // 모든 클라이언트에서 스킬의 범위를 늘림
        return skillHolder.IncreaseSkillRange();
    }

    [PunRPC]
    public void DecreaseSkillCooldown()
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("DecreaseSkillCooldown", RpcTarget.Others);
        }

        // 모든 클라이언트에서 쿨타임 감소를 시킴(필요한가?)
        _playerController.SetCooldownDuration(_skillDuration - 1, PlayerController.AttackType.Skill);
    }

    [PunRPC]
    public void DecreaseUltimateCooldown()
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("DecreaseUltimateCooldown", RpcTarget.Others);
        }

        // 모든 클라이언트에서 쿨타임 감소를 시킴(필요한가?)
        _playerController.SetCooldownDuration(_ultimateDuration - 5, PlayerController.AttackType.Ultimate);
    }

    [PunRPC]
    public float DecreaseSkillCastDelay()
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("DecreaseSkillCooldown", RpcTarget.Others);
        }

        // 모든 클라이언트에서 쿨타임 감소를 시킴(필요한가?)
        return skillHolder.DecreaseSkillCastDelay();
    }

    [PunRPC]
    public float DecreaseReviveTime()
    {
        _reviveTime = 6;
        return _reviveTime;
    }

    // 궁극기 데미지 상승
    [PunRPC]
    public float UltimateDamageUp()
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("UltimateDamageUp", RpcTarget.Others);
        }

        // 모든 클라이언트에서 공격력을 증가시킴
        return ultimateHolder.UltimateDamageUp();
    }
}
