
using System.Collections.Generic;

[System.Serializable]
public class Skill
{
    public string skillName;
    public string skillInfo;
}

[System.Serializable]
public class SkillList
{
    public List<Skill> skills;
}
