using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene9CalibrateHeight : MonoBehaviour
{
	public Transform head;
	public float verticalOffset = 0.0f;

    // Use this for initialization
    void Start()
    {
		// make the head be where the feet currently are by moving me down by the local position of the head
		transform.position += ( verticalOffset - head.localPosition.y ) * Vector3.up;
    }
}
