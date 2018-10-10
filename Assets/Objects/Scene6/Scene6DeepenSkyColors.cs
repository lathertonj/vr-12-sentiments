using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6DeepenSkyColors : MonoBehaviour
{


    public Color firstColor, secondColor;
    public float switchTime = 3f;
    private ChuckEventListener myListener;
	public ParticleSystem myFascinator;

    // Use this for initialization
    void Start()
    {
        // on first broadcast color changing chord change, also change the sky color
        myListener = gameObject.AddComponent<ChuckEventListener>();
        myListener.ListenForEvent( GetComponent<ChuckSubInstance>(), "ahhChordChange", DeepenSkyColors );

		// set it to begin with
		SetSkyboxColor( firstColor );
    }

    void DeepenSkyColors()
    {
        // do the fade 
        StartCoroutine( "DoFade" );

        // only call this once
        myListener.StopListening();
    }

    IEnumerator DoFade()
    {
        float startTime = Time.time;
		ParticleSystem.MainModule mainFascinator = myFascinator.main;

        while( Time.time < startTime + switchTime )
        {
            float normElapsedTime = ( Time.time - startTime ) / switchTime;
            // just straight up interp
            Color theColor = firstColor + normElapsedTime * ( secondColor - firstColor );
            RenderSettings.skybox.color = theColor;

			// set the skybox
            SetSkyboxColor( theColor );

			// set the fascinator
			mainFascinator.startColor = theColor;

            yield return null;

        }
        // finally, make sure it ends on the correct color
        SetSkyboxColor( secondColor );
		
		// set the fascinator
		mainFascinator.startColor = secondColor;
    }

	void SetSkyboxColor( Color c )
	{
		if( RenderSettings.skybox.HasProperty( "_Tint" ) )
        {
            RenderSettings.skybox.SetColor( "_Tint", c );
        }
	    else if( RenderSettings.skybox.HasProperty( "_SkyTint" ) )
		{
			RenderSettings.skybox.SetColor( "_SkyTint", c );
		}
	}	
}
