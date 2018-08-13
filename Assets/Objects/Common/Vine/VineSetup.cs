using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineSetup : MonoBehaviour
{
    public Transform headToTrack, armToTrack1, armToTrack2;
    public VineEndJointController myHeadJoint, myArmJoint1, myArmJoint2;

    // Use this for initialization
    void Awake()
    {
        myHeadJoint.myThingToFollow = headToTrack;
        myArmJoint1.myThingToFollow = armToTrack1;
        myArmJoint2.myThingToFollow = armToTrack2;
    }
}
