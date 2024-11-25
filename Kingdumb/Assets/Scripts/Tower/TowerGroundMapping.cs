using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ground의 이름과 인덱스를 매핑하기 위한 클래스
public class TowerGroundMapping
{
    private Dictionary<string, int> _towerGroundToIndex;

    public TowerGroundMapping()
    {
        _towerGroundToIndex = new Dictionary<string, int>();

        for (int i = 1; i <= 10; i++) // TowerGround 1 ~ TowerGround 10
        {
            _towerGroundToIndex.Add($"TowerGround {i}", i - 1); // 이름: "TowerGround 1" -> 값: 0
        }
    }

    public int GetIndexByName(string towerGroundName)
    {
        if (_towerGroundToIndex.TryGetValue(towerGroundName, out int index))
        {
            return index;
        }
        return -1; // 이름이 없으면 -1 반환
    }
}
