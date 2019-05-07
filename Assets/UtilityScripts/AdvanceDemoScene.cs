using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AdvanceDemoScene : MonoBehaviour
{


    public ControllerAccessors leftHand, rightHand;
	public string nextScene;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if( leftHand.IsAppButtonPressed() || rightHand.IsAppButtonPressed() )
        {
			SceneManager.LoadScene( nextScene );
        }
    }
}
