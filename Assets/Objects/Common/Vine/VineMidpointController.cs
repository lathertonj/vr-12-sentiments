using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineMidpointController : MonoBehaviour
{
    public Transform myHead;
    public Transform myRoot;
    private Vector3 myRootOffset; // aka my position in the coordinate space where root is at 0 
    private Vector3 theHeadOffset;
    private Vector3 myNearestPointOnHeadRootAxis;
    private Vector3 myOffsetFromHeadRootAxis;
    private float theHeadOriginalDistance;
    private float myFractionUpTheHeadRootAxis;

    // This all assumes the root does not move! What if the root moves? (It might still be ok!)
    void Start()
    {
        myRootOffset = transform.position - myRoot.position;
        theHeadOffset = myHead.position - myRoot.position;
        theHeadOriginalDistance = theHeadOffset.magnitude;
        myNearestPointOnHeadRootAxis = Vector3.Project( myRootOffset, theHeadOffset );
        myOffsetFromHeadRootAxis = myRootOffset - myNearestPointOnHeadRootAxis;
        myFractionUpTheHeadRootAxis = myNearestPointOnHeadRootAxis.magnitude / theHeadOriginalDistance;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 theHeadCurrentOffset = myHead.position - myRoot.position;
        // find where I hypothetically "connect" on the virtual vine
        Vector3 myCurrentHeadRootAxisPoint = myFractionUpTheHeadRootAxis * theHeadCurrentOffset;
        // how much the head is lowered (or raised) below its normal length 
        float theHeadSquashedFraction = theHeadCurrentOffset.magnitude / theHeadOriginalDistance;
        // my point extends outward inversely proportional to how much the head is lowered
        transform.position = myRoot.position + myCurrentHeadRootAxisPoint + 
            myOffsetFromHeadRootAxis * ( 1.0f / theHeadSquashedFraction );

    }
}
