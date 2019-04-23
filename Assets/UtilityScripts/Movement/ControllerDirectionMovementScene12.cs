using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerDirectionMovementScene12 : MonoBehaviour
{

    ControllerAccessors controller;
    ConstantDirectionMover room;
	public float maxSpeed = 3f;
    public float slowSpeedCutoff = 1f;
    public float fastSpeedCutoff = 2f;
    public Scene12SonifyFlowerSeedlings sonifier;
    // Use this for initialization
    void Start()
    {
        controller = GetComponent<ControllerAccessors>();
        room = GetComponentInParent<ConstantDirectionMover>();
    }

    // Update is called once per frame
    void Update()
    {
        if( controller.IsUnSqueezed() )
        {
            Vector3 v = controller.Velocity();
            float speed = v.magnitude.MapClamp( 0, 3, 0, maxSpeed );
            // transform direction from local space to world space
            Vector3 direction = room.transform.TransformDirection( v );
            room.SetDirection( direction.normalized, speed );

            if( speed < slowSpeedCutoff )
            {
                sonifier.SlowMovementHappened();
            }
            else if( speed > fastSpeedCutoff )
            {
                sonifier.FastMovementHappened();
            }
        }

    }
}
