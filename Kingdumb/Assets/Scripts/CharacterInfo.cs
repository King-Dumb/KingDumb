using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static PlayerSoundEntry;
public abstract class CharacterInfo : MonoBehaviourPun, IDamageable
{
    public Vector3 Position => transform.position;
    protected string _nickname;
    protected string _classType;

    //protected PhotonView 포톤 뷰 ID

    protected float _hp; //체력
    protected bool _dead = false; // 사망 여부
    protected float _maxHp; // 체력 최대치
    protected float _baseMoveSpeed; // 기준 이동속도
    protected float _moveSpeed; // 이동속도
    protected float _runningSpeed; // 달리기 속도
    protected float _baseAttackDamage; // 기준 공격력
    protected float _attackDamage; // 공격력
    protected float _defencePower; // 방어력
    protected float _attackDuration; // 공격 속도
    protected float _skillDuration; // 스킬 쿨타임
    protected float _ultimateDuration; // 궁 쿨타임

    protected float _reviveTime; // 부활 시간

    protected int _level; // 레벨
    protected int _skillPoint; // 스킬포인트
    public bool[] savedSkillNode = new bool[16]; // 찍은 스킬 저장

    protected int _exp; // 경험치
    protected int _gold; // 골드

    public Slider worldHpBar; // 월드상에서 보이는 hp bar

    protected Animator _animator; // 플레이어의 애니메이터
    protected DamageOutline damageOutline; // 데미지 받았을 경우 외곽선 표시 
    protected PlayerController _playerController; // 플레이어 컨트롤러
    //protected AudioSource _playerAudioPlayer; // 플레이어 소리 재생기
    protected readonly string[] targetNames = { "Mesh", "PlayerNickName", "Root_M", "PlayerNickname" };
    protected int _ownerPhotonViewID; // 플레이어의 포톤뷰 ID

    private PlayerWithNexus _playerWithNexus;
    private const string LevelUpEffectName = "LevelUpEffectGroup";

    private PlayerSoundComponent _soundComponent; // 플레이어가 발생시키는 소리를 관리하는 컴포넌트

    public CharacterInfo()
    {

    }

    protected void InitializeComponents()
    {
        _animator = GetComponent<Animator>();
        _playerController = GetComponent<PlayerController>();
        _playerWithNexus = GetComponent<PlayerWithNexus>();
        //_playerAudioPlayer = GetComponent<AudioSource>();
        damageOutline = GetComponent<DamageOutline>();
        damageOutline.enabled = false;

        if (photonView != null)
        {
            _ownerPhotonViewID = photonView.ViewID;
        }

        _playerController.SetCooldownDuration(_attackDuration, PlayerController.AttackType.Attack);
        _playerController.SetCooldownDuration(_skillDuration, PlayerController.AttackType.Skill);
        _playerController.SetCooldownDuration(_ultimateDuration, PlayerController.AttackType.Ultimate);

        string sceneName = SceneManager.GetActiveScene().name;
        // Debug.Log(sceneName);
        if (sceneName == "InGame" || sceneName == "InGame_2S" || sceneName == "InGame_3S")
        {
            //StartCoroutine(CheckNexusState());
            ActivePlayerWithNexusAndTower();
            //photonView.RPC("ActivePlayerWithNexusAndTower", RpcTarget.All);
        }

        //마스터 클라이언트에게 플레이어가 생성되었다고 알림
        if (photonView != null && photonView.IsMine)
        {
            photonView.RPC("NotifyMasterClient", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer.ActorNumber);

            worldHpBar?.gameObject.SetActive(false); // hpbar는 자기자신은 보이지않음
        }
    }
    private void Awake()
    {
        // IngameManager의 레벨 업 이벤트를 구독
        if (IngameManager.Inst != null)
        {
            IngameManager.Inst.OnLevelUpEvent += HandleLevelUp;
        }
        _soundComponent = GetComponent<PlayerSoundComponent>();
    }

    private void OnEnable()
    {
        _hp = _maxHp;
        _reviveTime = 8f;
    }

    public string GetNickname() { return _nickname; }
    public void SetNickname(string nickname) { _nickname = nickname; }

    public string GetClassType() { return _classType; }
    public void SetClassType(string classType) { _classType = classType; }

    //public PhotonView GetPhotonView() { return _photonView}
    //public void SetPhotonView(PhotonView photonView) { _photonView = photonView};

    public float GetHP() { return _hp; }
    public void SetHP(float hp) { _hp = hp; }
    public float GetMaxHP() { return _maxHp; }
    public void SetMaxHP(float maxHp)
    {
        _maxHp = maxHp;
        if (_hp > _maxHp)
        {
            _hp = _maxHp;
        }
    }

    public float GetBaseMoveSpeed() { return _baseMoveSpeed; }
    public void SetBaseMoveSpeed(float baseMoveSpeed) { _baseMoveSpeed = baseMoveSpeed; }
    public float GetMoveSpeed() { return _moveSpeed; }
    public void SetMoveSpeed(float moveSpeed) { _moveSpeed = moveSpeed; }
    public float GetRunningSpeed() { return _runningSpeed; }
    public void SetRunningSpeed(float runningSpeed) { _runningSpeed = runningSpeed; }

    public float GetBaseAttackDamage() { return _baseAttackDamage; }
    public void SetBaseAttackDamage(float baseAttackDamage) { _baseAttackDamage = baseAttackDamage; }
    public float GetAttackDamage() { return _attackDamage; }
    public void SetAttackDamage(float attackDamage) { _attackDamage = attackDamage; }

    public float GetDefencePower() { return _defencePower; }
    public void SetDefencePower(float defencePower) { _defencePower = defencePower; }

    public float GetAttackDuration() { return _attackDuration; }
    public void SetAttackDuration(float attackDuration) { _attackDuration = attackDuration; }

    public float GetSkillDuration() { return _skillDuration; }
    public void SetSkillDuration(float skillDuration) { _skillDuration = skillDuration; }
    public void SetUltimateDuration(float ultimateDuration) { _ultimateDuration = ultimateDuration; }

    public float GetUltimateDuration() { return _ultimateDuration; }

    public int GetLevel() { return _level; }
    public void SetLevel(int level) { _level = level; }

    public int GetSkillPoint() { return _skillPoint; }
    public void SetSkillPoint(int skillPoint) { _skillPoint = skillPoint; }

    public int GetExp() { return _exp; }
    public void SetExp(int exp) { _exp = exp; }

    public float GetReviveTime() { return _reviveTime; }
    public void SetReviveTime(float reviveTime) { _reviveTime = reviveTime; }

    [PunRPC]
    public void ApplyUpdatedHealth(float newHp, bool newDead)
    {
        _hp = newHp < 0 ? 0 : newHp;
        _dead = newDead;
        worldHpBar.value = newHp / _maxHp;
    }

    // 로컬 테스트용 OnDamage(sourceViewID가 필요하지 않음)
    private void OnDamageLocal(bool isMagic, float damage, Vector3 hitPoint)
    {
        if (!isMagic)
        {
            // 마법 공격이 아닌 경우 방어력 적용
            damage -= _defencePower;
        }

        _hp -= damage;
        PlayHitSound();

        // 체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
        if (_hp <= 0 && !_dead)
        {
            Die();
        }
    }

    // 실제 사용 OnDamage(sourceViewID가 필요함)

    [PunRPC]
    public virtual void OnDamage(float damage, bool isMagic, Vector3 hitPoint, int sourceViewID)
    {
        //Debug.Log($"데미지{damage}발생 {sourceViewID}가 공격함");
        // photonView가 없으면(로컬) 로컬 데미지 처리 
        if (photonView == null)
        {
            OnDamageLocal(isMagic, damage, hitPoint);
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            if (!isMagic)
            {
                // 마법 공격이 아닌 경우 방어력 적용
                damage -= _defencePower;
            }

            // 데미지만큼 체력 감소
            // _hp -= damage < 1 ?  1 : damage;
            float newDamage = damage < 1 ? 1 : damage;
            _hp -= newDamage;
            // CHECK: 플레이어가 받은 데미지를 기록하는 곳
            PlayerStatisticsManager.Instance.RecordTakenDamage(_ownerPhotonViewID, newDamage);
            //float totalTakenDamage = PhotonManager.GetPlayerCustomProperty<float>("TakenDamage", photonView.Owner);
            //totalTakenDamage += newDamage;
            //PhotonManager.SetPlayerCustomProperty<float>(totalTakenDamage, "TakenDamage", photonView.Owner);

            // 체력이 0 미만이면 0으로 설정
            _hp = _hp < 0 ? 0 : _hp;
            worldHpBar.value = _hp / _maxHp; // hp bar update

            // 궁수일 경우 차징 캔슬
            if (_classType == "Archer")
            {
                _playerController.CancelCharging();
            }

            // 호스트에서 클라이언트로 동기화
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, _hp, _dead);

            // 다른 클라이언트들도 OnDamage를 실행하도록 함
            photonView.RPC("OnDamage", RpcTarget.Others, damage, isMagic, hitPoint, sourceViewID);
            photonView.RPC("PlayHitAnimation", RpcTarget.All);
            photonView.RPC("PlayHitSound", RpcTarget.All);
        }
        // 모든 클라이언트에서 히트 소리 재생

        // 체력이 0 이하 && 아직 죽지 않았다면 사망 처리 실행
        if (_hp <= 0 && !_dead)
        {
            Die();
        }
        else
        {
            // 데미지 받았을 때 외곽선 표시
            StartCoroutine(OnDamageLine());
        }
    }

    private IEnumerator OnDamageLine()
    {
        damageOutline.enabled = true;
        yield return new WaitForSeconds(0.2f);
        damageOutline.enabled = false;
    }

    // 로컬 테스트용 RestoreHealth(sourceViewID가 필요하지 않음)
    private void RestoreHealthLocal(float healAmount, Vector3 hitPoint)
    {
        if (_dead) return;
        // Debug.Log($"체력 회복 {healAmount}발생");

        _hp += healAmount;

        // 최대 체력 이상으로 회복 시 최대 체력으로 초기화
        if (_hp > _maxHp)
        {
            _hp = _maxHp;
        }
    }

    [PunRPC]
    public virtual void RestoreHealth(float healAmount, Vector3 hitPoint, int sourceViewID)
    {
        // photonView가 없으면(로컬) 로컬 데미지 처리 
        if (photonView == null)
        {
            RestoreHealthLocal(healAmount, hitPoint);
            return;
        }

        if (_dead) return;

        if (PhotonNetwork.IsMasterClient)
        {
            // 데미지만큼 체력 감소
            healAmount = healAmount > _maxHp - _hp ? _maxHp - _hp : healAmount;
            _hp += healAmount;

            // CHECK: 힐량을 기록하는 곳
            // 힐량 저장
            //Debug.Log("힐을 준 당사자는: " + sourceViewID);
            if (healAmount > 0)
            {
                PlayerStatisticsManager.Instance.RecordHealedAmount(sourceViewID, healAmount);
            }
            worldHpBar.value = _hp / _maxHp;

            // 호스트에서 클라이언트로 동기화
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, _hp, _dead);
        }
    }

    [PunRPC]
    public void PlayHitSound()
    {
        //Debug.Log("PlayHitSound RPC 호출됨"); // 호출 여부 확인
        // 3D 오디오 설정이 되어 있는 AudioSource에서 소리 재생
        if (_soundComponent != null)
        {
            _soundComponent.PlaySound(PlayerSoundType.Hit);
            //_playerAudioPlayer.PlayOneShot(hitClip);
        }
    }

    [PunRPC]
    public void PlayHitAnimation()
    {
        if (_hp <= 0) return;
        //Debug.Log("Play Hit Animation RPC 호출됨"); // 호출 여부 확인
        // 3D 오디오 설정이 되어 있는 AudioSource에서 소리 재생
        if (_animator != null && !_dead)
        {
            _animator.SetTrigger("doHit");
            // 일정 시간 동안 멈춤
            if (_playerController != null)
            {
                DisableMovement();
                Invoke(nameof(EnableMovement), 0.5f);
            }
        }
    }

    [PunRPC]
    public void ApplyWorldHpBar(float newHp)
    {
        worldHpBar.value = newHp / _maxHp;
    }

    public void EnableMovement()
    {
        if (_playerController != null)
        {
            _playerController.IsStop(false);
        }
    }

    public void DisableMovement()
    {
        if (_playerController != null && !_dead)
        {
            _playerController.IsStop(true);
        }
    }

    public void Die()
    {
        // 넥서스를 안고 있었다면 떨구기
        if (_playerWithNexus.capturedNexus.activeSelf)
        {
            _playerWithNexus.ReleaseNexus();
        }

        // 넥서스와 상호작용 비활성화
        _playerWithNexus.enabled = false;
        // 컨트롤러 비활성화
        _playerController.enabled = false;

        // 사망 상태를 참으로 변경
        _dead = true;

        _animator.SetTrigger("doDie");
        // _playerAudioPlayer.PlayOneShot(deathClip);
        if (_soundComponent != null)
        {
            _soundComponent.PlaySound(PlayerSoundType.Death);
        }

        // 캐릭터의 죽는 모션을 보여주기 위함
        Invoke(nameof(Vanished), 3f);
        // 일정 시간이 지나면 되살아나게 설정
        Invoke(nameof(Revive), _reviveTime);

        if (photonView.IsMine) // 죽은 플레이어 본인만 보이도록
        {
            // 사망 UI
            IngameUIManager.Inst.ActiveDeadPanel(_reviveTime);
        }
    }

    private void Vanished()
    {
        // 플레이어를 표시하는 gameObject들을 찾아서 꺼주기
        foreach (string name in targetNames)
        {
            Transform child = transform.Find(name);
            if (child != null)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    private void Revive()
    {

        // 캐릭터 위치 초기화
        transform.position = new Vector3(0, 2, 0);

        // 최대 체력 회복
        _hp = _maxHp;

        // 넥서스와 상호작용 활성화
        _playerWithNexus.enabled = true;
        // 컨트롤러 활성화
        _playerController.enabled = true;
        // 사망 상태 변경
        _dead = false;

        foreach (string name in targetNames)
        {
            Transform child = transform.Find(name);
            if (child != null)
            {
                child.gameObject.SetActive(true);
            }
        }

        if (photonView != null && photonView.IsMine)
        {
            worldHpBar?.gameObject.SetActive(false); // hpbar는 자기자신은 보이지않음
        }

        // 캐릭터 직업군에 맞게 애니메이션 파라미터 변경
        int classCode = GameManager.Instance.playerClassCode;
        _animator.SetFloat("CharacterPosition", classCode);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ApplyWorldHpBar", RpcTarget.All, _hp);
        }
    }

    public bool IsDead()
    {
        return _dead;
    }

    public void SaveSkillNode(int num)
    {
        savedSkillNode[num] = true;
    }

    private void HandleLevelUp(int newLevel)
    {
        if (gameObject.activeSelf)
        {
            GameObject levelUpEffect = GameManager.Instance.Instantiate(LevelUpEffectName, gameObject.transform.position, Quaternion.identity);
            levelUpEffect.GetComponent<LevelUpEffect>().SetLevelNumText(newLevel);
            // 플레이어를 부모로 설정하여 이펙트를 자식으로 만듦    
            levelUpEffect.transform.SetParent(gameObject.transform);
            levelUpEffect.transform.localRotation = Quaternion.Euler(0, 0, 0);
            GameManager.Instance.Destroy(levelUpEffect, 2.0f);
        }
    }

    [PunRPC]
    public void ActivePlayerWithNexusAndTower()
    {
        // Debug.Log("찾았어요!!!!!!!!!!!!!!!");
        gameObject.GetComponent<PlayerWithNexus>().enabled = true;
        gameObject.GetComponent<PlayerWithTower>().enabled = true;
    }

    [PunRPC]
    public void NotifyMasterClient(int actorNumber)
    {
        // Debug.Log("!@@플레이어 생성 완료@@"+actorNumber);
        GameManager.Instance.CheckPlayerLoaded(actorNumber);
    }

    // 스킬트리 관련--------------------------------------------------------------

    public int AddSkillPoint()
    {
        _skillPoint++;

        return _skillPoint;
    }

    public int UseSkillPoint()
    {
        _skillPoint--;

        return _skillPoint;
    }

    public void LoadSkill()
    {
        // Debug.Log($"{gameObject.name}의 스킬을 로드 시도");
        int maxSkillLevel = GameConfig.maxSkillLevel;
        //Debug.Log(maxSkillLevel);
        savedSkillNode = new bool[maxSkillLevel + 1];

        if (photonView.IsMine)
        {
            ISkillTree skillTree = gameObject.GetComponent<ISkillTree>();
             Debug.Log("@@레벨계승@@" + _level);
            _skillPoint = IngameManager.Inst.CurLevel;
             Debug.Log("받아온 스킬 포인트는: " + _skillPoint);
            bool[] prevData = GameManager.Instance.savedSkillNode;

            if (prevData != null && prevData.Length > 0)
            {
                for (int i = 1; i <= maxSkillLevel; i++)
                {
                    savedSkillNode[i] = prevData[i];

                    if (prevData[i] == true)
                    {
                        // Debug.Log(i + "번 스킬 활성화");
                        skillTree.activateNode(i);
                        _skillPoint--;
                    }
                }
            }
            if (IngameUIManager.Inst != null)
            {
                // Debug.Log("스킬트리 UI를 CharacterInfo에서 설정하도록");
                //IngameUIManager.Inst.isSkillTreeUIActive = true;
                IngameUIManager.Inst.LoadSkill();
                // IngameUIManager.Inst.isSkillTreeUIActive = false;
                IngameUIManager.Inst.skillTreeUI.SetActive(false);

                //Debug.Log("기본 스킬 포인트 세팅");
                //_skillPoint = 20;
            }
            //ISkillTree skillTree = GameManager.Instance.localPlayer.transform.GetComponent<ISkillTree>();
        }
    }

    public void LoadSkillRequestToOwner()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        if (sceneName == "InGame" || sceneName == "InGame_2S" || sceneName == "InGame_3S")
        {
            if (!photonView.IsMine)
            {
                Debug.Log($"소유주{photonView.Owner.NickName}에게 스킬 정보를 요청");
                // 소유주에게 스킬 정보를 요청
                photonView.RPC("LoadSkillResponseForOthers", photonView.Owner, photonView.ViewID);
            }
        }
    }

    [PunRPC]
    public void LoadSkillResponseForOthers(int sourceViewID, PhotonMessageInfo info)
    {
        // 요청을 보낸 클라이언트의 PhotonPlayer
        Photon.Realtime.Player requestingPlayer = info.Sender;

        // 요청한 클라이언트에게 스킬 정보를 전달
        PhotonView requestingView = PhotonView.Find(sourceViewID);
        if (requestingView != null)
        {
            Debug.Log($"{requestingPlayer.NickName}에게 스킬 정보를 전달");
            requestingView.RPC("ActivateSkillWhenLoaded", requestingPlayer, savedSkillNode);
        }
    }

    [PunRPC]
    public void ActivateSkillWhenLoaded(bool[] loadedSkillInfo)
    {
        Debug.Log("받아온 스킬 정보를 로드 시도");
        ISkillTree skillTree = gameObject.GetComponent<ISkillTree>();
        //로딩 시 스킬포인트 재할당
        bool[] skillInfo = savedSkillNode;
        int size = GameConfig.maxSkillLevel;
        for (int i = 1; i <= size; i++)
        {
            //Debug.Log($"현재{i}번 노드의 값은 {skillInfo[i]}");
            if (loadedSkillInfo[i] == true && skillInfo[i] != true)
            {
                Debug.Log($"Node{i}를 저장한다.");
                skillTree.activateNode(i);
            }
            else
            {
                Debug.Log($"loadedSkillInfo[i]:{loadedSkillInfo[i]}, skillInfo[i]:{skillInfo[i]}");
            }
        }
    }
}