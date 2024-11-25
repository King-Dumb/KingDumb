using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastSplash : MonoBehaviour
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

    public void SetOwnerPhotonViewID(int id)
    {
        _ownerPhotonViewID = id;
    }

    public void SetEffectType(EffectType effectType)
    {
        _effectType = effectType;
    }

    public void SettingSplash(int id, EffectType effectType, float damage)
    {
        _ownerPhotonViewID = id;
        _effectType = effectType;
        _damage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        IDamageable target = other.GetComponent<IDamageable>();
        if (other.CompareTag("Monster"))
        {
            //Debug.Log($"{_effectType} 블래스트 데미지 발생");
            if (_effectType == EffectType.Frost)
            {
                //Debug.Log("빙결 슬로우 발생");
                IMonster monster = other.GetComponent<IMonster>();
                if (monster != null)
                {
                    monster.DebuffSlow(0.4f, 5.0f);

                }
            }
            rend.material.color = Color.red;
            //Debug.Log("Photon ID: " + _ownerPhotonViewID);
            if (target != null)
            {
                target.OnDamage(_damage, true, transform.position, _ownerPhotonViewID);
            }

        }
    }
}
