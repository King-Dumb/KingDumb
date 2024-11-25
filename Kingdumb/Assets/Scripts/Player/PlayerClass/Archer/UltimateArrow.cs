using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UltimateArrow : MonoBehaviour, IArrow
{
    private float speed = 5f;
    private float _damage;
    public GameObject attackedEffect;
    private Vector3 _dir;
    private int _ownerPhotonViewID;
    private readonly Dictionary<GameObject, Coroutine> _damageCoroutines = new();

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

    void OnEnable()
    {
        Invoke(nameof(DeactivateArrow), 15f);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DeactivateArrow));
    }

    private void DeactivateArrow()
    {
        gameObject.SetActive(false);
    }

    void Update()
    {
        transform.position += speed * Time.deltaTime * _dir;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            if (!_damageCoroutines.ContainsKey(other.gameObject))
            {
                Coroutine damageCoroutine = StartCoroutine(ApplyDamageOverTime(other.gameObject));
                _damageCoroutines.Add(other.gameObject, damageCoroutine);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_damageCoroutines.ContainsKey(other.gameObject))
        {
            StopCoroutine(_damageCoroutines[other.gameObject]);
            _damageCoroutines.Remove(other.gameObject);
        }
    }

    private IEnumerator ApplyDamageOverTime(GameObject target)
    {
        while (target.activeInHierarchy) // 타겟이 활성화 된 경우에만 루프 반복
        {
            ApplyDamage(target);
            yield return new WaitForSeconds(0.1f);
        }

        // 코루틴 종료 후 사전에 할당된 코루틴 참조 제거
        if (_damageCoroutines.ContainsKey(target))
        {
            _damageCoroutines.Remove(target);
        }
    }

    private void ApplyDamage(GameObject target)
    {
        // target에 데미지를 입히는 이펙트
        GameObject effect = GameManager.Instance.Instantiate(attackedEffect.name, target.transform.position, Quaternion.Euler(-90, 0, 0));
        // 일정 시간이 지나면 이펙트를 비활성화
        effect.GetComponent<ParticleSystem>().Play();
        //StartCoroutine(DisableAfterParticles(effect));

        // 상대방으로 부터 IDamageable 오브젝트를 가져오는데 성공했다면
        if (target.TryGetComponent<IDamageable>(out var targetIDmg))
        {
            // 상대방의 OnDamage 함수를 실행시켜서 상대방에게 데미지 주기
            targetIDmg.OnDamage(_damage, true, transform.position, _ownerPhotonViewID);
        }
    }

    private IEnumerator DisableAfterParticles(GameObject effect)
    {
        var particleSystem = effect.GetComponent<ParticleSystem>();
        yield return new WaitUntil(() => !particleSystem.IsAlive());
        effect.SetActive(false);  // 풀로 반환
    }
}
