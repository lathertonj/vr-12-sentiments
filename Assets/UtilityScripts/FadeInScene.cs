using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInScene : MonoBehaviour
{

    public Color skyColor;
    public float waitTime;
    public float fadeTime;

    // Use this for initialization
    void Start()
    {
        SteamVR_Fade.Start( skyColor, duration: 0 );
        Invoke( "DoFade", waitTime );
    }

    void DoFade()
    {
        SteamVR_Fade.Start( Color.clear, duration: fadeTime );
    }
}
