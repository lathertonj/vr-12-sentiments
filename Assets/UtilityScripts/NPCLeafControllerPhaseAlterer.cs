using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCLeafControllerPhaseAlterer : MonoBehaviour
{
    public float upDelay = 3;
    void Awake()
    {
        foreach( NPCLeafController3 leaf in GetComponentsInChildren<NPCLeafController3>() )
        {
            leaf.upDelay = upDelay;
        }
    }
}
