using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
using System;
public class ResultUI : MonoBehaviour
{
    
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _playTime;
    [SerializeField] private TextMeshProUGUI _nexusInfo;
    [SerializeField] private GameObject tableItemPrefab;
    [SerializeField] private Transform tableParent;

    public Button OkBtn;

    void Awake()
    {
        gameObject.SetActive(false); // 비활성화 상태로 시작
        OkBtn.onClick.AddListener(OnClickBtn);
    }
    void OnEnable()
    {
        LoadResultTable();
    }

    public void LoadResultTable()
    {
        foreach (Transform child in tableParent)
        {
            if (child.gameObject.name != "TableHeader")
            {
                Destroy(child.gameObject);
            }
        }
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            //Debug.Log($"{player.NickName}의 기록을 UI에 보여줍니다.");
            // 기존 테이블 초기화
            // tableItem 생성
            GameObject tableItem = Instantiate(tableItemPrefab);
            tableItem.transform.SetParent(tableParent, false); // 부모의 Transform 영향을 받지 않음
            string nickname = player.NickName;
            // 아래값들을 포톤서버의 Player CustomProperty로부터 가져온다 -- 다른 방법(전역 매니저에 저장)으로 하는 경우 수정 필요
            int classCode = PhotonManager.GetPlayerCustomProperty<int>("ClassCode", player, int.MinValue);
            float dealtDamage = PhotonManager.GetPlayerCustomProperty<float>("DealtDamage", player, float.MinValue);
            float healAmount = PhotonManager.GetPlayerCustomProperty<float>("HealAmount", player, float.MinValue);
            float takenDamage = PhotonManager.GetPlayerCustomProperty<float>("TakenDamage", player, float.MinValue);
            tableItem.GetComponent<ResultTableItem>()?.Initialize(classCode, nickname, dealtDamage, healAmount, takenDamage);
        }
    }

    public void SetTitle(string title)
    {
        _title.text = title;
    }

    public void SetPlayTime(float playTime)
    {
        int playTimeMin = (int)(playTime / 60);
        int playTimeSec = (int)(playTime % 60);
        _playTime.text = $"총 플레이 시간 : {playTimeMin:00}:{playTimeSec:00}";
    }

    public void SetPlayTime(string playTimeText)
    {
        _playTime.text = $"총 플레이 시간 : {playTimeText}";
    }

    public void SetNexusInfo(float nexusHpPercentage)
    {
        _nexusInfo.text = $"왕자 HP : {nexusHpPercentage}%";
    }

    private void OnClickBtn()
    {
        //Time.timeScale = 1f; // 시간 재개
        GameObject statisticsManager = GameObject.Find("StatisticsManager");
        Debug.Log(statisticsManager);
        Destroy(statisticsManager);
        //버튼 클릭 시 로직 추가
        PhotonManager.Inst.ExitGame();
        Debug.Log("클릭");
    }
}
