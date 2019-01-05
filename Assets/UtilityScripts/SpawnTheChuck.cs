using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnTheChuck : MonoBehaviour
{
    public Transform theChuckPrefab;

    void Awake()
    {
		if( TheChuck.instance == null )
		{
			Instantiate( theChuckPrefab );
		}
    }
}
