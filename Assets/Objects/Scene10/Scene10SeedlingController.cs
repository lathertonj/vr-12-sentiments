using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene10SeedlingController : MonoBehaviour
{
	Scene10WiggleSeedling[] mySeedlings;
	public ControllerAccessors leftHand, rightHand;
	private int currentSeedling = 0;
	private ChuckSubInstance myChuck;

    // Use this for initialization
    void Start()
    {
		mySeedlings = GetComponentsInChildren<Scene10WiggleSeedling>();
		myChuck = GetComponent<ChuckSubInstance>();
		ChuckEventListener myListener = gameObject.AddComponent<ChuckEventListener>();
		myListener.ListenForEvent( myChuck, "scene10NoteHappened", WiggleASeedling );
    }

    // Update is called once per frame
    void Update()
    {
		// too much vibration!!
		float wiggleAmount = 0;
		foreach( Scene10WiggleSeedling seedling in mySeedlings ) { wiggleAmount += seedling.GetWiggleAmount(); }
		ushort vibrationAmount = (ushort)( wiggleAmount.PowMapClamp( 0, mySeedlings.Length, 0, 500, pow: 0.5f ) );
		//leftHand.Vibrate( vibrationAmount );
		//rightHand.Vibrate( vibrationAmount );
    }

	void WiggleASeedling()
	{
		mySeedlings[currentSeedling].AnimateWiggle();

		currentSeedling++;
		currentSeedling %= mySeedlings.Length;
	}
}
