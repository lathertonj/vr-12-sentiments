using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene7Ender2 : MonoBehaviour
{
	public Color skyColor;
	private static bool hasSceneEnded = false;
	public float timeToStayForEnd = 10f;

	float landTime;

	void OnTriggerEnter( Collider other )
	{
		if( other.gameObject.CompareTag( "MainCamera" ) )
		{
			landTime = Time.time;
			TheChuck.instance.SetInt( "scene7HappyFinishMode", 1 );
		}
	}

	void OnTriggerStay( Collider other )
	{
		// something is leaving the main bubble of the world
		if( !hasSceneEnded && other.gameObject.CompareTag( "MainCamera" ) )
		{
			if( Time.time - landTime > timeToStayForEnd )
			{
				hasSceneEnded = true;
				// silence chuck
				TheChuck.instance.BroadcastEvent( "scene7EndScene" );

				// soon, fade out
				Invoke( "DoFade", 3f );
			}
		}
	}

	void OnTriggerExit( Collider other )
	{
		if( !hasSceneEnded && other.gameObject.CompareTag( "MainCamera" ) )
		{
			// we were wrong and the scene is not actually ending
			TheChuck.instance.SetInt( "scene7HappyFinishMode", 0 );
		}
	}
	public ControllerAccessors leftHand, rightHand;

	void DoFade()
	{
		// visuals
		SteamVR_Fade.Start( skyColor, duration: 3 );
		
		// vibration
		leftHand.StopVibrating();
		rightHand.StopVibrating();
		
		// next scene
		Invoke( "NextScene", 6 );
	}

	void NextScene()
	{
        UnityEngine.SceneManagement.SceneManager.LoadScene( "8_MelancholyWeak" );
	}
}
