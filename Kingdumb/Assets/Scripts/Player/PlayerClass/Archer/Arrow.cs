using System.Collections;
using UnityEngine;

public class Arrow : MonoBehaviour, IArrow
{
    private float speed = 35f;
    private float _damage;
    private bool _isDestroy = true;
    private bool _isKnockBack = false;
    private bool _isTracing = false;
    public GameObject attackedEffect;
    public GameObject tracingArrowPrefab;
    private Vector3 _dir;
    private int _ownerPhotonViewID;

    public void SetDamage(float damage)
    {
        _damage = damage;
    }

    public void SetIsDestroy(bool isDestroy)
    {
        _isDestroy = isDestroy;
    }

    public void SetIsKnockBack(bool isKnockBack)
    {
        _isKnockBack = isKnockBack;
    }

    public void SetIsTracing(bool isTracing)
    {
        _isTracing = isTracing;
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
        Invoke(nameof(DeactivateArrow), 3f);
        Invoke(nameof(EnableTrail), 0.05f);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DeactivateArrow));
        // 화살 재활용 시 꼬리 잔상이 남는 버그가 있어 초기화 해줌
        if (TryGetComponent<TrailRenderer>(out var trail))
        {
            trail.Clear();
            trail.enabled = false;
        }
    }

    private void DeactivateArrow()
    {
        GameManager.Instance.Destroy(gameObject);
        //gameObject.SetActive(false);
    }

    void EnableTrail()
    {
        // 화살 재활용 시 꼬리 잔상이 남는 버그가 있어 초기화 해줌
        if (TryGetComponent<TrailRenderer>(out var trail))
        {
            trail.Clear();
            trail.enabled = true;
        }
    }

    void Update()
    {
        transform.position += speed * Time.deltaTime * _dir;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            GameObject effect = GameManager.Instance.Instantiate(attackedEffect.name, transform.position, Quaternion.Euler(-90, 0, 0));
            effect.GetComponent<ParticleSystem>().Play();
            //StartCoroutine(DisableAfterParticles(effect));

            // 상대방으로 부터 IDamageable 오브젝트를 가져오는데 성공했다면
            if (other.TryGetComponent<IDamageable>(out var damageTarget))
            {
                // 상대방의 OnDamage 함수를 실행시켜서 상대방에게 데미지 주기
                damageTarget.OnDamage(_damage, false, transform.position, _ownerPhotonViewID);
            }

            // 차징 관통 스킬트리 활성화 전이면 화살이 어딘가에 닿았을 때 즉시 사라지도록
            if (_isDestroy)
            {
                DeactivateArrow();
            }

            if (_isKnockBack && other.TryGetComponent<IMonster>(out var monster))
            {
                // 넉백 확률을 설정 (10%의 확률로 넉백 발생)
                float knockbackChance = 0.1f;

                if (Random.Range(0f, 1f) < knockbackChance)
                {
                    monster.Knockback(_dir);
                }
            }

            // 스킬트리 추적 활성화가 되어있다면 추적화살 생성
            if (_isTracing)
            {
                GameObject arrow = GameManager.Instance.Instantiate(tracingArrowPrefab.name, transform.position, Quaternion.LookRotation(_dir));
                if (arrow.TryGetComponent<IArrow>(out var arrowType))
                {
                    arrowType.SetDamage(_damage);
                    arrowType.SetDirection(_dir);
                    arrowType.SetOwnerPhotonViewID(_ownerPhotonViewID);
                }
            }
        }
    }

    private IEnumerator DisableAfterParticles(GameObject effect)
    {
        var particleSystem = effect.GetComponent<ParticleSystem>();
        yield return new WaitUntil(() => !particleSystem.IsAlive());
        //effect.SetActive(false);
        GameManager.Instance.Destroy(effect);
    }
}
