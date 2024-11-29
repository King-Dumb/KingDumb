using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Priest : CharacterInfo, IPlayerClass
{

    public GameObject attackEffectPrefab;
    public GameObject skillEffectPrefab;
    public GameObject ultimateGolemPrefab;

    private float skillDuration = 5f; // 스킬 지속 시간
    private float skillPricePerSecond = 6f; // 초당 스킬 데미지/힐량
    private bool isAllPlayer = false; // 모든 플레이어에게 스킬 적용할지 여부
    private float ultimateDuration = 20f; // 궁극기 지속 시간
    private float golemSpeed = 5f;
    private float moveSpeedOnSkill; // 스킬 지속 동안 이동속도
    private bool isSpeedUp = false; // 스킬 지속 동안 이동속도 증가 여부
    private bool canHealNexus = false; // 스킬이 넥서스를 힐할 수 있는지 여부

    public Priest()
    {
        _classType = "Priest";
        _maxHp = _hp = 80; // 기준 최대 체력, 채력
        _baseMoveSpeed = _moveSpeed = 5f; // 기준 이동 속도, 이동 속도
        _runningSpeed = 2f * _moveSpeed; // 달리기 속도
        _baseAttackDamage = _attackDamage = 10f; // 기준 공격력, (버프 받는) 공격력
        _baseSkillDamage = _skillDamage = 6f; // 기준 스킬 공격력, (버프 받는) 스킬 공격력
        _baseUltimateDamage = _ultimateDamage = 15f; // 기준 궁극기 공격력, (버프 받는) 궁극기 공격력
        _defencePower = 0; // 방어력
        _attackDuration = 0.8f; // 공격속도(초)
        _skillDuration = 3f; // 스킬 쿨타임(초)
        _ultimateDuration = 30f; // 궁 쿨타임(초)
        _level = 1; // 레벨
        _skillPoint = 1; // 스킬 포인트
    }

    public void Attack()
    {
        //Debug.Log("사제 공격");
        // AddSkillPoint();
        // 넉백 공격
        Vector3 fireballDir = Camera.main.transform.forward.normalized;
        Vector3 fireballPos = transform.Find("FirePosition").transform.position; // 발사 위치 설정

        if (fireballDir.y < 0)
        {
            fireballDir = new Vector3(fireballDir.x, 0.001f, fireballDir.z).normalized;
        }

        if (photonView != null)
        {
            photonView.RPC("AttackBroadcast", RpcTarget.All, fireballPos, fireballDir);
        }
        else
        {
            // Effect
            ActiveAttackEffect(fireballPos, fireballDir, _ownerPhotonViewID);
        }
    }

    void Start()
    {
        InitializeComponents();

        if (photonView.IsMine)
        {
            LoadSkill();
        }
        else
        {
            LoadSkillRequestToOwner();
        }

        _hp = _maxHp;
        skillPricePerSecond = _skillDamage;
    }

    [PunRPC]
    public void AttackBroadcast(Vector3 firePos, Vector3 fireDir)
    {
        ActiveAttackEffect(firePos, fireDir, _ownerPhotonViewID);
    }

    private void ActiveAttackEffect(Vector3 pos, Vector3 dir, int ownerPvId)
    {
        Debug.Log("적용되는 데미지는:" + _attackDamage);
        GameObject attackProjectile = Instantiate(attackEffectPrefab, pos + dir, Quaternion.identity);
        attackProjectile.GetComponent<PriestAttack>().SetOwnerPvId(ownerPvId);
        attackProjectile.GetComponent<PriestAttack>().SetDirection(dir);
        attackProjectile.GetComponent<PriestAttack>().SetDamage(_attackDamage);
        attackProjectile.GetComponent<PriestAttack>().SetSpeed(30f);
    }

    public void Skill()
    {
        //Debug.Log("사제 스킬");

        // 광역 힐/딜

        // 현재 위치를 기준으로 설정 범위만큼 힐 영역 생성

        if (photonView != null)
        {
            photonView.RPC("SkillBroadcast", RpcTarget.All);
        }
        else
        {
            ActiveSkillEffect(_ownerPhotonViewID);
        }
    }

    [PunRPC]
    public void SkillBroadcast()
    {
        ActiveSkillEffect(_ownerPhotonViewID);
    }

    private void ActiveSkillEffect(int ownerPvId)
    {
        Vector3 pos = transform.position;
        pos.y += 0.1f;
        GameObject skillField = GameManager.Instance.Instantiate(skillEffectPrefab.name, pos, skillEffectPrefab.transform.rotation);
        PriestSkill priestSkill = skillField.GetComponent<PriestSkill>();
        priestSkill.SetOwnerPvId(ownerPvId);
        priestSkill.SetDuration(skillDuration);
        priestSkill.SetPricePerSecond(skillPricePerSecond);
        priestSkill.SetAllPlayer(isAllPlayer);
        priestSkill.SetMoveSpeedUp(moveSpeedOnSkill);
        priestSkill.SetIsSpeedUp(isSpeedUp);
        priestSkill.SetCanHealNexus(canHealNexus);
        GameManager.Instance.Destroy(skillField, skillDuration);
    }

    public void Ultimate()
    {
        //Debug.Log("사제 궁");

        Vector3 summonPos = transform.position;
        summonPos.x += 4f;

        // 소환수
        if (photonView != null && photonView.IsMine)
        {
            //photonView.RPC("UltimateBroadcast", RpcTarget.All, summonPos);
            ActiveUltimateEffect(summonPos, _ownerPhotonViewID);
        }      
    }

    [PunRPC]
    public void UltimateBroadcast(Vector3 pos)
    {
        ActiveUltimateEffect(pos, _ownerPhotonViewID);
    }

    private void ActiveUltimateEffect(Vector3 pos, int ownerPvId)
    {
        GameObject golem = PhotonNetwork.Instantiate("Priest_Golem", pos, ultimateGolemPrefab.transform.rotation);
        PhotonView pv = golem.GetComponent<PhotonView>();
        CharacterManager.Inst.RegisterPlayerToMaster(pv.ViewID);
        //Debug.Log($"등록한 골렘의 포톤 뷰 아이디는: " +  pv.ViewID);

        golem.GetComponent<PriestGolem>().SetOwnerPvId(ownerPvId);
        golem.GetComponent<PriestGolem>().SetPriest(gameObject.transform);
        golem.GetComponent<PriestGolem>().SetDamage(_attackDamage * 8);
        golem.GetComponent<PriestGolem>().SetSpeed(golemSpeed);
        
        GameManager.Instance.DestroyPhotonObj(golem, ultimateDuration);
    }

    [PunRPC]
    public float AttackDamageUp(float damage)
    {
        // 로컬 플레이어라면 RPC를 사용해 네트워크로 전달
        if (photonView.IsMine)
        {
            photonView.RPC("AttackDamageUp", RpcTarget.Others, damage);
        }

        // 모든 클라이언트에서 공격력을 증가시킴
        return _attackDamage += damage;
    }

    [PunRPC]
    public float ReviveTimeDown(float time)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("ReviveTimeDown", RpcTarget.Others, time);
        }
        return _reviveTime -= time;
    }

    [PunRPC]
    public float MaxHpUp(float hp)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("MaxHpUp", RpcTarget.Others, hp);
        }

        return _maxHp += hp;
    }

    [PunRPC]
    public float SpeedUp(float speed)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SpeedUp", RpcTarget.Others, speed);
        }

        _baseMoveSpeed += speed;
        _moveSpeed = _baseMoveSpeed;
        _runningSpeed = _moveSpeed * 2;

        return _moveSpeed;
    }

    [PunRPC]
    public IEnumerator RecoverHp(float hp)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("RecoverHp", RpcTarget.Others, hp);
        }

        while (true)
        {
            // 체력을 회복하고 최대 체력을 넘지 않도록 제한
            _hp = Mathf.Min(_hp + hp, _maxHp);

            // 1초 대기
            yield return new WaitForSeconds(1f);
        }
    }

    [PunRPC]
    public void SkillTimeUp(float time)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SkillTimeUp", RpcTarget.Others, time);
        }

        skillDuration += time;
    }

    [PunRPC]
    public void SkillPriceUp(float price)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SkillPriceUp", RpcTarget.Others, price);
        }

        skillPricePerSecond += price;
    }

    [PunRPC]
    public void GolemSpeedUp(float speed)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("GolemSpeedUp", RpcTarget.Others, speed);
        }

        golemSpeed += speed;
    }

    [PunRPC]
    public void SkillDurationDown(float dur)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SkillDurationDown", RpcTarget.Others, dur);
        }

        _skillDuration -= dur;
    }

    [PunRPC]
    public void GolemDurationUp(float dur)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("GolemDurationUp", RpcTarget.Others, dur);
        }

        ultimateDuration += dur;
    }

    [PunRPC]
    public void SkillAllPlayer()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SkillAllPlayer", RpcTarget.Others);
        }

        isAllPlayer = true;
    }

    [PunRPC]
    public void ApplyAttackDamageUp(float attackDamage)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("ApplyAttackDamageUp", RpcTarget.Others, attackDamage);
        }

        _playerController.SetAttackDamage(attackDamage);
    }

    [PunRPC]
    public void SpeedUpOnSkill(float amount)
    {
        if (photonView.IsMine)
        {
            photonView.RPC("SpeedUpOnSkill", RpcTarget.Others, amount);
        }

        isSpeedUp = true;
        moveSpeedOnSkill = amount;
    }

    [PunRPC]
    public void ApplyCanHealNexus()
    {
        if (photonView.IsMine)
        {
            photonView.RPC("ApplyCanHealNexus", RpcTarget.Others);
        }
        canHealNexus = true;
    }
}
