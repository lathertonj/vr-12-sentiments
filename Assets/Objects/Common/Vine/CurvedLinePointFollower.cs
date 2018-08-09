using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurvedLinePointFollower : MonoBehaviour
{
    public Transform objectToFollow;
    public Vector3 offsetFromObject;

    // Update is called once per frame
    void Update()
    {
        transform.position = objectToFollow.position + offsetFromObject;
    }
}
