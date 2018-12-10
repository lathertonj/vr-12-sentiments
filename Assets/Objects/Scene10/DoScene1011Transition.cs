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

    // Use this for initialization
    void Start()
    {
		RenderSettings.skybox = firstSkybox;

		Invoke( "DoTransition", 10 );
    }

    // Update is called once per frame
    void Update()
    {

    }

	public void DoTransition()
	{
		world10.SetActive( false );
		world11.SetActive( true );
		room.position = newRoomLocation;
		room.localScale *= newRoomRelativeScale;
		RenderSettings.skybox = secondSkybox;
		parentOfSeedlings.SwitchToScene11();
	}
}
