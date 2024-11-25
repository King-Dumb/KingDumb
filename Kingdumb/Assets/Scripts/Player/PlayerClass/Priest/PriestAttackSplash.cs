using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriestAttackSplash : MonoBehaviour
{
    void Start()
    {
        GameManager.Instance.Destroy(gameObject, 1f);
    }
}
