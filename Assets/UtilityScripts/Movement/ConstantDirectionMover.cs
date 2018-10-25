﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantDirectionMover : MonoBehaviour
{
    public float minSpeedWithoutStopping = 0.1f;

    private Vector3 currentDirection;
    private float currentSpeed;
    // Use this for initialization
    void Start()
    {
        currentDirection = Vector3.zero;
        currentSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
		transform.position += Time.deltaTime * currentSpeed * currentDirection;
    }

	public void SetDirection( Vector3 direction, float speed )
	{
		currentDirection = direction;
		currentSpeed = ( speed > minSpeedWithoutStopping ) ? speed : 0;
	}

	public Vector3 GetDirection()
	{
		return currentDirection;
	}

    public float GetSpeed()
    {
        return currentSpeed;
    }
}
