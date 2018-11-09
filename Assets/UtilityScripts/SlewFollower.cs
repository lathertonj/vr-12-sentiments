using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlewFollower : MonoBehaviour
{

    public Transform objectToFollow;
    public float slew;
    private Vector3 currentPos, goalPos;
    public Vector3 oscillateAmount;
    public Vector3 oscillateRate;
    private Vector3 oscillatePhase;

    // Use this for initialization
    void Start()
    {
        // unparent ourselves so we can move on our own
        transform.parent = null;
        // set position
        transform.position = currentPos = goalPos = objectToFollow.position;

        // oscillation
        // randomize rate
        oscillateRate.x *= Random.Range( 0.7f, 1.3f );
        oscillateRate.y *= Random.Range( 0.7f, 1.3f );
        oscillateRate.z *= Random.Range( 0.7f, 1.3f );

        // phase
        oscillatePhase = Vector3.zero;
        oscillatePhase.x = Random.Range( 0, oscillateRate.x );
        oscillatePhase.y = Random.Range( 0, oscillateRate.y );
        oscillatePhase.z = Random.Range( 0, oscillateRate.z );
    }

    // Update is called once per frame
    void Update()
    {
        // slew position toward object we're following
        goalPos = objectToFollow.position;
        
        // plus oscillation
        goalPos += new Vector3(
            oscillateAmount.x * Mathf.Sin( 2 * Mathf.PI * oscillateRate.x * Time.time + oscillatePhase.x ),
            oscillateAmount.y * Mathf.Sin( 2 * Mathf.PI * oscillateRate.y * Time.time + oscillatePhase.y ),
            oscillateAmount.z * Mathf.Sin( 2 * Mathf.PI * oscillateRate.z * Time.time + oscillatePhase.z )
        );

        currentPos += slew * ( goalPos - currentPos );
        transform.position = currentPos;
    }

}
