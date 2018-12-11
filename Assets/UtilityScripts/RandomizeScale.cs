using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeScale : MonoBehaviour
{
	public float minScale = 1f;
	public float maxScale = 1.3f;

    // Use this for initialization
    void Start()
    {
		transform.localScale *= Random.Range( minScale, maxScale );
    }

    
}
