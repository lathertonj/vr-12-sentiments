using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VineGrowth2 : MonoBehaviour
{
    public Transform vrRoom;
    private Scene1LookChords mySonifier;
    public ControllerAccessors leftController, rightController;
    public float growthCutoff1 = 0.3f, growthCutoff2 = 0.45f, growthCutoff3 = 0.6f;
    public Color skyFadeColor;

    public Transform[] thingsToDisableAtTransition, thingsToEnableAtTransition;

    private float growthSoFar = 0;

    // plot: when growthCutoff1 reached, start vibrating
    //       when growthCutoff2 reached, flash the orange sky color to the headset
    //          and add arms and leaves, and switch the chords
    //       when growthCutoff3 reached, fade to orange sky color and end scene
    // TODO: make growth more obvious

    // Use this for initialization
    void Start()
    {
        mySonifier = GetComponent<Scene1LookChords>();
    }

    bool haveSwitchedToSecondHalf = false;
    bool haveEndedScene = false;
    void Update()
    {
        float currentIntensity = mySonifier.GetCurrentLoudness();

        // controllers vibrate only when growing a significant amount
        if( growthSoFar > growthCutoff1 && growthSoFar <= growthCutoff2 )
        {
            ushort intensity = (ushort)growthSoFar.MapClamp( growthCutoff1, growthCutoff2, 0, 300 );
            leftController.Vibrate( intensity );
            rightController.Vibrate( intensity );
        }

        if( !haveSwitchedToSecondHalf && growthSoFar > growthCutoff2 )
        {
            // change sound 
            mySonifier.SwitchToSecondSetOfChords();

            // visual flash
            SteamVR_Fade.Start( skyFadeColor, duration: 0f );
            SteamVR_Fade.Start( Color.clear, duration: 3f );

            // enable and disable models
            foreach( Transform t in thingsToDisableAtTransition )
            {
                t.gameObject.SetActive( false );
            }

            foreach( Transform t in thingsToEnableAtTransition )
            {
                t.gameObject.SetActive( true );
            }

            // remember
            haveSwitchedToSecondHalf = true;
        }

        if( !haveEndedScene && growthSoFar > growthCutoff3 )
        {
            SteamVR_Fade.Start( skyFadeColor, duration: 5f );
            // fade audio in Scene1LookChords.
            Scene1LookChords.EndSceneAudio();
            // switch to next scene after duration + N::second
            Invoke( "SwitchToNextScene", 6f );
            haveEndedScene = true;
        }

        // change my scale
        // float scaleMultiplier = currentIntensity.MapClamp( 0, 1, 1, 1.01f );
        // vrRoom.localScale *= scaleMultiplier;
        float scaleIncrease = currentIntensity.MapClamp( 0, 1, 0, 0.0001f );
        growthSoFar += scaleIncrease;
        vrRoom.localScale = new Vector3(
            vrRoom.localScale.x + scaleIncrease,
            vrRoom.localScale.y + scaleIncrease,
            vrRoom.localScale.z + scaleIncrease
        );

    }

    void SwitchToNextScene()
    {
        // SceneManager.LoadScene( "2_ExcitementLonging" );
        StartCoroutine( "LoadSceneAsync" );
    }

    IEnumerator LoadSceneAsync()
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync( "2_ExcitementLonging" );

        // Wait until the asynchronous scene fully loads
        while( !asyncLoad.isDone )
        {
            yield return null;
        }
    }
}
