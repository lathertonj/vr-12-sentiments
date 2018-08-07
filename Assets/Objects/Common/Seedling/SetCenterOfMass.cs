using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetCenterOfMass : MonoBehaviour
{

    public Vector3 centerOfMass;

    // Use this for initialization
    void Start()
    {
        GetComponent<Rigidbody>().centerOfMass = centerOfMass;
    }
}
