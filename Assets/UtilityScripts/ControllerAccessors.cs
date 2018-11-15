using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR.InteractionSystem;

public class ControllerAccessors : MonoBehaviour
{
    // VR preamble
    private SteamVR_TrackedObject trackedObj;
    private SteamVR_Controller.Device Controller
    {
        get { return SteamVR_Controller.Input( (int) trackedObj.index ); }
    }
    void Awake()
    {
        trackedObj = GetComponent<SteamVR_TrackedObject>();
        goalVibrationEffectiveness = 1f;
        currentVibrationEffectiveness = goalVibrationEffectiveness;
        vibrationSlew = 0.02f;
    }

    private float currentVibrationEffectiveness;
    private float vibrationSlew;
    private float goalVibrationEffectiveness;
    private float squeezeStartTime;


    void Update()
    {
        currentVibrationEffectiveness += vibrationSlew * ( goalVibrationEffectiveness - currentVibrationEffectiveness );
    }

    public void StopVibrating()
    {
        goalVibrationEffectiveness = 0;
    }

    public void Vibrate( ushort amount )
    {
        amount = (ushort) ( currentVibrationEffectiveness * (float) amount );
        Controller.TriggerHapticPulse( amount );
    }

    public bool IsSqueezed()
    {
        return Controller.GetPress( SteamVR_Controller.ButtonMask.Grip );
    }

    public bool IsFirstSqueezed()
    {
        return Controller.GetPressDown( SteamVR_Controller.ButtonMask.Grip );
    }
    
    public bool IsUnSqueezed()
    {
        return Controller.GetPressUp( SteamVR_Controller.ButtonMask.Grip );
    }

    public Vector3 Velocity()
    {
        return Controller.velocity;
    }

    public Vector3 AngularVelocity()
    {
        return Controller.angularVelocity;
    }

    public void RecordSqueezeStartTime()
    {
        squeezeStartTime = Time.time;
    }

    public float ElapsedSqueezeTime()
    {
        return Time.time - squeezeStartTime;
    }

}
