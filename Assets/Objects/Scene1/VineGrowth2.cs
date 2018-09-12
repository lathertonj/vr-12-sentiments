using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineGrowth2 : MonoBehaviour
{
    public Transform vrRoom;
    private Scene1LookChords mySonifier;
    public ControllerAccessors leftController, rightController;

    private float growthSoFar = 0;

    // Use this for initialization
    void Start()
    {
        mySonifier = GetComponent<Scene1LookChords>();
    }

    // Update is called once per frame
    void Update()
    {
        float currentIntensity = mySonifier.GetCurrentLoudness();

        // TODO: should controllers vibrate with sound?
        // or only when growing something?
        ushort intensity = (ushort) currentIntensity.MapClamp( 0, 1f, 0, 100 );
        leftController.Vibrate( intensity );
        rightController.Vibrate( intensity );

        // change my scale
        // float scaleMultiplier = currentIntensity.MapClamp( 0, 1, 1, 1.01f );
        // vrRoom.localScale *= scaleMultiplier;
        float scaleIncrease = currentIntensity.MapClamp( 0, 1, 0, 0.0003f );
        growthSoFar += scaleIncrease;
        vrRoom.localScale = new Vector3( 
            vrRoom.localScale.x + scaleIncrease,
            vrRoom.localScale.y + scaleIncrease, 
            vrRoom.localScale.z + scaleIncrease 
        );

    }
}
