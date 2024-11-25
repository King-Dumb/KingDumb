using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSplash : MonoBehaviour
{
    private Collider col;
    private Renderer rend;
    private Color originalColor;
    private float _damage;
    private int _ownerPhotonViewID;
    private EffectType _effectType;

    private void Awake()
    {
        col = GetComponent<Collider>();
        rend = GetComponent<Renderer>();

        // 오브젝트의 원래 색상 저장
        originalColor = rend.material.color;
    }

    private void OnDrawGizmos()
    {
        if (col == null) return;

        Gizmos.color = new Color(1, 0, 0, 0.5f); // 반투명 빨간색으로 표시

        if (col is BoxCollider box)
        {
            Gizmos.matrix = Matrix4x4.TRS(box.transform.position, box.transform.rotation, box.transform.lossyScale);
            Gizmos.DrawWireCube(box.center, box.size);
        }
        else if (col is SphereCollider sphere)
        {
            Gizmos.DrawWireSphere(sphere.transform.position + sphere.center, sphere.radius * sphere.transform.lossyScale.x);
        }
        else if (col is CapsuleCollider capsule)
        {
            Gizmos.DrawWireSphere(capsule.transform.position + capsule.center, capsule.radius * capsule.transform.lossyScale.x);
        }
    }

    public void SettingBallSplash(int id, float damage)
    {
        _ownerPhotonViewID = id;
        _damage = damage;
        GameManager.Instance.Destroy(gameObject, 1f);
        //ObjectPool.Instance.SetDestroyTime(gameObject, 1f);
    }

    public void SetOwnerPhotonViewID(int id)
    {
        _ownerPhotonViewID = id;
    }

    public void SetEffectType(EffectType effectType)
    {
        _effectType = effectType;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        if (other.CompareTag("Monster"))
        {
            //Debug.Log($"{_ownerPhotonViewID}정보를 담으려고 함");
            if (target != null)
            {
                target.OnDamage(_damage, true, transform.position, _ownerPhotonViewID);
                //Debug.Log("스플래시 데미지 발생");
            }

        }
    }

    // 충돌 종료 시 호출
    private void OnCollisionExit(Collision collision)
    {
        // 충돌이 끝나면 원래 색상으로 복귀
        rend.material.color = originalColor;
    }
}
