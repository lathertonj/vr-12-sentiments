using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene10RoofRevealer : MonoBehaviour
{
	ChuckSubInstance myChuck;
	ChuckIntSyncer currentNote;

	private float currentAlpha, goalAlpha, alphaSlew;
	// alpha should go from 0.3 to 0.7
	private Color originalColor;
	private MeshRenderer myRenderer;

    // Use this for initialization
    void Start()
    {
		myChuck = GetComponent<ChuckSubInstance>();
		currentNote = gameObject.AddComponent<ChuckIntSyncer>();
		currentNote.SyncInt( myChuck, "numNotesPlayed" );

		alphaSlew = 0.03f;
		currentAlpha = goalAlpha = 0.7f;

		myRenderer = GetComponent<MeshRenderer>();
		originalColor = myRenderer.material.color;
    }

    // Update is called once per frame
    void Update()
    {
		// triangle wave
		int currentValue = currentNote.GetCurrentValue();
		if( currentValue < 3.5 )
		{
			goalAlpha = ( (float) currentNote.GetCurrentValue() ).PowMapClamp( 0, 3.5f, 0.8f, 0.3f, pow: 1.1f );
		}
		else
		{
			goalAlpha = ( (float) currentNote.GetCurrentValue() ).PowMapClamp( 3.5f, 7, 0.3f, 0.8f, pow: 1/1.1f );
		}

		// slew
		currentAlpha += alphaSlew * ( goalAlpha - currentAlpha );

		Debug.Log( "alpha is: " + currentAlpha.ToString() );
		myRenderer.material.color = new Color( originalColor.r, originalColor.g, originalColor.b, currentAlpha );
    }
}
