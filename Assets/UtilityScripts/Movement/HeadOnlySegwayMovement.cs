using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadOnlySegwayMovement : MonoBehaviour
{
	public Transform room;
	public float speed = 5f;
	private Vector3 offset;

    // Use this for initialization
    void Start()
    {
		offset = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 movementDirection = transform.localPosition;
		movementDirection.y = 0;
		room.position += Time.deltaTime * speed * movementDirection;
    }
}
