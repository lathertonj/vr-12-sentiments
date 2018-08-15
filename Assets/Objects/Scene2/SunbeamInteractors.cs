using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunbeamInteractors : MonoBehaviour
{
    public static float sunbeamAccumulated = 0;
    public ControllerAccessors myController;
    private ushort currentStrength = 0;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if( currentSunbeam != null )
        {
            float elapsedTime = Time.time - sunbeamTime;
            // map [0, 5] seconds --> [min, max] for haptic pulse
            currentStrength = (ushort)( elapsedTime.PowMapClamp( 0, 5, 0, 3999, 3 ) 
                * currentSunbeamController.GetStrength()    
            );
            myController.Vibrate( currentStrength );

            sunbeamAccumulated += currentStrength * 1.0f / 3999 * Time.deltaTime;
        }
        else
        {
            currentStrength = 0;
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
