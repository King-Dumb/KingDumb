using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Photon.Pun;

public class Nexus : MonoBehaviourPun, IDamageable
{
    public Vector3 Position => transform.position;
    public LayerMask collisionLayer;
    private NavMeshAgent nexusAgent;
    private float nexusNavmeshRange = 30f;
    private NavMeshHit hit;
    private RaycastHit rayHit;
    private Animator nexusAnimator;
    public Slider nexusHealthSlider;
    public GameObject nexusCounterAttackEffect;
    public GameObject nexusCounterAttackEffectCircle;
    private GameObject nexusUnderAttackUI;

    private GameObject nexusEffect1;
    private GameObject nexusEffect2;
    private GameObject nexusEffect3;
    private GameObject nexusEffect4;

    public int nexusPhotonViewID;

    public float nexusMaxHealth; // 넥서스의 최대 체력
    public float _nexusHealth; // 넥서스의 체력

    private float nexusAttackDamage = 3000f;
    private bool isNexusDead = false; // 넥서스 파괴여부
    private bool isCounterAttackExecuted = false;
    private bool isCounterAttacking = false;

    private AudioSource _nexusAudioSource;
    public AudioClip nexusCounterAttackSound1;
    public AudioClip nexusCounterAttackSound2;

    #region 넥서스 이동 관련
    private float nexusMinMovingTime = 12f; // nexusStopMovingTime을 빼야 최소 이동시간이 됨
    private float nexusMaxMovingTime = 30f; // nexusStopMovingTime을 빼야 최대 이동시간이 됨
    private float nexusStopMovingTime = 10f; // 한 번 이동하고 나면 쉬는 시간이 20f로 고정
    private float nexusMoveRemainTime;

    private bool _isNexusStopped = true;
    public bool isNexusStopped
    {
        get { return _isNexusStopped; }
        set
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _isNexusStopped = value;
                nexusAgent.isStopped = _isNexusStopped;

                if (_isNexusStopped)
                {
                    nexusAgent.speed = 0f;
                    nexusAnimator.SetFloat("NexusSpeed", 0f);
                }
            }
        }
    }
    #endregion

    // 몬스터가 처치되었을 시 적용될 delegate
    public event System.Action OnDeath;
    // 난이도가 너무 쉬우면 몬스터 쪽으로 돌진하는 패턴을 추가

    private void Awake()
    {
        //Debug.Log("Nexus Awake Call");
        nexusAgent = GetComponent<NavMeshAgent>();
        nexusAnimator = GetComponent<Animator>();

        nexusUnderAttackUI = IngameUIManager.Inst.nexusUnderAttack;

        //Debug.Log($"넥서스의 최대 체력은:{nexusMaxHealth}, 현재 체력은: {_nexusHealth}");
        _nexusHealth = nexusMaxHealth;
        nexusHealthSlider.maxValue = nexusMaxHealth;
        nexusHealthSlider.value = _nexusHealth;

        _nexusAudioSource = GetComponent<AudioSource>();
    }

    // void Start()
    // {
    //     //Debug.Log("Nexus Start");
    // }

    void OnEnable()
    {
        //Debug.Log("Nexus OnEnable");
        nexusMoveRemainTime = nexusStopMovingTime;
        nexusPhotonViewID = photonView.ViewID; // Start에 있다가 photonView 참조 못해서 OnEnable로 옮김
        isCounterAttacking = false;
    }

    void Update()
    {
        // Debug.Log(_nexusHealth);

        if (!PhotonNetwork.IsMasterClient) return;

        if (isNexusDead)
        {
            return;
        }

        if (!nexusAgent.hasPath && !isNexusStopped) // 넥서스가 이동하고 있지 않다면 애니메이션을 정지
        {
            isNexusStopped = true;
        }

        if (nexusMoveRemainTime < 0f && !isCounterAttacking)
        {
            nexusMove();
        }
        else
        {
            nexusMoveRemainTime -= Time.deltaTime;
        }

        if (!isNexusStopped) // ray에 몬스터가 걸려서 멈추는 경우 다시 moving 쿨타임이 지나서 움직이려고 해도 몬스터가 그대로 그자리에 있으면 안움직이는 문제가 있음 >> 이게 더 자연스러울 수 있음 + 몬스터가 가만히 있을 리 없음이라 일단 넘어감
        {
            Vector3 rayDirection = transform.forward;
            rayDirection.y += 0.5f;

            if (Physics.Raycast(transform.position, rayDirection, out rayHit, 0.8f, collisionLayer))
            {
                //Debug.Log("몬스터가 감지되어 넥서스 멈춤");
                isNexusStopped = true;
            }
            // Debug.DrawRay(transform.position, rayDirection * 0.8f, Color.red);
        }

        // if (PhotonNetwork.IsMasterClient)
        // {
        //     // 디버깅용
        //     if (Input.GetKeyDown(KeyCode.I))
        //     {
        //         //     OnDamage(10f, false, transform.position, 0);
        //         //nexusHealth = 0f;
        //     }
        //     if (Input.GetKeyDown(KeyCode.B))
        //     {
        //         NexusCounterAttack();
        //     }
        // }

    }

    public void nexusMove()
    {
        //Debug.Log("NexusMove!!");
        isNexusStopped = false;

        NexusAnimationBroadcast("NexusUnderAttack", true);
        //nexusAnimator.ResetTrigger("NexusUnderAttack");
        NexusAnimationBroadcast("Move", false);
        //nexusAnimator.SetTrigger("Move");

        nexusMoveRemainTime = Random.Range(nexusMinMovingTime, nexusMaxMovingTime);
        //Debug.Log($"다음 이동은 {nexusMoveRemainTime - nexusStopMovingTime}초 후");

        // 넥서스의 새로운 목적지(newDestination) 설정
        // nexusNavmeshRange에 -(최소), +(최대) 값 만큼의 범위 내에서 움직임
        // ex) nexusNavmeshRange가 50이면 다음 목적지 x, z 좌표는 각각 -50 ~ +50 의 범위 내 랜덤 값
        Vector3 newDestination = new Vector3(Random.Range(-nexusNavmeshRange, nexusNavmeshRange), 0, Random.Range(-nexusNavmeshRange, nexusNavmeshRange));
        transform.LookAt(newDestination);
        //Debug.Log("다음 위치 : " + newDestination);

        float distance = (transform.position - newDestination).magnitude;
        //Debug.Log("이동해야 하는 거리 : " + distance);

        // 1. 다음 이동까지 남은 시간(nexusMoveRemainTime) 에서 넥서스 이동 쿨타임(nexusStopMovingTime) 만큼의 시간을 제외한 값으로
        // 2. 다음 목적지까지 남은 거리(distance)를 나눠
        // 3. 넥서스의 이동 속력(nexusSpeed)를 구함
        float nexusSpeed = distance / (nexusMoveRemainTime - nexusStopMovingTime);
        nexusAgent.speed = nexusSpeed;
        nexusAnimator.SetFloat("NexusSpeed", nexusSpeed);
        //Debug.Log("넥서스의 이동속도 : " + nexusSpeed);
        // newDestination에서 받아온 랜덤한 위치가 navmesh 위에 없으면 이동하지 않고 종료
        if (!NavMesh.SamplePosition(newDestination, out hit, 0.1f, NavMesh.AllAreas))
        {
            //Debug.Log("NavMesh 위에 없어서 return함");
            return;
        }

        // 실제 이동
        nexusAgent.SetDestination(newDestination);
    }

    [PunRPC]
    public void ApplyUpdatedHealthForNexus(float newHealth, bool newDead)
    {
        _nexusHealth = newHealth;
        isNexusDead = newDead;
        nexusHealthSlider.value = _nexusHealth;

        nexusUnderAttackUI.SetActive(true);

        // TODO: 알림 동기화 필요
        Invoke("HideNexusDamageAlert", 3f);
    }

    [PunRPC]
    public void OnDamage(float damage, bool isMagic, Vector3 hitPoint, int sourceViewID)
    {
        if (isCounterAttacking)
        {
            return;
        }

        if (IsDead())
        {
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            // TODO: 애니메이션 동기화 필요
            NexusAnimationBroadcast("NexusUnderAttack", false);
            //nexusAnimator.SetTrigger("NexusUnderAttack");

            _nexusHealth = _nexusHealth > damage ? _nexusHealth - damage : 0;
            ////Debug.Log($"넥서스 체력 {nexusHealth}로 감소");

            //Debug.Log($"Nexus is UnderAttack nexusHealth:{_nexusHealth}, damage:{damage}");
            // 호스트에서 클라이언트로 동기화
            // 다른 클라이언트들도 OnDamage를 실행하도록 함
            photonView.RPC("OnDamage", RpcTarget.Others, damage, isMagic, hitPoint, sourceViewID);
            photonView.RPC("ApplyUpdatedHealthForNexus", RpcTarget.Others, _nexusHealth, isNexusDead);
            // TODO: UI동기화 필요
            nexusHealthSlider.value = _nexusHealth;
            nexusUnderAttackUI.SetActive(true);

            // TODO: 알림 동기화 필요
            Invoke("HideNexusDamageAlert", 3f); // Invoke 말고 다른 방법은 없을까~~ 아예 nexusUI 쪽에서 처리하는 것도~~
            // -> UI 자체에서 OnEnable로 계산, if activeself면 껐다켜기

            if (!isNexusStopped) // 계속 맞고 있으면 이동하지 않는 상황 방지
            {
                isNexusStopped = true;
            }

            //-----------------------------------------------------------------------------
        }
        // 체력이 30% 아래로 떨어지면 반격 패턴 실행
        if (_nexusHealth < (nexusMaxHealth * 0.3) && !isCounterAttackExecuted)
        {
            isCounterAttackExecuted = true;
            NexusCounterAttack();
        }
        // 넥서스가 죽은 경우
        if (_nexusHealth <= 0)
        {
            isNexusDead = true;
            OnDeath?.Invoke();
            NexusAfterDie();
            return;
        }

        //Debug.Log($"OnDamage를 처리한 뒤의 왕자의 체력은 {_nexusHealth} 입니다. 그리고 GetNexusHp를 하면{CharacterManager.Inst.GetNexusHp()}이 나오네요.");
    }

    [PunRPC]
    public void RestoreHealth(float amount, Vector3 transform, int pvId)
    {
        if (isCounterAttacking)
        {
            return;
        }

        if (IsDead())
        {
            return;
        }

        // Debug.Log($"힐량 : {amount}, From : {pvId}");

        if (PhotonNetwork.IsMasterClient)
        {
            _nexusHealth = _nexusHealth + amount > nexusMaxHealth ? nexusMaxHealth : _nexusHealth + amount;

            nexusHealthSlider.value = _nexusHealth;

            // 호스트에서 클라이언트로 동기화
            photonView.RPC("ApplyUpdatedHealthForNexus", RpcTarget.Others, _nexusHealth, IsDead());
        }
    }

    public bool IsDead()
    {
        return isNexusDead;
    }

    public void NexusAfterDie()
    {
        // 죽었을 때 모션 등 호출
        isNexusStopped = true;
        nexusAnimator.ResetTrigger("NexusUnderAttack");
        nexusAnimator.SetTrigger("NexusDie");

    }

    public void NexusCounterAttack()
    {
        isNexusStopped = true;
        isCounterAttacking = true;

        //NexusAnimationBroadcast("Move", true);
        nexusAnimator.ResetTrigger("Move");
        //NexusAnimationBroadcast("NexusUnderAttack", true);
        nexusAnimator.ResetTrigger("NexusUnderAttack");
        //NexusAnimationBroadcast("NexusUnderAttack", false);
        nexusAnimator.SetTrigger("NexusCounterAttack"); // 반격 모션 실행

        // 음성 재생
        _nexusAudioSource.PlayOneShot(nexusCounterAttackSound1);
        _nexusAudioSource.PlayOneShot(nexusCounterAttackSound2);

        StartCoroutine(NexusCounterAttackCoroutine());
    }

    private IEnumerator NexusCounterAttackCoroutine()
    {
        Quaternion quaternion = transform.rotation * Quaternion.Euler(-90f, 0f, 0f);
        nexusEffect1 = Instantiate(nexusCounterAttackEffect, transform.position, quaternion);

        yield return new WaitForSeconds(2f);

        nexusEffect2 = Instantiate(nexusCounterAttackEffectCircle, transform.position, quaternion);
        nexusEffect2.transform.localScale = new Vector3(8f, 8f, 8f);
        CreateOverlapSphere(2.5f); // 3번에 걸쳐서 Create 하지 말고 여기에서 5f 하는 게 나을 수도

        yield return new WaitForSeconds(0.2f);

        nexusEffect3 = Instantiate(nexusCounterAttackEffectCircle, transform.position, quaternion);
        nexusEffect3.transform.localScale = new Vector3(11f, 11f, 11f);
        CreateOverlapSphere(6f);

        yield return new WaitForSeconds(0.2f);

        nexusEffect4 = Instantiate(nexusCounterAttackEffectCircle, transform.position, quaternion);
        nexusEffect4.transform.localScale = new Vector3(13f, 13f, 13f);
        CreateOverlapSphere(9f);

        yield return new WaitForSeconds(1f);

        Destroy(nexusEffect1);
        Destroy(nexusEffect2);
        Destroy(nexusEffect3);
        Destroy(nexusEffect4);

        isNexusStopped = false;
        isCounterAttacking = false;
    }

    private void OnDisable()
    {
        if (nexusEffect1 != null)
        {
            Destroy(nexusEffect1);
        }
        if (nexusEffect2 != null)
        {
            Destroy(nexusEffect2);
        }
        if (nexusEffect3 != null)
        {
            Destroy(nexusEffect3);
        }
        if (nexusEffect4 != null)
        {
            Destroy(nexusEffect4);
        }
    }

    public void FinishNexusCounterAttack()
    {
        Destroy(nexusCounterAttackEffect);
    }

    public void HideNexusDamageAlert()
    {
        nexusUnderAttackUI.SetActive(false);
    }

    private void CreateOverlapSphere(float radius)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, collisionLayer);

        foreach (Collider hitCollider in hitColliders)
        {
            GolemSkeleton golemSkeleton = hitCollider.gameObject.GetComponent<GolemSkeleton>();

            if (golemSkeleton != null)
            {
                continue;
            }

            IMonster targetMonster = hitCollider.gameObject.GetComponent<IMonster>();

            if (targetMonster != null)
            {
                // //Debug.Log($"넥서스 OnDamage 전 PhotonViewID : {nexusPhotonViewID}");
                targetMonster.OnDamage(nexusAttackDamage, true, transform.position, nexusPhotonViewID);
            }
        }
    }

    // private void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.magenta;
    //     Gizmos.DrawWireSphere(transform.position, radius);
    // }

    // 애니메이션 관련 ------------------------------------------------------------------------------
    public void NexusAnimationBroadcast(string triggerName, bool isReset)
    {
        if (isReset)
        {
            photonView.RPC("StopNexusAnimation", RpcTarget.All, triggerName);
        }
        else
        {
            photonView.RPC("PlayNexusAnimation", RpcTarget.All, triggerName);
        }
    }

    [PunRPC]
    public void PlayNexusAnimation(string triggerName)
    {
        //Debug.Log("PlayNexusAnimation: " + triggerName);
        nexusAnimator.SetTrigger(triggerName);
    }

    [PunRPC]
    public void StopNexusAnimation(string triggerName)
    {
        //Debug.Log("StopNexusAnimation: " + triggerName);
        nexusAnimator.ResetTrigger(triggerName);
    }
}
