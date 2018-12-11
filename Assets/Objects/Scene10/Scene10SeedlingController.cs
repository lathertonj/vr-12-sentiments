using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene10SeedlingController : MonoBehaviour
{
	Scene10WiggleSeedling[] mySeedlings;
	public ControllerAccessors leftHand, rightHand;
	private int currentSeedling = 0;
	private ChuckSubInstance myChuck;
	private ChuckEventListener mySeedlingListener;
	private ParticleSystem myParticleEmitter;

    // Use this for initialization
    void Start()
    {
		mySeedlings = GetComponentsInChildren<Scene10WiggleSeedling>();
		myParticleEmitter = GetComponentInChildren<ParticleSystem>();
		myChuck = GetComponent<ChuckSubInstance>();
		mySeedlingListener = gameObject.AddComponent<ChuckEventListener>();
		mySeedlingListener.ListenForEvent( myChuck, "scene10NoteHappened", WiggleASeedling );
    }

    // Update is called once per frame
    void Update()
    {
		
    }

	void VibrateTickClosestHand( Vector3 position )
	{
		float leftDist = ( leftHand.transform.position - position ).sqrMagnitude;
		float rightDist = ( rightHand.transform.position - position ).sqrMagnitude;
		if( leftDist < rightDist )
		{
			leftHand.Vibrate( 500 );
		}
		else
		{
			rightHand.Vibrate( 500 );
		}
	}

	void AnimateParticle( Vector3 position )
	{
		// animate particle
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = myParticleEmitter.transform.InverseTransformPoint( position );
        emitParams.velocity = 0.15f * Vector3.up;
        myParticleEmitter.Emit( emitParams, count: 1 );
	}

	void TooMuchVibration()
	{
		// too much vibration!!
		float wiggleAmount = 0;
		foreach( Scene10WiggleSeedling seedling in mySeedlings ) { wiggleAmount += seedling.GetWiggleAmount(); }
		ushort vibrationAmount = (ushort)( wiggleAmount.PowMapClamp( 0, mySeedlings.Length, 0, 500, pow: 0.5f ) );
		leftHand.Vibrate( vibrationAmount );
		rightHand.Vibrate( vibrationAmount );
	}

	void WiggleASeedling()
	{
		// animate
		Vector3 position = mySeedlings[currentSeedling].transform.position;
		mySeedlings[currentSeedling].AnimateWiggle();
		AnimateParticle( position );

		// haptic feedback
		VibrateTickClosestHand( position );

		// prepare for next one
		currentSeedling++;
		currentSeedling %= mySeedlings.Length;
	}

	public void SwitchToScene11()
	{
		mySeedlingListener.StopListening();

		foreach( Transform seedling in transform )
		{
			seedling.gameObject.SetActive( false );
		}
	}
}
