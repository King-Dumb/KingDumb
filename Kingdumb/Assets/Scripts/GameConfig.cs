using System.Collections.Generic;
using UnityEngine;
using static GameConfig;

public static class GameConfig
{
    //게임 버전 
    public static readonly string GameVersion = "1.0";

    //화면 해상도
    public static readonly int Width = 2560;
    public static readonly int Height = 1440;

    //최대 가능 인원수
    public static readonly int MaxPlayersInRoom = 4;

    // 난수
    public static System.Random rand = new System.Random();

    // 유저 닉네임
    public static string UserNickName = string.Empty;

    // 유저 컬러
    public static string UserColor = string.Empty;

    //프레임 30, 60, 144 중 세팅
    public static readonly int FPS = 144;

    //서버 응답 대기 시간
    public static readonly float ServerWaitingTime = 10f;

    //방 이름 최대 길이
    public static readonly int maxRoomNameLength = 20;

    public static readonly string playerColor = "playerColor"; //플레이어 색상

    //씬 종류    
    //public static string titleScene = "Title";
    //public static readonly string lobbyScene = "Lobby";
    //public static readonly string roomScene = "Room";
    //public static readonly string inGameScene = "InGame";
    //public static readonly string loadingScene = "Loading";

    //맵의 개수
    //public static int mapCount = 3;

    public const string WarriorClass = "Warrior";
    public const string ArcherClass = "Archer";
    public const string MageClass = "Mage";
    public const string PriestClass = "Priest";

    public const int maxSkillLevel = 15;

    public static string GenerateRoomCode(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        char[] uid = new char[length];

        for (int i = 0; i < length; i++)
        {
            uid[i] = chars[rand.Next(chars.Length)];
        }
        return new string(uid);
    }

    //방 이름과 방 코드를 분리하는 함수
    public static string[] ParseString(string roomName)
    {
        string[] parts = roomName.Split('#');

        //반드시 2개로 나눠지기
        if (parts.Length == 2)
        {
            return parts;
        }

        //Debug.Log("방 이름 파싱 오류! : " + roomName);
        return null;
    }

    //n부터 m까지의 수를 섞는 함수
    public static List<int> GetShuffledList(int n, int m)
    {
        List<int> list = new List<int>();

        for (int i = n; i <= m; i++)
        {
            list.Add(i);
        }

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rand.Next(0, i + 1);

            int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }

        return list;
    }

    // 한글 여부 확인 (유니코드 범위: U+AC00 ~ U+D7AF)
    public static bool IsKorean(char c)
    {
        return
        (c >= 0xAC00 && c <= 0xD7A3) || // 완성형 한글
        (c >= 0x1100 && c <= 0x115F) || // 초성
        (c >= 0x1160 && c <= 0x11A7) || // 중성
        (c >= 0x3131 && c <= 0x318E);   // 한글 호환 자모 (ㄱ, ㄲ, ㄴ, ㄷ, 등)
    }

    // 영문 여부 확인 (대문자: A-Z, 소문자: a-z)
    public static bool IsEnglish(char c)
    {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    // 숫자 여부 확인 ('0' ~ '9')
    public static bool IsNumber(char c)
    {
        return (c >= '0' && c <= '9');
    }

    public static string GetSceneName(SceneName scene)
    {
        return scene.ToString();
    }
}
