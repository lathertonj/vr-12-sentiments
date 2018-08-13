using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineEndJointController : MonoBehaviour
{
    public Transform myRoot;
    public Transform myThingToFollow;

    private LinkedList<VineMidJointController> myMidpoints;
    private Vector3 myOriginalOffset;
    private float myOriginalDistance;

    // Use this for initialization
    void Start()
    {
        myMidpoints = new LinkedList<VineMidJointController>();
        // calculate some properties
        myOriginalOffset = transform.position - myRoot.position;
        myOriginalDistance = myOriginalOffset.magnitude;

        Transform maybeMidpoint = transform.parent;
        while( maybeMidpoint != myRoot )
        {
            VineMidJointController midPoint = maybeMidpoint.GetComponent<VineMidJointController>();
            
            // calculate some properties
            Vector3 midPointOffset = maybeMidpoint.position - myRoot.position;
            midPoint.myNearestPointOnHeadRootAxis = Vector3.Project( midPointOffset, myOriginalOffset );
            midPoint.myOffsetFromHeadRootAxis = midPointOffset - midPoint.myNearestPointOnHeadRootAxis;
            midPoint.myFractionUpTheHeadRootAxis = midPoint.myNearestPointOnHeadRootAxis.magnitude / myOriginalDistance;

            // track it for later
            myMidpoints.AddFirst( midPoint );

            // traverse up
            maybeMidpoint = maybeMidpoint.parent;
        }
        

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 myNewPosition = myThingToFollow.position;
        Vector3 currentVineOffset = myNewPosition - myRoot.position;
        
        // calculate new position for each midpoint
        foreach( VineMidJointController midPoint in myMidpoints )
        {
            // find where I hypothetically "connect" on the virtual vine
            Vector3 midPointCurrentHeadRootAxisPoint = midPoint.myFractionUpTheHeadRootAxis * currentVineOffset;
            // how much the head is lowered (or raised) below its normal length 
            float theHeadSquashedFraction = currentVineOffset.magnitude / myOriginalDistance;
            // my point extends outward inversely proportional to how much the head is lowered
            midPoint.transform.position = myRoot.position + midPointCurrentHeadRootAxisPoint + 
                midPoint.myOffsetFromHeadRootAxis * ( 1.0f / theHeadSquashedFraction );
        }

        // finally, set my position
        transform.position = myNewPosition;
    }
}
