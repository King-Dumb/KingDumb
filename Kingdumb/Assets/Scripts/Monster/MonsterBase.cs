using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;
using System;
using static MonsterSoundEntry;
using Photon.Realtime;

public abstract class MonsterBase : MonoBehaviourPun, IMonster
{
    // 인게임 내에서 바뀌지 않는 값들
    public MonsterInfo monsterInfo;
    protected NavMeshAgent _navMeshAgent; // 경로 계산 AI 에이전트
    protected Animator _animator; // 애니메이터 컴포넌트
    protected MonsterUI _ui;
    protected CapsuleCollider _collider;
    protected Rigidbody _rb;

    protected int _viewId;
    [SerializeField] protected float attackAnimationLength;

    public enum MonsterState
    {
        Moving,   // 타겟으로 이동하는 상태
        AttackReady,  // 다음 공격을 위해 대기중인 상태 (공격 사거리 안에 있는 상태)
        Attacking, // 타겟 방향으로 공격을 하는 상태
        Dead // 죽은 상태
    }

    // 오브젝트에 종속된 몬스터 정보
    protected float _currHp; // 현재 hp

    protected float _maxHp; // 맥스hp

    protected float _baseMoveSpeed;
    protected float _currMoveSpeed
    {
        get
        {
            if (moveSpeedDebuffList.Count > 0)
            {
                float minValue = float.MaxValue;
                foreach (float debuff in moveSpeedDebuffList)
                {
                    minValue = debuff < minValue ? debuff:minValue;
                }
                return minValue * _baseMoveSpeed;
            }
            return _baseMoveSpeed;
        }
    }

    protected float _currAttackRange; // 현재 공격 사거리

    protected float _currOutOfAttackRange; // 현재 공격 벗어나기위한 사거리

    public virtual Vector3 Position => transform.position;

    // 타겟, state
    [SerializeField] protected GameObject _target;
    [SerializeField] protected GameObject _defaultTarget;
    
    protected float _monsterRadius; 
    protected float _targetRadius; // surfaceDistance를 구하기 위함
    private GameObject _nexus;

    // 아래는 몬스터 스크립트가 아닌 각 객체 또는 Manager로부터 가져올 수 있으면 좋을 것 같긴 함
    
    protected MonsterState _state; // moving으로 시작

    protected float _lastAttackTime = 0; // 마지막 공격 시간
    protected float _lastNavTime = 0; // 마지막 Nav 계산 시간

    private const float NAV_STEP = 0.25f;

    //슬로우 구현
    private List<float> moveSpeedDebuffList;

    // 몬스터가 처치되었을 시 적용될 delegate
    public event Action OnDeath;

    private Coroutine SetTargetCoroutine;

    public float playerTargetChance = 0.3f;
 
    protected virtual void Awake()
    {
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _ui = GetComponent<MonsterUI>();
        _navMeshAgent.enabled = true;
        _monsterRadius = GetRadius(gameObject);
        OnDeath += GrandRewardToPlayers;
        _soundComponent = GetComponent<MonsterSoundComponent>();
        _collider = GetComponent<CapsuleCollider>();
        _rb = GetComponent<Rigidbody>();
        _maxHp = monsterInfo.hp;

        SubscribePlayerControllerMethod();
        _currAttackRange = monsterInfo.attackRange;
        _currOutOfAttackRange = monsterInfo.outOfAttackRange;
        _baseMoveSpeed = monsterInfo.moveSpeed;
    }

    protected virtual void Start()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
        OnSpawn();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        switch (_state)
        {
            case MonsterState.Moving:
                MoveToTarget();

                break;
            case MonsterState.AttackReady:
                DoAttackReady();
                break;
            default:
                return;
        }

        // 타겟 찾기 로직 보강
        FindTarget();
    }

    public void Initialize()
    {
        _currHp = _maxHp;
        _ui.UpdateHpBar(_currHp/_maxHp);

        _nexus = MonsterGenerator.Inst?.GetTarget();
        // _nexus = MonsterGeneratorForTest.Instance?.target;

        _state = MonsterState.Moving;
        moveSpeedDebuffList = new();
        ResetTarget();
        
        _viewId = GetComponent<PhotonView>().ViewID;
        _collider.enabled = true;

        _defaultTarget = _nexus;
        if(!TryGetComponent<GolemSkeleton>(out GolemSkeleton boss)) // 보스는 항상 넥서스가 기본 목표
        {
            SetRandomDefaultTarget();
        }
        //_animator = GetComponent<Animator>();
    }

    public void SetRandomDefaultTarget()
    {
        float randomValue = UnityEngine.Random.Range(0f, 1f);
        if (randomValue <= playerTargetChance)
        {
            Dictionary<int, GameObject> dict = CharacterManager.Inst.GetPlayerDictionary();
            int playerIdx = UnityEngine.Random.Range(0, dict.Count);
            foreach(GameObject player in dict.Values)
            {   
                if (playerIdx-- == 0) 
                {
                    _defaultTarget = player;
                    _targetRadius = GetRadius(_defaultTarget);
                    // //Debug.Log($"targetRadius: {_targetRadius}");
                    return;
                }
            }
        }
    }
    /*
        넥서스로 타겟 전환 -> 디폴트 타겟으로 타겟 전환
    */
    public void ResetTarget()
    {
        _target = _defaultTarget;
        _targetRadius = GetRadius(_defaultTarget);
        // //Debug.Log($"targetRadius: {_targetRadius}");
        // if (_nexus != null && _nexus.activeSelf)
        // {
        //     // //Debug.Log($"몬스터 {monsterInfo.monsterName}의 타겟 {_target} -> {_nexus}");
        //     _target = _nexus;
        //     _targetRadius = 0.5f;
        // }
        // else
        // {
        //     _nexus = GameObject.FindWithTag("Nexus");
        //     _target = _nexus;
        //     _targetRadius = 0.5f;
        // }
    }

    /*
        duration 동안 player로 target 전환
    */
    public IEnumerator SetTarget(GameObject target, float targetDuration)
    {
        // //Debug.Log($"몬스터 {monsterInfo.monsterName}의 타겟 {_target} -> {target}");
        _target = target;
        _targetRadius = GetRadius(_target);
        // //Debug.Log($"targetRadius: {_targetRadius}");
        yield return new WaitForSeconds(targetDuration);
        ResetTarget();
    }

    public void FindTarget()
    {
        // 넥서스 리셋 로직
        if (_nexus != null && !_nexus.activeSelf)
        {
            _nexus = GameObject.FindWithTag("Nexus");
        }
        // 디폴트 타겟 해제 로직
        if ((_defaultTarget?.GetComponent<IDamageable>()?.IsDead() ?? true) || !_defaultTarget.activeSelf)
        {
            _defaultTarget = _nexus;
        }
        // 타겟 리셋 로직 - 타겟이 널이거나 죽어있거나 IDamageable이 없는경우 
        if ((_target?.GetComponent<IDamageable>()?.IsDead() ?? true) || !_target.activeSelf)
        {
            ResetTarget();
        }
    }

    protected virtual void MoveToTarget()
    {
        _animator.SetBool("IsMoving", true);
        // 목표와의 거리
        float distance = GetSurfaceDistance(_target);

        // //Debug.Log($"distance: {distance}");

        // 일정 시간마다 계산 실행, 불필요한 계산 줄이기
        if (_target != null && _navMeshAgent.enabled && Time.time >= _lastNavTime + NAV_STEP)
        {
            _lastNavTime = Time.time;
            _navMeshAgent.SetDestination(_target.transform.position); // 계산
        }

        _navMeshAgent.speed = _currMoveSpeed;

        // 공격 범위 안에 들어오면 공격 상태로 전환
        if (distance <= _currAttackRange)
        {
            ChangeState(MonsterState.AttackReady);
        }

        // 타겟이 범위를 벗어난 경우 넥서스로 타겟 전환
        if (_target != null && _target.CompareTag("Player") && distance > monsterInfo.outOfDetectionRange)
        {
            ResetTarget();
        }
    }

    private void DoAttackReady()
    {
        _animator.SetBool("IsMoving", false);

        // 목표와의 거리 
        float distance = GetSurfaceDistance(_target);

        float angle = GetAngleToTarget();

        // //Debug.Log($"distance: {distance}, angle: {angle}");

        // 범위에서 벗어나면 이동으로 변경
        if (distance > _currOutOfAttackRange)
        {
            ChangeState(MonsterState.Moving);
            return;
        }

        LookAtTarget(); // 타겟을 바라봄
        // 각 클래스에서 구현한 공격 메서드 실행, 공격중 이동 비활성화
        // 바라보고있을때에만 공격 실행
        if (Time.time >= _lastAttackTime + monsterInfo.attackDuration && angle < 5f)
        {
            _lastAttackTime = Time.time;
            // //Debug.Log($"몬스터 {monsterInfo.monsterName}의 타겟과의 거리 : {distance}, 실제거리: {(_target.transform.position - transform.position).magnitude}");
            Attack();
        }
    }

    /*
        공격 메서드는 각 몬스터 클래스에서 직접 구현
    */
    public abstract void Attack();

    /* 
        damageAmount 만큼 체력을 깎고 사망 처리한다.
        TODO: 데미지를 입는 이펙트, 애니메이션, 사운드, 물리적인 효과 추가
    */

    [PunRPC]
    public void ApplyUpdatedHealth(float newHealth, float damage)
    {
        _currHp = newHealth;
        _ui.UpdateHpBar(_currHp / _maxHp);
        _ui.DamagePopup(damage);
    }


    [PunRPC]
    public void OnDamage(float damage, bool isMagic, Vector3 hitPoint, int sourceViewID)
    {
        if (_state == MonsterState.Dead) return;

        //_animator.SetTrigger("Hit");

        if (PhotonNetwork.IsMasterClient)
        {
            // 데미지 계산 로직
            if (!isMagic)
            {
                // 최종 데미지 = (데미지 - 방어력) 또는 1
                damage = math.max(damage - monsterInfo.defencePower, 1);
            }
            // 체력을 깎는다.
            _currHp -= damage;
            _ui.UpdateHpBar(_currHp / _maxHp);
            _ui.DamagePopup(damage);
            // TODO: 가능하다면 로컬에서 기록 후 필요할 때(씬 전환, 게임 승리, 종료 시에)만 업데이트 되도록 해 부하를 줄이기
            // CHECK: 플레이어가 준 피해량을 기록하는 곳
            PlayerStatisticsManager.Instance.RecordDealtDamage(sourceViewID, damage);
            PhotonView sourcePhotonView = PhotonView.Find(sourceViewID);
            GameObject sourcePlayer = sourcePhotonView.gameObject;
            // GameObject sourcePlayer = CharacterManager.Inst?.GetPlayerDictionary()[sourceViewID];
            //if (sourcePlayer?.tag == "Player" && sourcePhotonView?.Owner is Player player)
            //{
            //    float totalDealtDamage = PhotonManager.GetPlayerCustomProperty<float>("DealtDamage", player);
            //    totalDealtDamage += damage;
            //    PhotonManager.SetPlayerCustomProperty<float>(totalDealtDamage, "DealtDamage", player);
            //}

            float distance = GetSurfaceDistance(sourcePlayer); // 공격 대상과의 거리

            // 현재 타겟이 Player가 아니고, damageSource가 Player일 때 hp가 임계값 이하이고
            // detectionRange 안에 있을 경우 Player의 타겟을 변경
            // 만약 공격을 무시하고 넥서스만 타겟하는 몬스터를 만들고 싶다면 detectionRange 또는 agroTriggerHp를 줄이는 방향
            ////Debug.Log($"{monsterInfo.monsterName} - damageSource : {sourcePlayer}, distance: {distance}" );
            if (sourcePlayer?.tag == "Player" &&
                    _currHp <= monsterInfo.aggroTriggerHp &&
                    distance <= monsterInfo.detectionRange)
            {
                if (_target.CompareTag("Nexus"))
                {
                    SetTargetCoroutine = StartCoroutine(SetTarget(sourcePlayer, monsterInfo.aggroDuration));
                }
                else if (_target == sourcePlayer && SetTargetCoroutine != null)
                {
                    StopCoroutine(SetTargetCoroutine); // 현재 코루틴을 멈추고
                    SetTargetCoroutine = StartCoroutine(SetTarget(sourcePlayer, monsterInfo.aggroDuration)); // 지속시간 갱신
                }
            }
            // 호스트에서 클라이언트로 동기화
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, _currHp, damage);
            photonView.RPC("PlayHitAnimationAndSound", RpcTarget.All);
            photonView.RPC("OnDamage", RpcTarget.Others, damage, isMagic, hitPoint, sourceViewID);
        }
        ////Debug.Log($"{monsterInfo.monsterName}이(가) {damage}의 피해를 입었습니다. 남은 체력: {_currHp}");

        if (_currHp <= 0 && _state != MonsterState.Dead)
        {
            Die();
        }
    }

    [PunRPC]
    public void PlayHitAnimationAndSound()
    {
        if (_currHp <= 0) return;
        ////Debug.Log("Play Hit Animation RPC 호출됨"); // 호출 여부 확인
        // 3D 오디오 설정이 되어 있는 AudioSource에서 소리 재생
        if (_animator != null)
        {
            _animator.SetTrigger("Hit");
        }
        OnHit();
    }



    public void Die()
    {
        _animator.SetBool("IsMoving", false);
        _animator.SetTrigger("Dead");
        
        _collider.enabled = false;
        foreach(Collider collider in GetComponentsInChildren<Collider>())
        {
            collider.enabled = false;
        }
        ChangeState(MonsterState.Dead);
        ////Debug.Log($"몬스터 {monsterInfo.monsterName}이(가) 사망했습니다.");
        // 테스트를 위한 임시 코드 추가
            if (MonsterGenerator.Inst != null)
            {
                MonsterGenerator.Inst.RemoveMonster(gameObject);
            }
        if (PhotonNetwork.IsMasterClient)
        { 
            Invoke("DestroySelf", 5);
        }
        _soundComponent.PlaySound(MonsterSoundType.Death);
        ////Debug.Log("사망 체크 포인트2");
        OnDeath?.Invoke();

        ////Debug.Log("사망 체크 포인트3");
    }

    public void DestroySelf()
    {
        if (photonView != null && PhotonNetwork.IsMasterClient)
        {
            ////Debug.Log("사망 체크 포인트4");
            PhotonNetwork.Destroy(gameObject);
        }
        else if (photonView == null) 
        {
            Destroy(gameObject);
        }
    }

    public void ChangeState(MonsterState newState)
    {
        // 이동중이 아닐 때에는 멈춘다.
        if (_navMeshAgent.enabled)
        {
            _navMeshAgent.isStopped = newState != MonsterState.Moving;
            _navMeshAgent.velocity = Vector3.zero;
        }

        if (_state == MonsterState.Dead) return; // 죽고나서는 상태 변환 안함

        // //Debug.Log($"몬스터 {monsterInfo.monsterName}의 상태 {_state} -> {newState}");
        _state = newState;
    }

    public float GetSurfaceDistance(GameObject target)
    {  
        if (target == null) return -1;
        Vector3 direction = target.transform.position - transform.position; // 거리벡터
        direction.y = 0;
        return direction.magnitude - (_monsterRadius + _targetRadius);
    }

    public float GetRadius(GameObject target)
    {   
        if (target == null) return 0;
        CapsuleCollider cCollider;
        if (target.TryGetComponent<CapsuleCollider>(out cCollider))
        {
            return cCollider.radius * cCollider.transform.lossyScale.x;
        }
        SphereCollider sCollider;
        if (target.TryGetComponent<SphereCollider>(out sCollider))
        {
            return sCollider.radius * sCollider.transform.lossyScale.x;
        }
        //Debug.LogWarning($"{target}가 콜라이더가 없어서 거리 측정에 문제가 있을 수 있음");
        return 0;
    }

    public void LookAtTarget()
    {
        if (_target == null) return;
        Vector3 direction = _target.transform.position - transform.position;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, monsterInfo.rotationSpeed * Time.deltaTime);
        }
    }

    public float GetAngleToTarget()
    {
        if (_target == null) return 9999f;
        Vector3 direction = _target.transform.position - transform.position;
        direction.y = 0;
        return Vector3.Angle(transform.forward, direction);
    }

    public void DebuffSlow(float slowRate, float duration)
    {
        // 슬로우를 마스터에서 제어하도록 변경
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(ApplyDebuffSlow(slowRate, duration));
        }
    }

    public IEnumerator ApplyDebuffSlow(float slowRate, float duration)
    {
        float moveSpeedDebuff = math.max((1f - slowRate), 0f);
        moveSpeedDebuffList.Add(moveSpeedDebuff);
        int count = moveSpeedDebuffList.Count;
        photonView.RPC("ApplyUpdatedSlowUI", RpcTarget.All, count);
        //_ui.UpdateSlowEffect(moveSpeedDebuffList.Count);
        yield return new WaitForSeconds(duration);
        moveSpeedDebuffList.Remove(moveSpeedDebuff);
        count = moveSpeedDebuffList.Count;
        photonView.RPC("ApplyUpdatedSlowUI", RpcTarget.All, count);
        //_ui.UpdateSlowEffect(moveSpeedDebuffList.Count);
    }

    [PunRPC]
    public void ApplyUpdatedSlowUI(int debuffCount)
    {
        _ui.UpdateSlowEffect(debuffCount);
    }

    private bool _isKnockbackActive = false;
    private Vector3 knockbackTargetPos; // 넉백 위치
    private float knockbackTime; // 넉백 경과 시간
    public void Knockback(Vector3 force)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ApplyKnockback(force);
        }
    }

    void FixedUpdate()
    {
        if (_isKnockbackActive)
        {
            // 경과 시간 증가
            knockbackTime += Time.fixedDeltaTime;

            // 넉백 진행 비율 계산  ;

            // 위치를 선형 보간하여 부드럽게 이동
            Vector3 newPosition = Vector3.Lerp(
                _rb.position, 
                knockbackTargetPos, 
                knockbackTime);

            _rb.MovePosition(newPosition);

            // 넉백 완료 시 상태 초기화
            if (knockbackTime >= 1f)
            {
                _isKnockbackActive = false;
                if (_navMeshAgent != null) _navMeshAgent.enabled = true; // NavMeshAgent 다시 활성화
            }
        }
    }

    public void ApplyKnockback(Vector3 force)
    {
        if (_state == MonsterState.Dead || _rb == null) return;
        if(TryGetComponent<GolemSkeleton>(out GolemSkeleton boss)) return; // 보스는 넉백 안됨
        // 초기값 설정
        force.y = 0;
        knockbackTargetPos = _rb.position+(force/_rb.mass);
        knockbackTime = 0f;
        _isKnockbackActive = true;
        _navMeshAgent.enabled = false;
    }
    
    
    public bool IsDead()
    {
        return _state == MonsterState.Dead;
    }

    // 애니메이션 관련 ------------------------------------------------------------------------------
    public void MonsterAnimationBroadcast(string triggerName)
    {
        photonView.RPC("PlayMonsterAnimation", RpcTarget.All, triggerName);
    }

    [PunRPC]
    public void PlayMonsterAnimation(string triggerName)
    {
        _animator.SetTrigger(triggerName);
    }


    // 몬스터 사망 시 ------------------------------------------------------------------------------------------
    public void GrandRewardToPlayers()
    {
        ////Debug.Log($"플레이어들에게 경험치:{monsterInfo.expReward}와 골드:{monsterInfo.goldReward}를 제공");
        if (PhotonNetwork.IsMasterClient)
        {
            IngameManager.Inst.AddGoldAndExp(monsterInfo.goldReward, monsterInfo.expReward);
        }
    }

    public void SubscribePlayerControllerMethod()
    {
        GameObject player = GameManager.Instance.localPlayer;
        player.GetComponent<PlayerController>().SubscribeToMonsterDeath(this);
    }

    // 오디오 관련 ------------------------------------------------------------------------------
    private MonsterSoundComponent _soundComponent;

    public void OnSpawn()
    {
        _soundComponent.PlaySound(MonsterSoundType.Spawn);
    }

    public void OnHit()
    {
        _soundComponent.PlaySound(MonsterSoundType.Hit);
    }
}

