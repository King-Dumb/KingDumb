using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatisticsManager : MonoBehaviour
{
    public static PlayerStatisticsManager Instance;

    void Awake()
    {
        //싱글톤 선언
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 씬 변경에도 오브젝트 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Dictionary<Player, PlayerStatistics> totalPlayerStatistics = new Dictionary<Player, PlayerStatistics>();

    public void InitializePlayerStatistics()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!totalPlayerStatistics.ContainsKey(player))
            {
                totalPlayerStatistics[player] = new PlayerStatistics(); // 기본값 생성
                //Debug.Log($"Initialized statistics for player: {player.NickName}");
            }
        }
    }


    // PlayerStatistics 가져오기 또는 초기화
    private PlayerStatistics GetOrCreateTotalStatistics(Player player)
    {
        if (!totalPlayerStatistics.ContainsKey(player))
        {
            totalPlayerStatistics[player] = new PlayerStatistics();
        }
        return totalPlayerStatistics[player];
    }

    // 딜량 기록
    public void RecordDealtDamage(int viewID, float damage)
    {
        var player = GetPlayerFromViewID(viewID);
        if (player != null)
        {
            var stats = GetOrCreateTotalStatistics(player);
            stats.DealtDamage += damage;
            //Debug.Log($"Player {player.NickName} - Total Dealt Damage: {stats.DealtDamage}");
        }
        // else
        // {
        //     //Debug.LogWarning($"Player not found for ViewID: {viewID}");
        // }
    }

    // 힐량 기록
    public void RecordHealedAmount(int viewID, float healAmount)
    {
        //Debug.Log("RecordHealedAmount 호출");
        var player = GetPlayerFromViewID(viewID);
        if (player != null)
        {
            var stats = GetOrCreateTotalStatistics(player);
            stats.Healed += healAmount;
            //Debug.Log($"Player {player.NickName} - Total Healed Amount: {stats.Healed}");
        }
        else
        {
            //Debug.LogWarning($"Player not found for ViewID: {viewID}");
        }
    }

    // 받은 피해량 기록
    public void RecordTakenDamage(int viewID, float damage)
    {
        var player = GetPlayerFromViewID(viewID);
        if (player != null)
        {
            var stats = GetOrCreateTotalStatistics(player);
            stats.TakenDamage += damage;
            //Debug.Log($"Player {player.NickName} - Total Taken Damage: {stats.TakenDamage}");
        }
        // else
        // {
        //     //Debug.LogWarning($"Player not found for ViewID: {viewID}");
        // }
    }

    // ViewID로 Player 객체 가져오기
    //private Player GetPlayerFromViewID(int viewID)
    //{
    //    //Debug.Log("가져오려는 viewID는: " + viewID);
    //    PhotonView targetPhotonView = PhotonNetwork.GetPhotonView(viewID);
    //    if (targetPhotonView != null)
    //    {
    //        return targetPhotonView.Owner;
    //    }
    //    return null;
    //}

    private Player GetPlayerFromViewID(int viewID)
    {
        PhotonView targetPhotonView = PhotonNetwork.GetPhotonView(viewID);
        if (targetPhotonView != null)
        {
            GameObject targetObject = targetPhotonView.gameObject;
            Debug.Log("받은 오브젝트의 태그는:" + targetObject.tag);
            if (targetObject.CompareTag("Player"))  // Player 태그 확인
            {
                return targetPhotonView.Owner;
            }
        }
        return null;
    }



    // 게임이 끝나면(이기거나 지거나) 호출
    public void SetPlayerStatisticsWhenGameEnded()
    {
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            PlayerStatistics stats = GetPlayerStatistics(player);
            UpdatePlayerStatistics(player, stats);  
        }
    }

    // 특정 Player의 통계 조회
    public PlayerStatistics GetPlayerStatistics(Player player)
    {
        if (totalPlayerStatistics.TryGetValue(player, out var stats))
        {
            return stats;
        }
        return null;
    }

    public void UpdatePlayerStatistics(Player player, PlayerStatistics stats)
    {
        ExitGames.Client.Photon.Hashtable statsTable = new ExitGames.Client.Photon.Hashtable
        {
            { "DealtDamage", stats.DealtDamage },
            { "HealAmount", stats.Healed },
            { "TakenDamage", stats.TakenDamage }
        };
        player.SetCustomProperties(statsTable);
    }
}


public class PlayerStatistics
{
        public float DealtDamage { get; set; }
        public float Healed { get; set; }
        public float TakenDamage { get; set; }
}
