using UnityEngine;
using Cinemachine;
using Photon.Pun;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using static PlayerSoundEntry;

public class PlayerController : MonoBehaviourPun
{
    public enum AttackType { Attack, Skill, Ultimate }
    private CharacterInfo characterInfo; //직업 정보
    private IPlayerClass playerClass; //현재 직업

    //여기에 플레이어가 하는 모든 행동을 수행한다.
    public bool isRun; // 달리기가 가능한 상태인지
    private bool isAttack; // 기본 공격 중인지
    public bool isCharging; // 차징 중인지
    private bool isSkill; // 스킬 시전 중인지
    private bool isUltimate; // 궁극기 시전 중인지
    private bool _isSlow = false; // 느려지는 상태인지
    private bool _isStop; // 멈췄는지
    public bool isNexusCaptured = false; // 왕자를 들었는지
    private bool isSkillTreeUIActive = false;

    Animator animator;
    private InputHandler inputHandler;

    // attack cooldown
    private Coroutine attackCooldownCoroutine;
    private float attackCooldownDuration = 1f; // 공격 쿨타임 시간
    private float attackRemainingCooldown = 0f;     // 남은 쿨타임 시간

    // skill cooldown
    private Coroutine skillCooldownCoroutine;
    public float skillCooldownDuration = 1f; // 스킬 쿨타임 시간
    public float skillRemainingCooldown = 0f;     // 남은 쿨타임 시간

    // ultimate cooldown
    private Coroutine ultimateCooldownCoroutine;
    public float ultimateCooldownDuration = 1f; // 스킬 쿨타임 시간
    public float ultimateRemainingCooldown = 0f;     // 남은 쿨타임 시간

    private float cooldownReductionAmount = 0f;
    private float ultimateCooldownReductionAmount = 0f;
    private float attackDamageAmount = 0f;

    // attckDamage buff
    private Coroutine attackDamageBuffCoroutine;

    // defense buff
    private float defenseBuffAmount = 0f;
    private Coroutine defenseBuffCoroutine;

    private PlayerSoundComponent _soundComponent; // 플레이어가 발생시키는 소리를 관리하는 컴포넌트

    public GameObject playerMiniMapObject;


    //플레이어의 클래스를 세팅한다.
    public void SetPlayerClass(IPlayerClass _playerClass)
    {
        playerClass = _playerClass;
        characterInfo = _playerClass as CharacterInfo;

        //그 외에 플레이어 스킨이나 무기 세팅등을 변경한다.
        switch (characterInfo.GetClassType())
        {
            case "Warrior":
                animator.SetFloat("CharacterPosition", 0f);
                break;
            case "Archer":
                animator.SetFloat("CharacterPosition", 1f);
                break;
            case "Mage":
                animator.SetFloat("CharacterPosition", 2f);
                break;
            case "Priest":
                animator.SetFloat("CharacterPosition", 3f);
                break;
        }
    }

    void Awake()
    {
        animator = GetComponent<Animator>();
        inputHandler = GetComponent<InputHandler>();
        _soundComponent = GetComponent<PlayerSoundComponent>();

        characterInfo = GetComponent<CharacterInfo>();
    }

    void Start()
    {
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }

        playerMiniMapObject.GetComponent<Renderer>().material.SetColor("_BaseColor", Color.green);

        // 몬스터 생성 시 SubscribeMonsterDeath실행
        //if (MonsterGenerator.Inst != null)
        //{
        //    MonsterGenerator.Inst.OnMonsterSpawned += SubscribeToMonsterDeath;
        //}

        isRun = false;
        isAttack = false;
        isCharging = false;
        isSkill = false;
        isUltimate = false;
        _isStop = false;
    }

    void Update()
    {
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }

        if (_isStop)
        {
            return;
        }

        LookAround();
        Move();

        if (inputHandler.RunKey && !isCharging && !isNexusCaptured && !_isSlow)
        {
            isRun = true;
        }
        else
        {
            isRun = false;
        }

        if (!isRun && !isNexusCaptured)
        {
            // 공격 관련
            if (inputHandler.AttackKey && !isAttack)
            {
                Attack();
            }
            if (characterInfo is Archer archer)
            {
                if (inputHandler.ChargingKey && !isAttack && !isCharging && !archer.IsSkillActive)
                {
                    Charging();
                }

                if (inputHandler.ChargingKeyUp && isCharging)
                {
                    Attack();
                    CancelCharging();
                }
            }
            if (inputHandler.SkillKeyDown && !isCharging && !isSkill)
            {
                Skill();
            }
            if (inputHandler.UltimateKeyDown && !isCharging && !isUltimate)
            {
                Ultimate();
            }
        }
    }

    #region 이동관련
    public void Move()
    {
        Vector3 forward = transform.forward * inputHandler.VAxis;
        Vector3 right = transform.right * inputHandler.HAxis;
        Vector3 moveDir = (forward + right).normalized;

        if (isRun)
        {
            characterInfo.SetMoveSpeed(characterInfo.GetRunningSpeed());
        }
        else if (_isSlow || isNexusCaptured)
        {
            characterInfo.SetMoveSpeed(characterInfo.GetBaseMoveSpeed() / 2);
        }
        else
        {
            characterInfo.SetMoveSpeed(characterInfo.GetBaseMoveSpeed());
        }

        // if (isBuffed)
        // {

        // }

        transform.position += characterInfo.GetMoveSpeed() * Time.deltaTime * moveDir;

        // 애니메이션 관련
        float targetVertical = inputHandler.VAxis * (isRun ? 2 : 1);
        float targetHorizontal = inputHandler.HAxis * (isRun ? 2 : 1);

        float currentVertical = animator.GetFloat("Vertical");
        float currentHorizontal = animator.GetFloat("Horizontal");

        currentVertical = Mathf.Lerp(currentVertical, targetVertical, Time.deltaTime * 10f);
        currentHorizontal = Mathf.Lerp(currentHorizontal, targetHorizontal, Time.deltaTime * 10f);

        animator.SetFloat("Vertical", currentVertical);
        animator.SetFloat("Horizontal", currentHorizontal);
    }

    public void IsSlow(bool isSlow)
    {
        _isSlow = isSlow;
    }

    public void IsStop(bool isStop)
    {
        _isStop = isStop;
        if (isStop)
        {
            animator.SetFloat("Vertical", 0f);
            animator.SetFloat("Horizontal", 0f);
        }
    }

    public void LookAround()
    {
        float cameraYRotation = CameraControl.Inst.POV.m_HorizontalAxis.Value;

        // 캐릭터의 y축 회전값을 카메라와 동일하게 설정
        transform.rotation = Quaternion.Euler(0, cameraYRotation, 0);
    }

    #endregion

    #region 공격 관련
    private void Attack()
    {
        if (_soundComponent != null) {
            _soundComponent.PlaySound(PlayerSoundType.Attack);
        }
        playerClass.Attack();
        animator.SetTrigger(doAttack);
        photonView.RPC("PullTheTrigger", RpcTarget.Others, doAttack);

        isAttack = true;

        // 차징 중 공격 시
        if (isCharging)
        {
            CancelCharging();
        }

        Invoke(nameof(ReadyAttack), characterInfo.GetAttackDuration());
    }
    private void ReadyAttack()
    {
        isAttack = false;
    }

    private void Charging()
    {
        if (!isCharging && characterInfo is Archer archer)
        {
            archer.StartCharging();
            IsSlow(true);
            animator.SetTrigger("doCharging");
            animator.SetFloat("ChargingRate", 0.5f);
            isCharging = true;
        }
    }
    public void CancelCharging()
    {
        if (characterInfo is Archer archer)
        {
            archer.CancelCharging();
        }

        // 애니메이션 부드럽게
        float currentFloat = animator.GetFloat("ChargingRate");
        float targetFloat = 0;
        currentFloat = Mathf.Lerp(currentFloat, targetFloat, Time.deltaTime * 10f);
        animator.SetFloat("ChargingRate", currentFloat);

        // 원래 속도로 변경
        IsSlow(false);
        // 차징 상태 해제
        isCharging = false;
    }

    private void Skill()
    {
        if (_soundComponent != null) 
            _soundComponent.PlaySound(PlayerSoundType.Skill);
        playerClass.Skill();
        animator.SetTrigger(doSkill);
        photonView.RPC("PullTheTrigger", RpcTarget.Others, doSkill);
        //isSkill = true;
        //Invoke(nameof(ReadySkill), characterInfo.GetSkillDuration());

        // 쿨타임 시작
        if (skillCooldownCoroutine != null)
            StopCoroutine(skillCooldownCoroutine); // 기존 쿨타임이 있다면 중지
        skillCooldownCoroutine = StartCoroutine(SkillCooldown(skillCooldownDuration));

    }

    private void Ultimate()
    {
        if (_soundComponent != null) {
            _soundComponent.PlaySound(PlayerSoundType.Ultimate);
        }
        playerClass.Ultimate();
        animator.SetTrigger(doUltimate);
        photonView.RPC("PullTheTrigger", RpcTarget.Others, doUltimate);
        isUltimate = true;
        //Invoke(nameof(ReadyUltimate), characterInfo.GetUltimateDuration());
        // 쿨타임 시작
        if (ultimateCooldownCoroutine != null)
            StopCoroutine(ultimateCooldownCoroutine); // 기존 쿨타임이 있다면 중지
        ultimateCooldownCoroutine = StartCoroutine(UltimateCooldown(ultimateCooldownDuration));
    }

    // 스킬 쿨타임 관리 코루틴
    private IEnumerator SkillCooldown(float duration)
    {
        isSkill = true;
        skillRemainingCooldown = duration;

        while (skillRemainingCooldown > 0f)
        {
            skillRemainingCooldown -= Time.deltaTime;
            yield return null;
        }

        isSkill = false; // 쿨타임이 완료되면 스킬 준비 완료
    }

    // 궁극기 쿨타임 관리 코루틴
    private IEnumerator UltimateCooldown(float duration)
    {
        isUltimate = true;
        ultimateRemainingCooldown = duration;

        while (ultimateRemainingCooldown > 0f)
        {
            ultimateRemainingCooldown -= Time.deltaTime;
            yield return null;
        }

        isUltimate = false; // 쿨타임이 완료되면 스킬 준비 완료
    }

    public void SetCooldownDuration(float duration, AttackType type)
    {
        switch (type)
        {
            case AttackType.Attack:
                attackCooldownDuration = duration;
                break;
            case AttackType.Skill:
                skillCooldownDuration = duration;
                break;
            case AttackType.Ultimate:
                ultimateCooldownDuration = duration;
                break;
        }
    }
    #endregion

    // #region UI 관련
    // public bool ToggleSkillTreeUI()
    // {
    //     isSkillTreeUIActive = !isSkillTreeUIActive;
    //     return isSkillTreeUIActive;
    // }

    // #endregion

    #region 방어력 증가 관련
    public void SetIncreaseDefensePowerAmount(float amount)
    {
        defenseBuffAmount = amount;
    }

    public void IncreaseDefensePower()
    {
        if (photonView == null ||  defenseBuffAmount == 0f)
        {
            return;
        }

        if (photonView.IsMine)
        {
            photonView.RPC("IncreaseDefensePower", RpcTarget.Others);
        }

        characterInfo.SetDefencePower(characterInfo.GetDefencePower() + defenseBuffAmount);

        //Debug.Log($"적 처치 시 전사의 방어력이 증가합니다. 현재 공격력 : {characterInfo.GetDefencePower()}");
    }
    #endregion

    #region 쿨타임 감소 관련
    public void SetCooldownReductionAmount(float amount)
    {
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }
        cooldownReductionAmount = amount;
    }
    public void ReduceCooldownOnKill()
    {
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }
        //Debug.Log("구독 했던 쿨감 메서드 실행");
        if (isSkill)
        {
            skillRemainingCooldown -= cooldownReductionAmount;
            if (skillRemainingCooldown <= 0f)
            {
                skillRemainingCooldown = 0f;
                isSkill = false;

                if (skillCooldownCoroutine != null)
                    StopCoroutine(skillCooldownCoroutine);
            }
        }
    }

    // 궁극기 쿨다운
    public void SetUltimateCooldownReductionAmount(float amount)
    {
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }
        ultimateCooldownReductionAmount = amount;
    }
    public void ReduceUltimateCooldownOnKill()
    {
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }
        //Debug.Log("궁극기 쿨감 메서드 실행");
        if (isUltimate)
        {
            ultimateRemainingCooldown -= ultimateCooldownReductionAmount;
            if (ultimateRemainingCooldown <= 0f)
            {
                ultimateRemainingCooldown = 0f;
                isUltimate = false;

                if (ultimateCooldownCoroutine != null)
                    StopCoroutine(ultimateCooldownCoroutine);
            }
        }
    }

    [PunRPC]
    public void SetAttackDamage(float damage)
    {
        if (photonView == null)
        {
            return;
        }
        if (photonView.IsMine)
        {
            Debug.Log($"데미지가 올라감을 전송");
            photonView.RPC("SetAttackDamage", RpcTarget.Others, damage);
        }

        attackDamageAmount = damage;
    }

    [PunRPC]
    public void ApplyAttackDamageUp()
    {
        Debug.Log($"attackDamageUpAmount:{attackDamageAmount}");
        if (photonView == null || attackDamageAmount == 0)
        {
            return;
        }

        if (photonView.IsMine)
        {
            Debug.Log($"데미지가 올라감을 전송");
            photonView.RPC("ApplyAttackDamageUp", RpcTarget.Others);
        }

        Debug.Log($"현재 데미지는{characterInfo.GetAttackDamage()}");
        characterInfo.SetAttackDamage(characterInfo.GetAttackDamage() + attackDamageAmount);

        Debug.Log($"현재 데미지는{characterInfo.GetAttackDamage()}");
    }

    public void SubscribeToMonsterDeath(MonsterBase monster)
    {
         Debug.Log("몬스터가 생성되어 구독");
        // 새 몬스터의 사망 이벤트 구독
        monster.OnDeath += ReduceCooldownOnKill;
        monster.OnDeath += ReduceUltimateCooldownOnKill;
        monster.OnDeath += ApplyAttackDamageUp;
        monster.OnDeath += IncreaseDefensePower;
    }
    #endregion

    #region 애니메이션 브로드캐스트
    private const string doAttack = "doAttack";
    private const string doSkill = "doSkill";
    private const string doUltimate = "doUltimate";

    [PunRPC]
    private void PullTheTrigger(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    #endregion

    #region 버프관련
    // amount가 변경되지 않는다는 전제 하에 작성된 코드
    public void IncreaseAttackDamage(float amount, float duration)
    {
        float original = characterInfo.GetAttackDamage();

        if (attackDamageBuffCoroutine != null)
        {
            StopCoroutine(attackDamageBuffCoroutine); // 기존 쿨타임이 있다면 중지
        }
        else
        {
            // 처음 버프를 설정하거나 버프가 시간 초과로 종료된 후 버프 설정일 때
            characterInfo.SetAttackDamage(original + amount);
        }

        attackDamageBuffCoroutine = StartCoroutine(AttackDamageBuff(amount, duration));
    }

    private IEnumerator AttackDamageBuff(float amount, float duration)
    {
        while (duration > 0f)
        {
            duration -= Time.deltaTime;
            yield return null;
        }

        characterInfo.SetAttackDamage(characterInfo.GetAttackDamage() - amount);
    }

    #endregion

}
