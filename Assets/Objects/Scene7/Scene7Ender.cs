using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        // UnityEngine.SceneManagement.SceneManager.LoadScene( "8_MelancholyWeak" );
		StartCoroutine( "LoadSceneAsync" );
	}

    IEnumerator LoadSceneAsync()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync( "8_MelancholyWeak" );

        // Wait until the asynchronous scene fully loads
        while( !asyncLoad.isDone )
        {
            yield return null;
        }
    }
}
