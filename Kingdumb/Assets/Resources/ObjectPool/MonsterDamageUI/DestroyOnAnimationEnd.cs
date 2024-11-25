using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyOnAnimationEnd : MonoBehaviour
{
    public void DestroySelf()
    {
        //Debug.Log("데미지 텍스트 삭제");
        GameManager.Instance.Destroy(gameObject);
    }
}
