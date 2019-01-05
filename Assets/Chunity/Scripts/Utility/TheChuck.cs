using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheChuck : MonoBehaviour
{
    public static ChuckMainInstance instance = null;

    void Awake()
    {
        if( instance == null )
        {
            instance = GetComponent<ChuckMainInstance>();
        }
    }

    void OnDestroy()
    {
        if( instance == GetComponent<ChuckMainInstance>() )
        {
            instance = null;
        }
    }
}
