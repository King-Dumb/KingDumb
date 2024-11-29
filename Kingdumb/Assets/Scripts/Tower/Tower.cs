using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Tower : MonoBehaviourPun
{
    // 공격력, 공격속도, 사거리, 공격주기, 타워수명
    private float[] towerInfo = new float[5] { 10f, 50f, 80f, 3f, 180f };

    private float afterAttackTime = 0f;
    // private float towerRemainTime;

    public int towerType;
    public int towerLevel;

    private bool isHighestLevel;

    private GameObject targetMonster;
    private Transform nexusTransform;
    public Transform towerStonePosition;

    private GameObject[] monsterList;
    private GameObject[] playerList;
    public GameObject[] projectileList;
    private TowerGroundMapping towerGroundMapping = new TowerGroundMapping();

    public GameObject[] healTowerEffectList;


    // 건설 시 이펙트, 사운드
    public GameObject buildEffect1;
    public GameObject buildEffect2;

    private AudioSource _audioSource;
    public AudioClip buildAudioClip;

    private void Awake() // 임시로 맵에 있는 몬스터 리스트로 가져오는 코드를 여기에 둠
    {
        playerList = GameObject.FindGameObjectsWithTag("Player");
        nexusTransform = GameObject.FindGameObjectWithTag("Nexus").transform;
        _audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        // // 타워 제한시간 계산
        // if (towerRemainTime > 0)
        // {
        //     towerRemainTime -= Time.deltaTime;
        // }
        // else
        // {
        //     gameObject.transform.parent.GetComponent<ParticleSystem>().Play();
        //     gameObject.SetActive(false);
        // }

        // 타워 공격 쿨타임 계산 및 공격
        if (afterAttackTime < towerInfo[3])
        {
            afterAttackTime += Time.deltaTime;
        }
        else
        {
            afterAttackTime = 0f;

            if (towerType == 3)
            {
                photonView.RPC("HealPlayer", RpcTarget.All);
            }
            else
            {
                TowerTarget();
                if (targetMonster != null)
                {
                    TowerGenerateStoneBroadcast();
                    targetMonster = null;
                }
            }
        }

        // 타워 방향 디버깅용
        // Debug.DrawRay(transform.position, -transform.forward * 100f, Color.red);
    }

    public void Initialize(int towerType, int towerLevel)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        // 타워 데이터 초기화
        towerInfo = TowerInfo.GetTowerInfo(towerType, towerLevel);
        isHighestLevel = false;

        // 타워 남은 시간 초기화
        // towerRemainTime = towerInfo[4];
        afterAttackTime = 0f;

        if (towerLevel > 2)
        {
            isHighestLevel = true;
        }

        // towerType => 0: 전사, 1: 궁수, 2: 마법사, 3: 힐러
        this.towerType = towerType;
        this.towerLevel = towerLevel;

        //Debug.Log("선택된 타워의 타입 : " + towerType + " 선택된 타워의 레벨 : " + towerLevel);
        //Debug.Log(towerInfo[0] + " " + towerInfo[1] + " " + towerInfo[2] + " " + towerInfo[3] + " " + towerInfo[4] + " " + isHighestLevel);

    }

    // 타겟 설정
    public void TowerTarget()
    {
        // 필드에 있는 모든 몬스터 찾아오기
        monsterList = GameObject.FindGameObjectsWithTag("Monster");

        float shortestDistance = 987654321f; // 최소값을 받아오기 위해 임의로 설정한 큰 값

        foreach (GameObject fieldMonster in monsterList)
        {
            Debug.Log(fieldMonster.name);
            if (fieldMonster == null || !fieldMonster.activeSelf || fieldMonster.GetComponent<IMonster>().IsDead())
            {
                continue;
            }

            // 1. 타워의 사거리 내에 있는 적들을 골라내고
            float MonsterTowerDistance = (transform.position - fieldMonster.transform.position).sqrMagnitude;

            if (towerInfo[2] < MonsterTowerDistance)
            {
                continue;
            }

            // 2. 그 중 넥서스에 가장 가까운 적을 타겟으로 설정
            float MonsterNexusDistance = (nexusTransform.position - fieldMonster.transform.position).sqrMagnitude;

            if (shortestDistance > MonsterNexusDistance)
            {
                shortestDistance = MonsterNexusDistance;
                targetMonster = fieldMonster;
            }
        }
    }

    public void TowerGenerateStoneBroadcast()
    {
        Vector3 targetPosition = targetMonster.transform.position;
        photonView.RPC("TowerGenerateStone", RpcTarget.All, towerInfo[0], towerInfo[1], targetPosition, towerType, isHighestLevel); ;
    }

    [PunRPC]
    public void TowerGenerateStone(float damage, float attackSpeed, Vector3 targetPosition, int towerType, bool isHighestLevel)
    {
        // Debug.Log(projectileList[towerType].name);
        GameObject projectile = GameManager.Instance.Instantiate(projectileList[towerType].name, towerStonePosition.position, Quaternion.identity);

        //Debug.Log($"타워 투사체가 생성된 위치 : {projectile.transform.position}");

        // GameObject projectile = Instantiate(projectileList[towerType], towerStonePosition.position, Quaternion.identity);

        // 투사체에 공격력과 공격 속도 넘겨주기
        projectile.GetComponent<Projectile>().Initialize(damage, attackSpeed, targetPosition, towerType, isHighestLevel, photonView.ViewID);
        GameManager.Instance.Destroy(projectile, 3f);
    }

    [PunRPC]
    public void HealPlayer()
    {
        Vector3 effectPosition = transform.position;
        effectPosition.y += 0.1f;

        GameObject healGround = GameManager.Instance.Instantiate(healTowerEffectList[0].name, effectPosition, Quaternion.Euler(-90f, 0f, 0f));
        GameManager.Instance.Destroy(healGround, 1f);

        foreach (GameObject player in playerList)
        {
            float PlayerTowerDistance = (transform.position - player.transform.position).sqrMagnitude;

            if (PlayerTowerDistance > towerInfo[2])
            {
                continue;
            }

            effectPosition = player.transform.position;
            effectPosition.y += 1.5f;

            GameObject healPlayer = GameManager.Instance.Instantiate(healTowerEffectList[1].name, effectPosition, Quaternion.Euler(-90f, 0f, 0f));
            GameManager.Instance.Destroy(healPlayer, 1f);

            if (PhotonNetwork.IsMasterClient)
            {
                if (player != null)
                {
                    player.GetComponent<CharacterInfo>().RestoreHealth(towerInfo[0], transform.position, photonView.ViewID);
                }
            }

            // towerLevel = 3; // 디버깅용

            if (towerLevel == 3)
            {
                // GameObject buffPlayer = GameManager.Instance.Instantiate(healTowerEffectList[2].name, effectPosition, Quaternion.identity);
                // GameManager.Instance.Destroy(buffPlayer, 1f);

                // player.GetComponent<PlayerController>().IncreaseAttackDamage(3f, 3f);
                monsterList = GameObject.FindGameObjectsWithTag("Monster");

                foreach (GameObject fieldMonster in monsterList)
                {
                    if (fieldMonster != null)
                    {

                        float MonsterTowerDistance = (transform.position - fieldMonster.transform.position).sqrMagnitude;

                        if (towerInfo[2] < MonsterTowerDistance)
                        {
                            continue;
                        }

                        if (fieldMonster.CompareTag("Monster"))
                        {
                            IMonster targetMonster = fieldMonster.GetComponent<IMonster>();

                            if (targetMonster != null)
                            {
                                targetMonster.OnDamage(2f, true, transform.position, photonView.ViewID);
                            }
                        }
                    }
                }
            }
        }
    }

    // 사거리 확인하는 기즈모 그리기
    void OnDrawGizmos()
    {
        // Gizmos의 색상을 설정
        Gizmos.color = Color.red;

        // 사거리 경계를 Wireframe 원으로 그린다.
        Gizmos.DrawWireSphere(transform.position, (float)Math.Sqrt(towerInfo[2]));
    }

    public void BuyTowerBroadcast(int index, int requireGold, int builtTowerType, int towerLevel)
    {
        //Debug.Log($"모두에게 {index}에 타워를 설치함을 알린다");
        photonView.RPC("BuyTower", RpcTarget.All, index, requireGold, builtTowerType, towerLevel);
    }

    [PunRPC]
    public void BuyTower(int index, int requireGold, int builtTowerType, int towerLevel)
    {
        Debug.Log($"{index}에 타워를 설치한다는 것을 수신했다");
        //Debug.Log($"현재 설치하려는 타워의 정보 : 타입은 {builtTowerType}, 레벨은 {towerLevel}");
        IngameManager.Inst.BuyTower(requireGold); // 타워 구매 처리
        TowerManager.Inst.SetIsTowerBuilt(index, true); // 타워가 설치되어있는 ground list 관리

        GameObject[] towerGroundList = GameObject.FindGameObjectsWithTag("TowerGround"); // 세부 처리를 위한 검색용
        ShowTowerBuildEffect();
        foreach (GameObject towerGround in towerGroundList)
        {
            int selectedIndex = towerGroundMapping.GetIndexByName(towerGround.name);
            if (index == selectedIndex)
            {
                towerGround.GetComponent<ParticleSystem>().Stop(); // 파티클 시스템 스탑
                gameObject.transform.SetParent(towerGround.transform); // 해당 타워의 부모를 설치된 그라운드로 설정
                Initialize(builtTowerType, towerLevel); // 타워 기능 초기화(마스터만)
                return;
            }
        }
    }

    // 건설 시 이펙트, 사운드 
    private void ShowTowerBuildEffect()
    {
        Debug.Log("ShowTowerBuildEffect");

        _audioSource.PlayOneShot(buildAudioClip);
        GameObject effect1 = GameManager.Instance.Instantiate(buildEffect1.name, gameObject.transform.position, Quaternion.identity);
        GameObject effect2 = GameManager.Instance.Instantiate(buildEffect2.name, gameObject.transform.position, Quaternion.identity);
        GameManager.Instance.Destroy(effect1, 2f);
        GameManager.Instance.Destroy(effect2, 2f);
    }

    public void DestroyTowerRequestToMaster()
    {
        //Debug.Log($"마스터에게 타워를 부술 것을 요청한다");
        photonView.RPC("DestroyTower", RpcTarget.MasterClient);
    }

    [PunRPC]
    public void DestroyTower()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
