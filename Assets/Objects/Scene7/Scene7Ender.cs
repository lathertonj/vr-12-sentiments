using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene7Ender : MonoBehaviour
{
	public Color skyColor;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

	void OnTriggerExit( Collider other )
	{
		// something is leaving the main bubble of the world
		if( other.gameObject.CompareTag( "MainCamera" ) )
		{
			// silence chuck
			TheChuck.instance.BroadcastEvent( "scene7EndScene" );

			// soon, fade out
			Invoke( "DoFade", 3f );
		}
	}

	void DoFade()
	{
		SteamVR_Fade.Start( skyColor, duration: 3 );
		Invoke( "NextScene", 6 );
	}

	void NextScene()
	{
        UnityEngine.SceneManagement.SceneManager.LoadScene( "8_MelancholyWeak" );
	}
}
