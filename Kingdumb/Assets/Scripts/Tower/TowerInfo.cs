using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerInfo
{
    // 공격력, 투사체 속도, 공격 사거리, 공격 쿨타임, 타워 지속시간
    // 80 120 60 40 >> 20 20 15 10
    // 이펙트 프레임 때문에 속도 절반으로 줄여놓음
    private static float[,] allTowerData = new float[4, 5]{
      {10f, 25f, 250f, 3f, 180f}, // 전사타워
      {15f, 30f, 300f, 3f, 180f}, // 궁수타워
      {10f, 25f, 200f, 3f, 180f}, // 마법사타워
      {10f, 0f, 100f, 3f, 180f} // 힐러타워
    };

    // 전사 1단계 : 3, 3, 3, 3
    // 전사 2단계 : 4, 3, 3, 3
    // 전사 3단계 : 4, 3, 4, 3

    // 궁수 1단계 : 4, 4, 4, 3
    // 궁수 2단계 : 4, 4, 5, 3
    // 궁수 3단계 : 4, 4, 5, 4

    // 마법사 1단계 : 3, 3, 2, 3
    // 마법사 2단계 : 3, 3, 3, 3
    // 마법사 3단계 : 4, 3, 3, 3

    // 사제 1단계 : 3, -, 1, 3
    // 사제 2단계 : 3, -, 1, 4
    // 사제 3단계 : 4, -, 1, 4

    // [업그레이드 단계][타워 종류][(업그레이드 종류, 업그레이드 정도)]
    private static float[,,] towerUpgrade = new float[2, 4, 2]{
        // 1단계로 업그레이드 후
        {
            {0f, 5f},
            {2f, 50f},
            {2f, 50f},
            {3f, -1f},
        },
        
        // 2단계로 업그레이드 후
        {
            {2f, 50f},
            {3f, -1f},
            {0f, 5f},
            {0f, 5f},
        },
    };

    public static float[] GetTowerInfo(int towerType, int towerLevel)
    {
        float[] towerInfo = new float[5];

        towerInfo[0] = allTowerData[towerType, 0];
        towerInfo[1] = allTowerData[towerType, 1];
        towerInfo[2] = allTowerData[towerType, 2];
        towerInfo[3] = allTowerData[towerType, 3];
        towerInfo[4] = allTowerData[towerType, 4];

        // 타워 레벨에 따른 공격력, 지속시간, 사거리, 공격주기 설정
        if (towerLevel > 0)
        {
            int abilityToUpgrade = (int)towerUpgrade[0, towerType, 0];
            float amountToUpgrade = towerUpgrade[0, towerType, 1];
            towerInfo[abilityToUpgrade] += amountToUpgrade;

            if (towerLevel > 1)
            {
                abilityToUpgrade = (int)towerUpgrade[1, towerType, 0];
                amountToUpgrade = towerUpgrade[1, towerType, 1];
                towerInfo[abilityToUpgrade] += amountToUpgrade;
            }
        }

        return towerInfo;
    }

    public static int[] GetTowerRateByIndex(int towerType, int towerLevel)
    {
        float[] towerInfo = GetTowerInfo(towerType, towerLevel);
        int[] returnInfo = new int[towerInfo.Length];

        for (int i = 0; i < 4; i++)
        {
            returnInfo[i] = GetTowerRateByAbility(i, (int)towerInfo[i]);
        }

        return returnInfo;
    }

    private static int GetTowerRateByAbility(int towerAbilityType, int abilityAmount)
    {
        int towerRate = 0;

        switch (towerAbilityType)
        {
            case 0: // 공격력
                towerRate = abilityAmount > 10 ? 4 : 3;
                break;
            case 1: // 투사체 속도
                towerRate = abilityAmount > 25 ? 4 : 3;
                break;
            case 2: // 사거리
                int temp = (abilityAmount - 100) / 50;
                towerRate = temp == 0 ? 1 : temp;
                break;
            case 3:
                towerRate = abilityAmount > 2 ? 3 : 4;
                break;
        }

        return towerRate;
    }

}
