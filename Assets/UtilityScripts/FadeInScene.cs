using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeInScene : MonoBehaviour
{

    public Color skyColor;
    public float waitTime;
    public float fadeTime;
    public bool debugFadeInColor = false;

    // Use this for initialization
    void Start()
    {
        SteamVR_Fade.Start( skyColor, duration: 0 );
        if( !debugFadeInColor )
        {
            Invoke( "DoFade", waitTime );
        }
    }

    void Update()
    {
        if( debugFadeInColor )
        {
            SteamVR_Fade.Start( skyColor, duration: 0 );
        }
    }

    void DoFade()
    {
        SteamVR_Fade.Start( Color.clear, duration: fadeTime );
    }
}
