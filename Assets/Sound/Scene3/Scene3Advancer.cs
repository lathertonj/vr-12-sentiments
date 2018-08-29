using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene3Advancer : MonoBehaviour
{
    public FlowerAddSeedlings2 leftHand, rightHand;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    bool haveAdvanced = false;
    void Update()
    {
        if( !haveAdvanced && leftHand.numLongPresses >= 2 && rightHand.numLongPresses >= 2 )
        {
            haveAdvanced = true; 
            // advance!
            float period = 0.17f;
            float timeToStart = 0;
            leftHand.StartSpewing( timeToStart, period );
            rightHand.StartSpewing( timeToStart + period / 2, period );
        }
    }
}
