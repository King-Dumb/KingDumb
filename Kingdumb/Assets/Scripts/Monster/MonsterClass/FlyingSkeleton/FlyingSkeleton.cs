using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class FlyingSkeleton : MonsterBase
{
    public Transform head;
    public float attackSpeed = 10f;      // 머리가 날아가는 속도
    public float stopDistance = 1f;      // 플레이어에게 가까이 갈 때 멈추는 거리
    public float returnSpeed = 5f;       // 머리가 돌아오는 속도

    [SerializeField] private MonsterWeapon monsterWeapon; // 자기 몸임
    private Collider attackArea;

    public GameObject attackEffectPrefab;

    private Coroutine attackCoroutine;

    private Vector3 originalPosition = Vector3.up*2f;

    public override Vector3 Position => head.position;

    protected override void Awake()
    {
        base.Awake();
        attackArea = monsterWeapon.GetComponent<SphereCollider>();
        attackArea.enabled = false;
        OnDeath += OnDeathDuringAttack;
        _collider = monsterWeapon.GetComponent<CapsuleCollider>();
        monsterWeapon.Initialize(monsterInfo.attackDamage, _viewId, photonView); // 의존성 주입
    }

    public override void Initialize()
    {
        base.Initialize();
        head.localPosition = originalPosition;
    }

    public override void Attack()
    {
        attackCoroutine = StartCoroutine(AttackCoroutine());
        // Debug.Log($"{monsterInfo.monsterName}의 공격");
    }
    private IEnumerator AttackCoroutine()
    {
        ChangeState(MonsterState.Attacking);
        attackArea.enabled = true;
        Vector3 targetPosition = GetTargetPosition();
        targetPosition.y = 0.5f;
        // 플레이어 앞 일정 거리 위치 계산
        while (Vector3.Distance(head.transform.position, targetPosition) > 0.1f && _state != MonsterState.Dead)
        {
            head.transform.position = Vector3.MoveTowards(head.transform.position, targetPosition, attackSpeed * Time.deltaTime);
            yield return null;
        }

        attackArea.enabled = false;
        // 머리 원래 위치로 돌아오기
        while (Vector3.Distance(head.transform.localPosition, originalPosition) > 0.1f && _state != MonsterState.Dead)
        {
            head.transform.localPosition = Vector3.MoveTowards(head.transform.localPosition, originalPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }
        head.transform.localPosition = originalPosition;
        // Debug.Log("공격종료");
        ChangeState(MonsterState.AttackReady);
    }
    void OnDeathDuringAttack()
    {
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
        }
        attackArea.enabled = false;
    }
    Vector3 GetTargetPosition()
    {
        // 타겟이 자신을 바라보는 방향
        Vector3 direction = (head.transform.position - _target.transform.position).normalized;

        // XZ 평면에서만 계산 (Y축 제거)
        direction.y = 0;

        // 타겟 위치에서 방향으로 일정 거리 떨어진 최종 위치 계산
        Vector3 finalPosition = _target.transform.position + direction * stopDistance;

        return finalPosition;
    }

    [PunRPC]
    public void ApplyHitEffect(Vector3 position)
    {
        GameObject _effect = GameManager.Instance.Instantiate(attackEffectPrefab.name, position, Quaternion.identity);
        if (_effect != null)
        {
            _effect.GetComponent<ParticleSystem>()?.Play();
            GameManager.Instance.Destroy(_effect, 2f);
        }
    }
}
