using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class MonsterProjectile : MonoBehaviourPun
{
    public float projectileDamage;
    public float projectileSpeed;
    public GameObject target;
    public bool isTrack; // 계속 타겟을 쫓아갈것인지 유도 여부
    public bool isMagic;

    public GameObject impactParticlePrefab;
    public GameObject projectileParticlePrefab;
    public GameObject muzzleParticlePrefab;

    private GameObject impactParticle;
    private GameObject projectileParticle;
    private GameObject muzzleParticle;

    public float collideOffset = 0.15f;

    public float targetYOffset = 0.5f;

    private int _viewId;

    void Start()
    {
        _viewId = GetComponent<PhotonView>().ViewID;
        
    }

    void OnEnable()
    {
        CancelInvoke(); // 모든 Invoke 초기화
        //Debug.Log("enable확인");
        projectileParticle = GameManager.Instance.Instantiate(projectileParticlePrefab.name, transform.position, transform.rotation) as GameObject;
        projectileParticle.transform.parent = transform; // 날아가는 이펙트는 객체와 연결 
        muzzleParticle = GameManager.Instance.Instantiate(muzzleParticlePrefab.name, transform.position, transform.rotation) as GameObject;
        GameManager.Instance.Destroy(muzzleParticle, 1.5f);
    }

    void OnDisable()
    {
        CancelInvoke(); // 모든 Invoke 초기화
        //Debug.Log("disable확인");    
    }

    void Update()
    {
        if (target != null && isTrack)
        {
            
            Vector3 targetPosition = target.transform.position;
            targetPosition.y += targetYOffset;
            // Debug.Log($"position: {transform.position} targetPosition: {targetPosition}");
            // 타겟 방향으로 이동할 벡터 계산
            Vector3 direction = (targetPosition-transform.position).normalized;

            // 타겟 방향으로 이동
            transform.Translate(direction * projectileSpeed * Time.deltaTime, Space.World);
        }
        else {
            // 앞으로 이동
            transform.Translate(transform.forward * projectileSpeed * Time.deltaTime, Space.World);
        }
    }

    public void Initialize(float _ProjectileDamage, float _ProjectileSpeed, float duration, GameObject _target, bool _isTrack, bool _isMagic)
    {
        projectileDamage = _ProjectileDamage;
        projectileSpeed = _ProjectileSpeed;
        target = _target;
        isTrack = _isTrack;
        isMagic = _isMagic;
        Invoke("DestroySelf", duration);
    }

    // 투사체가 무언가에 부딪혔을 때
    private void OnTriggerEnter(Collider other)
    {
        Vector3 collisionPoint = other.ClosestPoint(transform.position);
        PlayImpactEffect(collisionPoint);
        //Debug.Log("공격하려는 상대의 태그는: " + other.tag);
        if (other.CompareTag("Player") || other.CompareTag("Nexus"))
        {
            IDamageable targetObject = other.gameObject.GetComponent<IDamageable>();

            if (targetObject != null)
            {
                targetObject.OnDamage(projectileDamage, isMagic, collisionPoint, _viewId);
            }
        }
        Invoke("DestroySelf", 0.1f); // 파괴 딜레이 0으로하면 클라이언트에서 충돌하기전에 파괴되서 이펙트가 안나옴..
    }

    public void PlayImpactEffect(Vector3 collisionPoint)
    {
        // 충돌 효과 생성, 충돌 지점의 표면을 향하도록 회전 설정
        impactParticle = GameManager.Instance.Instantiate(impactParticlePrefab.name, transform.position, Quaternion.FromToRotation(Vector3.up, transform.position-collisionPoint));
        
        // 이 아래부분은 파티클의 일부를 분리해서 먼저 삭제하는 로직인데
        // obj풀에서 관리하려면 내 뇌피셜 상으로 다시 재생성해야해서 빡셈
        // 없어도 생각보다 이펙트가 어차피 그럴듯하게 보이므로 일단 주석처리
        
        // // 발사체에 있는 모든 파티클 시스템을 가져와서 배열로 저장
        // ParticleSystem[] trails = GetComponentsInChildren<ParticleSystem>();

        // // 발사체 자식들 중 'Trail' 이름이 포함된 파티클 시스템을 찾아서 분리
        // for (int i = 1; i < trails.Length; i++)
        // {
        //     ParticleSystem trail = trails[i];
        //     if (trail.gameObject.name.Contains("Trail"))
        //     {
        //         trail.transform.SetParent(null); // 트레일을 발사체에서 분리
        //         GameManager.Instance.Destroy(trail.gameObject, 2f); // 분리된 트레일을 2초 후에 제거
        //     }
        // }

        // 충돌 효과를 일정 시간 후 제거
        GameManager.Instance.Destroy(impactParticle, 3.5f);
    }

    public void DestroySelf()
    {
        if (projectileParticle)
        {
            projectileParticle.transform.SetParent(null);
            GameManager.Instance.Destroy(projectileParticle); // 자식에 있는 파티클 제거
        }
        if (photonView != null && PhotonNetwork.IsMasterClient)
        {
            // DestroySelf가 중복호출되서 2번 제거되는 것을 방지
            //Debug.Log($"photonView.gameObject = {photonView.gameObject}");
            if (photonView.gameObject != null)
            {
                PhotonNetwork.Destroy(photonView.gameObject);
            }
        }
        else if (photonView == null) 
        {
            //Debug.Log("no photonview Destroy");
            Destroy(gameObject);
        }
    }
}
