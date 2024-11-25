using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// 몬스터가 생성하는 도트딜을 주는 영역
/// </summary>
public class MonsterDamageField : MonoBehaviourPun
{
    private float _damageInterval;
    private float _damageAmount;
    private bool _isMagic;
    private Dictionary<GameObject, float> _timers = new(); // 오브젝트별 데미지

    private int _viewId;

    protected void Start()
    {
        _viewId = GetComponent<PhotonView>().ViewID;
    }

    void OnTriggerStay(Collider other)
    {
        GameObject targetObject = other.gameObject;
        if (targetObject.CompareTag("Player") || targetObject.CompareTag("Nexus"))
        {
            // 타이머에 값이 존재하지않거나, 존재한다면 그값이 time.Time (현재시간)보다 damageInterval 이상 차이나는 경우 실행
            float lastAttackTime;
            if (!_timers.TryGetValue(targetObject,  out lastAttackTime) || Time.time > lastAttackTime + _damageInterval)
            {
                _timers[targetObject] = Time.time;
                targetObject.GetComponent<IDamageable>()?.OnDamage(_damageAmount, _isMagic, other.ClosestPoint(transform.position), _viewId);
                DealEffect();
            }

        }
        
    }
    void OnTriggerExit(Collider other)
    {
        // _timers.Remove(other.gameObject); 
        //나갔다가 재입장시 데미지 안 입을려면 없애는게 
    }

    public void Initialize(float damageInterval, float damageAmount, bool isMagic, float duration)
    {
        _damageInterval = damageInterval;
        _damageAmount = damageAmount;
        _isMagic = isMagic;
        _timers.Clear();
        Invoke("DestroySelf", duration);
    }

    public virtual void DestroySelf()
    {
        _timers.Clear();
        if (photonView != null && PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.Destroy(photonView.gameObject);
        }
        else if (photonView == null) 
        {
            Destroy(gameObject);
        }
    }

    protected virtual void DealEffect()
    {
        
        // 필요시 override하고 DealEffect
    }
}
