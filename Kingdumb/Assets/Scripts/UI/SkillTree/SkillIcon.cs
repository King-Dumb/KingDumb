using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkillIcon : MonoBehaviour
{
    public List<Sprite> warriorSkillIcon;
    public List<Sprite> mageSkillIcon;
    public List<Sprite> archerSkillIcon;
    public List<Sprite> priestSkillIcon;

    //GameConfig의 이름을 받아옴
    public List<Sprite> GetIconList(string playerClass)
    {
        switch (playerClass)
        {
            case GameConfig.WarriorClass:
                return warriorSkillIcon;
            case GameConfig.ArcherClass:
                return archerSkillIcon;
            case GameConfig.PriestClass:
                return priestSkillIcon;
            case GameConfig.MageClass:
                return mageSkillIcon;        
        }
        return null;
    }
}
