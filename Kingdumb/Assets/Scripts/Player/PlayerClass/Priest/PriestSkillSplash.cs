using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestSkillSplash : MonoBehaviour
{
    private float duration;
    private GameObject objectPrefab;

    //void Start()
    //{
    //    GameManager.Instance.Destroy(gameObject, duration);
    //}

    private void Update()
    {
        //if (objectPrefab == null)
        //{
        //    GameManager.Instance.Destroy(gameObject);
        //    return;
        //}

        Move();
    }

    private void Move()
    {
        if (objectPrefab == null) return;
        transform.position = objectPrefab.transform.position;
    }

    //public void SetDuration(float dur)
    //{
    //    duration = dur;
    //}

    public void SetObject(GameObject prefab)
    {
        objectPrefab = prefab;
    }
}
