using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeTestManager : MonoBehaviour
{
    
    private ISkillTree skillTree;
    void Start()
    {
        skillTree = GetComponent<ISkillTree>();
        //Debug.Log("skillTree 초기화: " + skillTree);
    }

    // Update is called once per frame
    void Update()
    { 
        if (Input.GetKeyDown(KeyCode.F1))
        {
            skillTree.activateNode(1);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            skillTree.activateNode(2);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            skillTree.activateNode(3);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            skillTree.activateNode(4);
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            skillTree.activateNode(5);
        }
        if (Input.GetKeyDown(KeyCode.F6))
        {
            skillTree.activateNode(6);
        }
        if (Input.GetKeyDown(KeyCode.F7))
        {
            skillTree.activateNode(7);
        }
        if (Input.GetKeyDown(KeyCode.F8))
        {
            skillTree.activateNode(8);
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            skillTree.activateNode(9);
        }
        if (Input.GetKeyDown(KeyCode.F10))
        {
            skillTree.activateNode(10);
        }
        if (Input.GetKeyDown(KeyCode.F11))
        {
            skillTree.activateNode(11);
        }
        if (Input.GetKeyDown(KeyCode.F12))
        {
            skillTree.activateNode(12);
        }
    }
}
