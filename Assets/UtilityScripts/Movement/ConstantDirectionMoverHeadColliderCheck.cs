using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantDirectionMoverHeadColliderCheck : MonoBehaviour
{
    ConstantDirectionMover room;
    public GameObject[] collisionRespondersToInform;
    // Use this for initialization
    void Start()
    {
        room = GetComponentInParent<ConstantDirectionMover>();
    }

    void OnTriggerEnter( Collider other )
    {
        // if we hit the ground, stop moving.
        if( other.gameObject.CompareTag( "Ground" ) )
        {
            room.SetDirection( Vector3.zero, 0 );
            foreach( GameObject thing in collisionRespondersToInform )
            {
                CollisionResponder responder = thing.GetComponent( typeof( CollisionResponder ) ) as CollisionResponder;
                if( responder != null ) { responder.RespondToCollision(); }
            }
        }
    }
}

public interface CollisionResponder
{
    void RespondToCollision();
}