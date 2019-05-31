using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunbeamInteractors : MonoBehaviour
{
    public MeshRenderer myGlowLeaf;
    private Color glowLeafOriginalColor;
    public static float sunbeamAccumulated = 0;
    public ControllerAccessors myController;
    private ushort currentStrength = 0;
    public static float sunbeamFadeinTime = 5;
    public static bool turnDownVibrate = false;
    private float vibrateIntensity = 1;
    // the maximum value is 3999 but the controllers can't vibrate at maximum intensity for a long period of time
    private ushort maxVibrate = 1000;

    // Use this for initialization
    void Start()
    {
        glowLeafOriginalColor = myGlowLeaf.material.color;
    }

    // Update is called once per frame
    void Update()
    {
        if( turnDownVibrate )
        {
            vibrateIntensity *= 0.96f;
        }

        if( SunbeamController.haveExpanded )
        {
            currentStrength = maxVibrate;
            myController.Vibrate( (ushort) ( currentStrength * vibrateIntensity ) );
        }
        else if( currentSunbeam != null )
        {
            float elapsedTime = Time.time - sunbeamTime;
            // map [0, 5] seconds --> [min, max] for haptic pulse
            currentStrength = (ushort) ( elapsedTime.PowMapClamp( 0, sunbeamFadeinTime, 0, maxVibrate, 3 )
                * currentSunbeamController.GetStrength()
            );
            myController.Vibrate( (ushort) ( currentStrength * vibrateIntensity ) );

            sunbeamAccumulated += currentStrength * 1.0f / maxVibrate * Time.deltaTime;
        }
        else
        {
            currentStrength = 0;
        }

        // color the glow
        float glowAlpha = ( (float) currentStrength ).PowMapClamp( 0, maxVibrate, 0, glowLeafOriginalColor.a, 0.65f );
        Color currentGlow = new Color(
            glowLeafOriginalColor.r,
            glowLeafOriginalColor.g,
            glowLeafOriginalColor.b,
            glowAlpha
        );

        foreach( Material m in myGlowLeaf.materials )
        {
            m.color = currentGlow;
        }
    }

    private GameObject currentSunbeam = null;
    private SunbeamController currentSunbeamController = null;
    private float sunbeamTime;
    private void OnTriggerEnter( Collider other )
    {
        if( currentSunbeam == null &&
            other.gameObject.CompareTag( "Sunbeam" ) )
        {
            currentSunbeam = other.gameObject;
            currentSunbeamController = currentSunbeam.GetComponentInParent<SunbeamController>();
            sunbeamTime = Time.time;
        }
    }

    private void OnTriggerStay( Collider other )
    {
        OnTriggerEnter( other );
    }

    private void OnTriggerExit( Collider other )
    {
        if( other.gameObject == currentSunbeam )
        {
            currentSunbeam = null;
            currentSunbeamController = null;
            Debug.Log( "on this exit, sunbeam accumulated is " + sunbeamAccumulated.ToString( "0.000" ) );
        }
    }

    public ushort CurrentStrength()
    {
        return currentStrength;
    }
}
