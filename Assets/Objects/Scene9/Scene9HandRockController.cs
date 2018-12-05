using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene9HandRockController : MonoBehaviour
{
	private ControllerAccessors myController;

    // Use this for initialization
    void Start()
    {
		myController = GetComponent<FollowObject>().objectToFollow.GetComponent<ControllerAccessors>();
    }

    public void InformHit()
	{
		myController.Vibrate( 500 );
	}
}
