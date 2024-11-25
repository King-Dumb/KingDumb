using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class FireBall : MonoBehaviour
{
    public float _speed;
    private float _damage;
    private float _splashDamage;
    public GameObject novaEffect;
    private Vector3 _dir;
    private int _ownerPhotonViewID;
    public float RotationSpeed = 5f;       // 회전 속도
    public float DetectionRange = 7f;     // 탐색 거리 (탐지 반경)
    public float MaxTrackingRange = 30f;   // 최대 추적 거리 (투사체와 초기 위치 간의 거리)
    private IDamageable target;            // 추적 대상
    private bool _isTrackable = false;
    private bool _isGrounded = false; // 바닥에 닿았는지 여부
    private float groundY; // 바닥의 y축 위치를 저장할 변수

    private Coroutine _coroutine;

    // Start is called before the first frame update
    void Start()
    {
        //Destroy(gameObject, 3.0f);
    }

    void OnEnable()
    {
        target = null;    
    }

    // Update is called once per frame
    public void SetDirection(Vector3 dir)
    {
        _dir = dir.normalized;
    }

    public void SetOwnerPhotonViewID(int id)
    {
        _ownerPhotonViewID = id;
    }

    public void SettingBall(Vector3 dir, int id, float damage, float splashDamage, float speed, 
        bool isTrackable)
    {
        _ownerPhotonViewID = id;
        _damage = damage;
        _splashDamage = splashDamage;
        _dir = dir.normalized;
        _speed = speed;
        _isTrackable = isTrackable;
        GetComponent<Collider>().enabled = true;

        GameManager.Instance.Destroy(gameObject, 3.0f);
    }

    private void Update()
    {
        if (_isTrackable)
        {
            // 추적 대상을 발견하지 못한 경우 탐지 시도
            if (target == null)
            {
                DetectTarget();
            }

            // 목표가 설정되어 있고, 추적 가능한 거리 내에 있을 경우 추적 시작
            if (target != null)
            {
                TrackTarget();
            }
            else
            {
                // 목표가 없거나 추적 범위를 벗어난 경우 직선 이동
                MoveForward();
            }
        }
        else
        {
            MoveForward();
        }
       
    }

    private void DetectTarget()
    {
        //Debug.Log("적 감지 시도");
        // DetectionRange 내에 있는 모든 Collider 탐색
        Collider[] colliders = Physics.OverlapSphere(transform.position, DetectionRange);
        foreach (Collider collider in colliders)
        {
            IDamageable potentialTarget = collider.GetComponent<IDamageable>();
            if (potentialTarget != null && !potentialTarget.IsDead() && collider.CompareTag("Monster"))
            {
                //Debug.Log("탐지한 적의 상태:" + potentialTarget.IsDead());
                target = potentialTarget;
                break;
            }
        }
    }

    private void TrackTarget()
    {
        //Debug.Log("적 추적 시도");
        if (target == null)
            return;
        // 목표 방향 계산
        try
        {
            Vector3 direction = (target.Position - transform.position).normalized;
            // 목표를 향해 회전
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSpeed * Time.deltaTime);
        }
        catch(MissingReferenceException)
        {

        }


        // 목표를 향해 이동
        transform.position += transform.forward * _speed * Time.deltaTime;
    }

    private void MoveForward()
    {
        transform.position += _dir * _speed * Time.deltaTime;
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster") || other.CompareTag("Wall"))
        {
            GetComponent<Collider>().enabled = false;
            //GameObject attackProjectile = ObjectPool.Instance.CreateObj(effectPrefabs[effectType].name, position + direction, Quaternion.identity);
            GameObject effectInstance = ObjectPool.Instance.CreateObj(novaEffect.name, transform.position, Quaternion.Euler(-90, 0, 0));
            //GameObject effectInstance = Instantiate(novaEffect, transform.position, Quaternion.Euler(-90, 0, 0));
            effectInstance.GetComponent<BallSplash>().SettingBallSplash(_ownerPhotonViewID, _splashDamage);

            IDamageable target = other.GetComponent<IDamageable>();
            // 상대방으로 부터 IDamageable 오브젝트를 가져오는데 성공했다면
            if (target != null)
            {
                // 상대방의 OnDamage 함수를 실행시켜서 상대방에게 데미지 주기
                target.OnDamage(_damage, true, transform.position, _ownerPhotonViewID);
            }

            GameManager.Instance.Destroy(gameObject);   
        }
    }
}
