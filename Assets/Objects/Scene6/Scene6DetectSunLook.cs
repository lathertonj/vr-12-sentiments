using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6DetectSunLook : MonoBehaviour
{

    public static float sunLookAmount = 0;
    public static float sunContinuousLookAmount = 0;
    public static bool currentlyLookingAtSun = false;
    public Transform theHead;
    public float ignoreLookTime = 40f;

    private bool continuousLookIsFirstHigh = true;
    public static int numLongContinuousLooks = 0;
    public static bool startCountingLongContinuousLooks = false;

	void Update()
    {
        // do nothing if we haven't waited enough
        if( Time.timeSinceLevelLoad < ignoreLookTime )
        {
            return;
        }

		RaycastHit hit;
        int layerMask = LayerMask.GetMask( "LookChordPlane" );
        currentlyLookingAtSun = false;
        if( Physics.Raycast( theHead.position, theHead.forward, out hit, Mathf.Infinity, layerMask ) )
        {
            if( hit.collider.gameObject == gameObject )
            {
                sunLookAmount += Time.deltaTime;
                sunContinuousLookAmount += Time.deltaTime;
                currentlyLookingAtSun = true;
            }
        }
        
        if( !currentlyLookingAtSun )
        {
            sunContinuousLookAmount = 0;
            continuousLookIsFirstHigh = true;
        }

        if( startCountingLongContinuousLooks && sunContinuousLookAmount > 5 && continuousLookIsFirstHigh )
        {
            continuousLookIsFirstHigh = false;
            numLongContinuousLooks++;
            Debug.Log( "long continuous look!" );
        }
    }
}
