using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineGrowth : MonoBehaviour
{
    public ControllerAccessors leftController, rightController;
    public Transform vrRoom;
    private Vector3 previousPosition;
    private float prevHeightChange = 0;
    private SonifyVineGrowth mySonifier;

    // Use this for initialization
    void Start()
    {
        previousPosition = transform.position;
        mySonifier = GetComponent<SonifyVineGrowth>();
    }

    // Update is called once per frame
    void Update()
    {
        float heightChange = transform.position.y - previousPosition.y;
        if( heightChange > 0 && ( leftController.IsSqueezed() || rightController.IsSqueezed() ) )
        {
            if( prevHeightChange < 0 )
            {
                mySonifier.StartPlaying();
            }

            ushort intensity = (ushort) heightChange.MapClamp( 0, 0.01f, 0, 3999 );
            leftController.Vibrate( intensity );
            rightController.Vibrate( intensity );

            // change my scale
            // float scaleMultiplier = heightChange.MapClamp( 0, 0.01f, 1, 1.01f );
            // vrRoom.localScale *= scaleMultiplier;
            float scaleIncrease = heightChange.MapClamp( 0, 0.005f, 0, 0.001f );
            vrRoom.localScale = new Vector3( 
                vrRoom.localScale.x + scaleIncrease,
                vrRoom.localScale.y + scaleIncrease, 
                vrRoom.localScale.z + scaleIncrease 
            );
        }
        // TODO is this the right condition?
        else // if( heightChange < 0 && prevHeightChange > 0 )
        {
            mySonifier.StopPlaying();
        }

        previousPosition = transform.position;
        prevHeightChange = heightChange;
    }
}
