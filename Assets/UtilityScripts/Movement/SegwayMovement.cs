using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegwayMovement : MonoBehaviour
{

    public Transform room, leftHand, rightHand;
	public float speed = 5f;
    private Vector3 initialOffset;
    // Use this for initialization
    void Start()
    {
		initialOffset = ComputeOffset();
    }

    // Update is called once per frame
    void Update()
    {
		Vector3 currentOffset = ComputeOffset();
		Vector3 offsetDifference = currentOffset - initialOffset;
		offsetDifference.y = 0;
		room.position += Time.deltaTime * speed * offsetDifference;
    }

	Vector3 ComputeOffset()
	{
		return transform.position - 0.5f * ( leftHand.position + rightHand.position );
	}
}
