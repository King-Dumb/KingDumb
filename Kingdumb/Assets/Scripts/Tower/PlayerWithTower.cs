using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerWithTower : MonoBehaviourPun
{
    private GameObject towerInfoUI;
    private TowerBuyUI towerBuyUIScript;
    private GameObject[] towerGroundList;

    private InputHandler inputHandler;

    private int closestTowerGroundIndex;

    private bool isNearTower = false;
    private TowerGroundMapping towerGroundMapping = new TowerGroundMapping();
    void Start()
    {
        inputHandler = GetComponent<InputHandler>();

        towerInfoUI = IngameUIManager.Inst.towerInfoUI;
        towerBuyUIScript = IngameUIManager.Inst.towerBuyUI.GetComponent<TowerBuyUI>();
        towerGroundList = GameObject.FindGameObjectsWithTag("TowerGround");
    }

    public void Init()
    {
        inputHandler = GetComponent<InputHandler>();

        towerInfoUI = IngameUIManager.Inst.towerInfoUI;
        towerBuyUIScript = IngameUIManager.Inst.towerBuyUI.GetComponent<TowerBuyUI>();
        towerGroundList = GameObject.FindGameObjectsWithTag("TowerGround");
    }

    void Update()
    {
        if (photonView != null && !photonView.IsMine)
        {
            return;
        }
        CloseToTower();
        //Debug.Log(isNearTower);
        if (isNearTower)
        {
            if (inputHandler.TowerShopKeyDown)
            {

                if (!IngameUIManager.Inst.isTowerBuyUIActive)
                {
                    IngameUIManager.Inst.isTowerBuyUIActive = true;
                    int index = towerGroundMapping.GetIndexByName(towerGroundList[closestTowerGroundIndex].name);
                    //Debug.Log(towerGroundList[closestTowerGroundIndex] + ", index:" + index);
                    towerBuyUIScript.Initialize(towerGroundList[closestTowerGroundIndex], index);
                }
                else
                {
                    GlobalSoundManager.Instance.PlayUIActiveSound();
                    IngameUIManager.Inst.isTowerBuyUIActive = false;
                }
            }

        }
        else
        {
            if (IngameUIManager.Inst.isTowerBuyUIActive)
            {
                IngameUIManager.Inst.isTowerBuyUIActive = false;
            }
        }
    }

    public void CloseToTower()
    {
        for (int i = 0; i < towerGroundList.Length; i++)
        {
            float PlayerTowerDistance = (towerGroundList[i].transform.position - transform.position).sqrMagnitude;

            if (PlayerTowerDistance < 100f) // 제곱근 제거해서 10*10 으로 기준 거리 설정
            {
                isNearTower = true;
                closestTowerGroundIndex = i;
                towerInfoUI.SetActive(true);
                break; // 둘 이상의 가까운 타워는 없음 : 타워 간의 거리 > 타워 설치할 수 있는 거리 * 2
            }
            else
            {
                isNearTower = false;
                towerInfoUI.SetActive(false);
            }
        }
    }
}
