using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

// 기본형의 스켈레톤
public class GolemSkeleton : MonsterBase
{
    [SerializeField] private MonsterWeapon monsterWeapon;
    private BoxCollider weaponCollider;

    public float attackTimeout = 10f;
    public List<float> attackRangeList = new List<float> {1f, 1.5f, 8f, 8f};
    public List<float> outOfAttackRangeList = new List<float> {1.5f, 2f, 10f, 10f};

    public enum BossAttack
    {
        NormalAttack,
        Smash,
        CastSpell,
        BattleCry
    }

    private BossAttack _nextAttack;

    private BossAttack _nextSkill;
    
    [Header("Skill 1 : Smash")]
    public float smashDamage = 50f;
    public GameObject smashDamageField;
    public float fieldDamage = 5f;
    public float fieldDamageInterval = 2f;
    public bool fieldIsMagic = false;
    public float fieldDuration = 10f;
    public float smashAnimationLength = 3.0f;
    public ParticleSystem weaponGlowEffect; 

    [Header("Skill 2 : CastSpell")]

    public float spellDetectionRange = 10f;
    public GameObject prisonSpellZone;
    public float zoneInitialSpeed = 8f;
    public float zoneFinalSpeed = 3f;
    public float zoneDuration = 2.5f; // zone 이동 길이
    public float spellDamage = 10;
    public bool spellIsMagic = true;
    public float spellDuration = 2f;// 속박 길이
    public float spellDamageInterval = 0.5f; 
    public float spellAnimationLength = 6f;

    [Header("Skill 3 : BattleCry & Missile")]
    public GameObject novaEffect;
    public float novaYOffset = 0.1f;
    [Range(0,1)] public float stunChance = 0.33f;
    public GameObject stunEffect;
    public float stunYOffset = 1f;
    public GameObject missilePrefab;
    public float missileGenYOffset = 5f;
    public float missileDetectionRange= 10f;
    public float missileSpeed = 15f;
    public bool missileIsMagic = true;
    public float missileDamage = 50f;
    public float missileDuration = 1f;
    public float battleCryAnimationLength = 3.0f;
    // private field
    private int attackCount;

    private int skillCount;

    private delegate void BossSkill();
    private List<BossSkill> bossSkills = new();

    protected override void Awake()
    {
        base.Awake();
        weaponCollider = monsterWeapon.GetComponent<BoxCollider>();
        
        monsterWeapon.Initialize(monsterInfo.attackDamage, _viewId);

        // 스킬 순서
        bossSkills.Add(NormalAttack);
        bossSkills.Add(GreatSmash);
        bossSkills.Add(CastSpell);
        bossSkills.Add(BattleCry);
        _nextAttack = BossAttack. NormalAttack;

        _maxHp = monsterInfo.hp * (1+PhotonNetwork.CurrentRoom.PlayerCount);
    }

    protected override void Start()
    {
        base.Start();
        StartCoroutine(IncreaseMoveSpeed());
    }

    private IEnumerator IncreaseMoveSpeed()
    {
        while(true)
        {
            _baseMoveSpeed += 0.1f;
            yield return new WaitForSeconds(1f);
        }
    }

    public override void Attack()
    {
        //Debug.Log($"{monsterInfo.monsterName}의 공격");
        
        _baseMoveSpeed = monsterInfo.moveSpeed; // 속도 정상화
        ChangeState(MonsterState.Attacking);
        bossSkills[(int)_nextAttack].Invoke();
    }
    
    private void SetNextAttack(BossAttack nextAttack)
    {
        _nextAttack = nextAttack;
        _currAttackRange = attackRangeList[(int)nextAttack];
        _currOutOfAttackRange = attackRangeList[(int)nextAttack];
    }

    private void ChangeNextAttack()
    {
        switch(_nextAttack)
        {
            case BossAttack.NormalAttack:
                SetNextAttack(_nextSkill);
                ChangeNextSkill();
                break;
            case BossAttack.Smash:
            case BossAttack.CastSpell:
            case BossAttack.BattleCry:
                SetNextAttack(BossAttack.NormalAttack);
                break;
        }
    }

    private void ChangeNextSkill()
    {
        switch(_nextSkill)
        {
            case BossAttack.NormalAttack:
                _nextSkill = BossAttack.Smash;
                break;
            case BossAttack.Smash:
                _nextSkill = BossAttack.CastSpell;
                break;
            case BossAttack.CastSpell:
                _nextSkill = BossAttack.BattleCry;
                break;
            case BossAttack.BattleCry:
                _nextSkill = BossAttack.Smash;
                break;
        }
    }

    private void NormalAttack()
    {
        monsterWeapon.Initialize(monsterInfo.attackDamage, _viewId);
        weaponCollider.enabled = true;
        MonsterAnimationBroadcast("Attack");
        Invoke(nameof(OnAttackEnd), attackAnimationLength);
    }
    private void OnAttackEnd()
    {
        weaponCollider.enabled = false;
        _lastAttackTime = Time.time; // 공격주기 끝난 후에 초기화
        ChangeState(MonsterState.AttackReady);
        ChangeNextAttack();
        //Debug.Log($"{monsterInfo.monsterName}의 공격 종료");
    }

    private void GreatSmash()
    {
        monsterWeapon.Initialize(smashDamage, _viewId);
        weaponCollider.enabled = true;
        MonsterAnimationBroadcast("GreatSmaaash");
        Invoke(nameof(OnAttackEnd), smashAnimationLength);
    }

    private void WeaponGlowEffect()
    {
        weaponGlowEffect.Play();
    }

    public void GenerateField()
    {   
        weaponGlowEffect.Stop();
        if (PhotonNetwork.IsMasterClient)
        {
            Vector3 fieldPos = weaponGlowEffect.transform.position;
            fieldPos.y = 0;
            PhotonNetwork.Instantiate("Monster/"+smashDamageField.name, fieldPos, Quaternion.identity)
            .GetComponent<MonsterDamageField>()
            .Initialize(fieldDamageInterval, fieldDamage, fieldIsMagic, fieldDuration);
        }
    }

    public void CastSpell()
    {
        MonsterAnimationBroadcast("CastSpell");
        GenerateSpellZone();
        Invoke(nameof(OnAttackEnd), spellAnimationLength);
    }

    public void GenerateSpellZone()
    {
        if (PhotonNetwork.IsMasterClient)
        {   
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, spellDetectionRange, LayerMask.GetMask("Player"));
            //Debug.Log($"hitCollidersCount : {hitColliders.Length}");
            foreach(Collider collider in hitColliders)
            {
                // 스펠존 생성
                PhotonNetwork.Instantiate("Monster/"+prisonSpellZone.name, transform.position, Quaternion.Euler(-90, 0, 0))
                .GetComponent<BonePrisonSpellZone>()
                .Initialize(collider.transform, zoneInitialSpeed, zoneFinalSpeed, zoneDuration,
                            spellDamage, spellIsMagic, spellDuration, spellDamageInterval);
            }
        }
    }

    public void BattleCry()
    {
        MonsterAnimationBroadcast("BattleCry");
        //Debug.Log("BattleCry");
        Invoke(nameof(OnAttackEnd), battleCryAnimationLength);
    }

    public void GenerateMissile()
    {
        // 이펙트 실행
        StartCoroutine(ApplyEffect(novaEffect.name, transform.position+transform.up*novaYOffset));
        if (PhotonNetwork.IsMasterClient)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, spellDetectionRange, LayerMask.GetMask("Player"));
            //Debug.Log($"hitCollidersCount : {hitColliders.Length}");
            foreach(Collider collider in hitColliders)
            {
                // 스턴
                if (UnityEngine.Random.value < stunChance)
                {
                    // TODO: 이부분 동기화해야함
                    StartCoroutine(ApplyEffect(stunEffect.name, collider.transform.position+collider.transform.up*stunYOffset));
                    ApplyStun(collider.gameObject);
                }
                // 미사일 생성
                Vector3 missileGenPos = transform.position+transform.up*missileGenYOffset;
                Quaternion missileDirection = Quaternion.LookRotation(collider.transform.position - missileGenPos);
                PhotonNetwork.Instantiate("Monster/"+missilePrefab.name, missileGenPos, missileDirection)
                .GetComponent<MonsterProjectile>()
                .Initialize(missileDamage, missileSpeed, missileDuration, 
                            collider.gameObject, false, missileIsMagic);
            }
        }
    }

    public void ApplyStun(GameObject player)
    {
        //Debug.Log($"Player: {player}가 스턴에 걸림");
        // 임시로 구현 (피격판정을 이용한 경직)
        
        player.GetComponent<IDamageable>()?.OnDamage(0, true, player.transform.position+transform.up, _viewId);
    }

    // novaEffect와 stunEffect가 실행 방식이 너무 동일해서 통일시켰음.
    public IEnumerator ApplyEffect(string effectName, Vector3 effectPos)
    {
        GameObject effectGo = GameManager.Instance.Instantiate(effectName, effectPos, Quaternion.Euler(-90, 0, 0));
        ParticleSystem ps = effectGo.GetComponent<ParticleSystem>();
        ps.Play();
        yield return new WaitForSeconds(ps.main.duration);
        GameManager.Instance.Destroy(effectGo);
    }
}
