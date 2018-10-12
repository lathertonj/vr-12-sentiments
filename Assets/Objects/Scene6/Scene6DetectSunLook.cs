using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6DetectSunLook : MonoBehaviour
{

    public static float sunLookAmount = 0;
    public static float sunContinuousLookAmount = 0;
    public static bool currentlyLookingAtSun = false;
    public Transform theHead;

	void Update()
    {
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
        }
    }
}
