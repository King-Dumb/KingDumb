using System.Collections;
using Photon.Pun;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class PriestGolem : MonoBehaviourPun
{
    private int ownerPvId;
    private float damage;
    private float speed;
    private float detectionRadius = 10f; // 탐지 범위
    private float attackRange = 5f; // 공격 범위
    private float attackCooldown = 1f; // 공격 쿨다운 시간
    private float lastAttackTime = 0f;  // 마지막 공격 시간

    private Transform priest; // 사제
    private Transform currentTarget; // 현재 타겟
    private NavMeshAgent agent; // 이동을 위한 NavMeshAgent

    private Animator animator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        agent.speed = speed;
    }

    void Update()
    {
        if (!photonView.IsMine)
            return;

        // 범위 내에 타겟이 있는지 확인
        FindTargetInRange();

        if (currentTarget != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTarget.position);

            if (distanceToTarget <= attackRange)
            {
                // agent.isStopped = true;
                animator.SetBool("hasTarget", false);

                // 목표 방향 계산
                Vector3 direction = (currentTarget.position - transform.position).normalized;
                Quaternion lookRotation = Quaternion.LookRotation(direction);

                // 부드러운 회전 적용
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 7f); // 회전 속도 조절

                // 공격
                Attack();
            }
            else
            {
                // agent.isStopped = false;
                animator.SetBool("hasTarget", true);
                agent.SetDestination(currentTarget.position);
            }
        }

    }

    void FindTargetInRange()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadius);
        var potentialTargets = hitColliders
            .Where(collider => collider.CompareTag("Monster"))  // "Target" 태그를 가진 오브젝트 탐색
            .Where(damageable => damageable != null && !damageable.gameObject.GetComponent<IDamageable>().IsDead()) // 죽지 않은 대상만 필터링
            .Select(collider => collider.transform)
            .ToArray();

        // 타겟이 있으면 가장 가까운 타겟을 선택, 없으면 사제를 타겟으로 설정
        if (potentialTargets.Length > 0)
        {
            currentTarget = potentialTargets.OrderBy(target => Vector3.Distance(transform.position, target.position)).FirstOrDefault();
        }
        else
        {
            currentTarget = priest;  // 범위 내 타겟이 없으면 사제를 추적
        }
    }

    void Attack()
    {
        // 몬스터가 아니라면 공격 안 함
        if (!currentTarget.gameObject.CompareTag("Monster")) return;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;


            SetAttackAnimationTrigger();
            //animator.SetTrigger("IsAttack");


            // 공격 로직 (예: 데미지 주기)
            //PhotonView pv = GetComponent<PhotonView>();
            //currentTarget.gameObject.GetComponent<IDamageable>().OnDamage(damage, false, currentTarget.position, pv.ViewID);

            //photonView.RPC("OnDamageBroadcast", RpcTarget.All, 5f * nexusControlTimeDamageMultiplier, true, transform.position, nexusScript.nexusPhotonViewID);

            // PhotonView를 통해 OnDamage RPC 호출
            PhotonView targetPhotonView = currentTarget.gameObject.GetComponent<PhotonView>();

            if (targetPhotonView != null)
            {
                // target의 PhotonView.ViewID를 전송
                photonView.RPC("BroadcastOnDamage", RpcTarget.All, targetPhotonView.ViewID, damage, false, currentTarget.position, photonView.ViewID);
            }


            // 타겟이 사망하거나 비활성화 상태라면 새로운 타겟을 찾음
            if (currentTarget != null && currentTarget.gameObject.GetComponent<IDamageable>().IsDead())
            {
                currentTarget = null;
            }
        }
    }

    public void SetOwnerPvId(int id)
    {
        ownerPvId = id;
    }

    public void SetPriest(Transform priest)
    {
        this.priest = priest;
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }

    public void SetSpeed(float sp)
    {
        speed = sp;
    }


    public void SetAttackAnimationTrigger()
    {
        photonView.RPC("AnimationBroadcast", RpcTarget.All, "IsAttack");
    }

    [PunRPC]
    public void AnimationBroadcast(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    [PunRPC]
    public void BroadcastOnDamage(int targetViewId, float damage, bool isMagic, Vector3 hitPoint, int attackerViewId)
    {
        // PhotonView를 사용해 대상 객체 찾기
        PhotonView targetPhotonView = PhotonView.Find(targetViewId);

        if (targetPhotonView != null)
        {
            // IDamageable 인터페이스를 구현한 컴포넌트에 OnDamage 호출
            IDamageable damageable = targetPhotonView.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.OnDamage(damage, isMagic, hitPoint, attackerViewId);
            }
        }
    }

}
