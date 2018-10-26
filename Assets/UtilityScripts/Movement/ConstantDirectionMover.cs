using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantDirectionMover : MonoBehaviour
{
    public float minSpeedWithoutStopping = 0.1f;
    public bool mostlyIgnoreMovementRequests = false;
    public float timeToReturnToOriginalDirection = 1f;
    public float percentOfRequestToConsider = 0.5f;

    private Vector3 currentDirection;
    private Vector3 requestedDirection;
    private float timeOfRequest;
    private float currentSpeed;
    // Use this for initialization
    void Awake()
    {
        currentDirection = Vector3.zero;
        requestedDirection = Vector3.zero;
        currentSpeed = 0;
        timeOfRequest = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 theDirection = currentDirection;
        float normTimeSinceRequest = ( Time.time - timeOfRequest ) / timeToReturnToOriginalDirection;
        if( mostlyIgnoreMovementRequests && normTimeSinceRequest < 1 )
        {
            theDirection = normTimeSinceRequest * currentDirection + ( 1 - normTimeSinceRequest ) * requestedDirection;
        }
		transform.position += Time.deltaTime * currentSpeed * theDirection;
    }

	public void SetDirection( Vector3 direction, float speed )
	{
        if( mostlyIgnoreMovementRequests )
        {
            requestedDirection = percentOfRequestToConsider * direction + ( 1 - percentOfRequestToConsider ) * currentDirection;
            timeOfRequest = Time.time;
            return;
        }
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
