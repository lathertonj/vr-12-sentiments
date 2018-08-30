using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }

    private float squeezeStartTime;

    public void Vibrate( ushort amount )
    {
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

    // TODO: accessor for grips and whatever else
}
