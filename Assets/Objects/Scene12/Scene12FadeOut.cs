using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene12FadeOut : MonoBehaviour
{
    public Color skyColor;
    float lastTimeBeforeLookingUp = 0;
    public float lookUpEndTime = 5f;
    private bool haveTransitioned = false;
    private ChuckSubInstance myChuck;
    public float visualFadeTime = 7f, delayBeforeVisualFade = 3f;
    public float delayBeforeAudioFade = 3f;

    // Start is called before the first frame update
    void Start()
    {
        myChuck = GetComponent<ChuckSubInstance>();
    }

    // Update is called once per frame
    void Update()
    {
        if( Scene12LookUpGrowth.doneGrowing && !haveTransitioned )
        {
            if( !Scene12LookUpGrowth.currentlyLookingUp )
            {
                // not looking up
                lastTimeBeforeLookingUp = Time.time;
            }
            else
            {
                // looking up
                if( Time.time - lastTimeBeforeLookingUp > lookUpEndTime )
                {
                    haveTransitioned = true;
                    Invoke( "FadeAudio", delayBeforeAudioFade );
                    Invoke( "FadeVisuals", delayBeforeVisualFade );
                }
            }
        }
        else
        {
            // reset time too
            lastTimeBeforeLookingUp = Time.time;
        }
    }

    void FadeVisuals()
    {
        SteamVR_Fade.Start( skyColor, visualFadeTime );
    }

    void FadeAudio()
    {
        myChuck.BroadcastEvent( "scene12Finish" );
    }
}
