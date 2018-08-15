using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineGrowth : MonoBehaviour
{
    public ControllerAccessors leftController, rightController;
    public Transform vrRoom;
    private Vector3 previousPosition;

    // Use this for initialization
    void Start()
    {
        previousPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        float heightChange = transform.position.y - previousPosition.y;
        if( heightChange > 0 && ( leftController.IsSqueezed() || rightController.IsSqueezed() ) )
        {
            ushort intensity = (ushort) heightChange.MapClamp( 0, 0.01f, 0, 3999 );
            leftController.Vibrate( intensity );
            rightController.Vibrate( intensity );

            // change my scale
            float scaleMultiplier = heightChange.MapClamp( 0, 0.01f, 1, 1.01f );
            vrRoom.localScale *= scaleMultiplier;
        }

        previousPosition = transform.position;
    }
}
