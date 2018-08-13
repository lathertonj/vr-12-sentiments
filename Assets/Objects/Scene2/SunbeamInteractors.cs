using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunbeamInteractors : MonoBehaviour
{
    public static float sunbeamAccumulated = 0;

    // VR preamble
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input( (int) trackedObj.index ); }
    }
    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
    }

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
            ushort currentStrength = (ushort) elapsedTime.PowMapClamp( 0, 5, 0, 3999, 3 );
            Controller.TriggerHapticPulse( currentStrength );

            sunbeamAccumulated += currentStrength * 1.0f / 3999 * Time.deltaTime;
        }
    }

    private GameObject currentSunbeam = null;
    private float sunbeamTime;
    private void OnTriggerEnter( Collider other )
    {
        if( currentSunbeam == null &&
            other.gameObject.CompareTag( "Sunbeam" ) )
        {
            currentSunbeam = other.gameObject;
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
        }
        Debug.Log( "on this exit, sunbeam accumulated is " + sunbeamAccumulated.ToString( "0.000" ) );
    }
}
