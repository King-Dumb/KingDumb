using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestShot : MonoBehaviour
{
    private float speed = 10f;
    private Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        //Debug.Log("스타트");
        GameManager.Instance.DestroyPhotonObj(gameObject, 1f);

        //Destroy(gameObject, 1.0f);
    }

    private void OnEnable()
    {
        Start();
    }

    // Update is called once per frame
    //public void SetDirection(Vector3 dir)
    //{
    //    direction = dir.normalized;
    //}

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 충돌 시 즉시 삭제
        Destroy(gameObject);
    }
}
