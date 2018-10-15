using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncourageSqueezing : MonoBehaviour
{
	private static bool controllersHaveBeenSqueezed = false;
	private ushort currentVibrateIntensity = 0;
	private ControllerAccessors myController;

	public float shortCycleLength = 0.1f;
	public float shortCycleRest = 0.3f;
	public float longCycleRest = 1.1f;
	public int numShortCycles = 2;
	public ushort maxVibrateIntensity = 5;
	public float encouragementWaitTime = 10f;

    // Use this for initialization
    void Start()
    {
		myController = GetComponent<ControllerAccessors>();
		Invoke( "LaunchCoroutine", encouragementWaitTime );
    }

    // Update is called once per frame
    void Update()
    {
		// check if this hand is squeezing
		if( myController.IsFirstSqueezed() )
		{
			controllersHaveBeenSqueezed = true;
		}

		if( !controllersHaveBeenSqueezed )
		{
			myController.Vibrate( currentVibrateIntensity );
		}
		else
		{
			// completely remove all scripts after the first squeeze on any controller
			Destroy( this );
		}
    }

	void LaunchCoroutine()
	{
		StartCoroutine( "SqueezeIntensity" );
	}

	IEnumerator SqueezeIntensity() 
	{
		while( true )
		{
			for( int i = 0; i < numShortCycles; i++ )
			{
				currentVibrateIntensity = maxVibrateIntensity;
				yield return new WaitForSeconds( shortCycleLength );
				currentVibrateIntensity = 0;
				yield return new WaitForSeconds( shortCycleRest );
			}
			yield return new WaitForSeconds( longCycleRest - shortCycleLength );
		}
	}
}
