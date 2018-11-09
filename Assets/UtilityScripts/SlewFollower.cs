using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlewFollower : MonoBehaviour
{

    public Transform objectToFollow;
    public float slew;
    private Vector3 currentPos, goalPos;

    // Use this for initialization
    void Start()
    {
        // unparent ourselves so we can move on our own
        transform.parent = null;
        // set position
        transform.position = currentPos = goalPos = objectToFollow.position;
    }

    // Update is called once per frame
    void Update()
    {
        // slew position toward object we're following
        goalPos = objectToFollow.position;
        currentPos += Time.deltaTime * slew * ( goalPos - currentPos );
        transform.position = currentPos;
    }
}
