using System.Collections;
using UnityEngine;

public class MagicArrow : MonoBehaviour, IArrow
{
    private float speed = 40f;
    private float _damage;
    private bool _isMagic;
    public GameObject attackedEffect;
    private Vector3 _dir;
    private int _ownerPhotonViewID;

    public void SetIsMagic(bool isMagic)
    {
        _isMagic = isMagic;
    }

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
        Invoke(nameof(DeactivateArrow), 3f);
    }

    void OnDisable()
    {
        CancelInvoke(nameof(DeactivateArrow));
    }

    private void DeactivateArrow()
    {
        //gameObject.SetActive(false);
        GameManager.Instance.Destroy(gameObject);
    }

    void Update()
    {
        transform.position += speed * Time.deltaTime * _dir;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            DeactivateArrow();

            GameObject effect = GameManager.Instance.Instantiate(attackedEffect.name, transform.position, Quaternion.Euler(-90, 0, 0));
            effect.GetComponent<ParticleSystem>().Play();
            //StartCoroutine(DisableAfterParticles(effect));

            // 상대방으로 부터 IDamageable 오브젝트를 가져오는데 성공했다면
            if (other.TryGetComponent<IDamageable>(out var target))
            {
                // 상대방의 OnDamage 함수를 실행시켜서 상대방에게 데미지 주기
                target.OnDamage(_damage, _isMagic, transform.position, _ownerPhotonViewID);
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