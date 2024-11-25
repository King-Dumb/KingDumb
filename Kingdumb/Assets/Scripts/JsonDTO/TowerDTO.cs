using System.Collections.Generic;

[System.Serializable]
public class TowerData
{
    public int level;          // 타워 레벨
    public string towerName;   // 타워 이름
    public string towerInfo;   // 타워 설명
    public int requiredGold;   // 레벨업에 필요한 골드
}

[System.Serializable]
public class TowerJsonInfos
{
    public TowerType towerType;            // 전사, 궁수, 마법사, 사제 (enum 사용)
    public List<TowerData> towerData;   // 타워 레벨별 데이터
}

[System.Serializable]
public class TowerList
{
    public List<TowerJsonInfos> towers; // 전체 타워 데이터 리스트
}
public enum TowerType
{
    Warrior = 0,
    Archer = 1,
    Mage = 2,
    Priest = 3
}