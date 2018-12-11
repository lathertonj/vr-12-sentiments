using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoScene1011Transition : MonoBehaviour
{
	public Transform room;
	public GameObject world10, world11;
	public Vector3 newRoomLocation;
	public float newRoomRelativeScale = 0.5f;

	public Material firstSkybox, secondSkybox;

	public Scene10SeedlingController parentOfSeedlings;

	private ChuckSubInstance myChuck;
	private ChuckEventListener mySwitchListener;

    // Use this for initialization
    void Start()
    {
		RenderSettings.skybox = firstSkybox;
		myChuck = GetComponent<ChuckSubInstance>();
		mySwitchListener = gameObject.AddComponent<ChuckEventListener>();
		mySwitchListener.ListenForEvent( myChuck, "scene10AdvanceToScene11", DoTransition );
    }

    // Update is called once per frame
    void Update()
    {

    }

	void DoTransition()
	{
		world10.SetActive( false );
		world11.SetActive( true );
		room.position = newRoomLocation;
		room.localScale *= newRoomRelativeScale;
		RenderSettings.skybox = secondSkybox;
		parentOfSeedlings.SwitchToScene11();

		mySwitchListener.StopListening();
	}
}
