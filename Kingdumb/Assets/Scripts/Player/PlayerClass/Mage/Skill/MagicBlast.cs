using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBlast : MonoBehaviour
{
    public float speed = 10f;
    public GameObject blastEffect;
    private Vector3 direction;
    private int _ownerPhotonViewID;
    private EffectType _effectType;
    private Vector3 _scale;

    public void SetOwnerPhotonViewID(int id)
    {
        _ownerPhotonViewID = id;
    }

    public void Blast(EffectType effectType, float damage, Vector3 scale)
    {
        _effectType = effectType;
        //Debug.Log($"{effectType} 폭발 발생");
        GameObject effectInstance = GameManager.Instance.Instantiate(blastEffect.name, transform.position, Quaternion.Euler(-90, 0, 0));
        effectInstance.transform.localScale = scale;
        //Debug.Log("크기 확인: " + effectInstance.transform.localScale);
        effectInstance.GetComponent<BlastSplash>().SettingSplash(_ownerPhotonViewID, effectType, damage);
        GameManager.Instance.Destroy(effectInstance, 1.0f);
    }

    // Update is called once per frame

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("Player") || other.CompareTag("Monster"))
    //    {
    //        if (!other.CompareTag("Wall"))
    //        {
    //            //Debug.Log($"{_effectType} 블래스트 데미지 발생");
    //            if (_effectType == EffectType.Frost) 
    //            {
    //                Debug.Log("빙결 슬로우 발생");
    //                IMonster target = other.GetComponent<IMonster>();
    //                if (target != null)
    //                {
    //                    target.DebuffSlow(10.0f, 1.0f);
    //                }
    //            }
    //        }
    //    }
    //}
}
