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

    public void Vibrate( ushort amount )
    {
        Controller.TriggerHapticPulse( amount );
    }

    public bool IsSqueezed()
    {
        return Controller.GetPress( SteamVR_Controller.ButtonMask.Grip );
    }

    // TODO: accessor for grips and whatever else
}
