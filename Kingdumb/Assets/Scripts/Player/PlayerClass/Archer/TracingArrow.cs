using System.Collections;
using UnityEngine;

public class TracingArrow : MonoBehaviour, IArrow
{
    private float speed = 15f;
    private readonly float rotationSpeed = 50f;
    private float _damage;
    public GameObject attackedEffect;
    private Vector3 _dir;
    public float traceRadius = 50f; // 추적 반경 설정
    private IDamageable target; // 추적 대상
    private int _ownerPhotonViewID;
    private float initializeTime;
    private bool isInitialized = false;

    public void SetDamage(float damage)
    {
        _damage = damage;
    }

    public void SetDirection(Vector3 dir)
    {
        _dir = dir.normalized;
    }

    public void SetOwnerPhotonViewID(int id)
    {
        _ownerPhotonViewID = id;
    }

    void OnEnable() {
        isInitialized = false;
        initializeTime = Time.time + 0.1f; // 0.1초 동안 충돌 무시
        Invoke(nameof(DeactivateArrow), 5f);
        Invoke(nameof(EnableTrail), 0.05f);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DeactivateArrow));
        if (TryGetComponent<TrailRenderer>(out var trail))
        {
            trail.Clear();
            trail.enabled = false;
        }
    }

    private void DeactivateArrow()
    {
        gameObject.SetActive(false);
    }

    void EnableTrail()
    {
        if (TryGetComponent<TrailRenderer>(out var trail))
        {
            trail.Clear();
            trail.enabled = true;
        }
    }

    void Update()
    {
        if (!isInitialized && Time.time >= initializeTime)
        {
            isInitialized = true;
        }

        // 추적 대상을 발견하지 못한 경우 탐지 시도
        if (target == null)
        {
            DetectTarget();
        }

        // 목표가 설정되어 있고, 추적 가능한 거리 내에 있을 경우 추적 시작
        if (target != null)
        {
            TrackTarget();
        }
        else
        {
            // 목표가 없거나 추적 범위를 벗어난 경우 직선 이동
            MoveForward();
        }
    }

    private void MoveForward()
    {
        transform.position += speed * Time.deltaTime * _dir;
    }

    // 주변의 가까운 몬스터 찾기
     private void DetectTarget()
     {
        // DetectionRange 내에 있는 모든 Collider 탐색
        Collider[] colliders = Physics.OverlapSphere(transform.position, traceRadius);
        foreach (Collider collider in colliders)
        {
            IDamageable potentialTarget = collider.GetComponent<IDamageable>();
            if (potentialTarget != null && !potentialTarget.IsDead() && collider.CompareTag("Monster"))
            {
                target = potentialTarget;
                break;
            }
        }
     }

     private void TrackTarget()
     {
        if (target == null)
            return;
        // 목표 방향 계산
        try
        {
            Vector3 direction = (target.Position - transform.position).normalized;
            // 목표를 향해 회전
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        catch(MissingReferenceException)
        {

        }
        
        // 목표를 향해 이동
        transform.position += speed * Time.deltaTime * transform.forward;
     }

    private void OnTriggerEnter(Collider other)
    {
        if (!isInitialized || !other.CompareTag("Monster")) return;

        DeactivateArrow();
        GameObject effect = GameManager.Instance.Instantiate(attackedEffect.name, transform.position, Quaternion.Euler(-90, 0, 0));
        effect.GetComponent<ParticleSystem>().Play();
        //StartCoroutine(DisableAfterParticles(effect));

        // 상대방으로 부터 IDamageable 오브젝트를 가져오는데 성공했다면
        if (other.TryGetComponent<IDamageable>(out var damageTarget))
        {
            // 상대방의 OnDamage 함수를 실행시켜서 상대방에게 데미지 주기
            damageTarget.OnDamage(_damage, false, transform.position, _ownerPhotonViewID);
        }
    }

    private IEnumerator DisableAfterParticles(GameObject effect)
    {
        var particleSystem = effect.GetComponent<ParticleSystem>();
        yield return new WaitUntil(() => !particleSystem.IsAlive());
        effect.SetActive(false);
    }
}
