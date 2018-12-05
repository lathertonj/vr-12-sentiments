using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplashController : MonoBehaviour
{
    private ParticleSystem me;
    public ParticleSystem splashEmitter;
	public float splashVerticleOffset;

    private ParticleCollisionEvent[] collisionEvents;

    // Use this for initialization
    void Awake()
    {
        me = GetComponent<ParticleSystem>();
        collisionEvents = new ParticleCollisionEvent[16];
    }

    public void OnParticleCollision( GameObject other )
    {
		if( me == null )
		{
			me = GetComponent<ParticleSystem>();
		}

        int collCount = ParticlePhysicsExtensions.GetSafeCollisionEventSize( me );

        if( collCount > collisionEvents.Length )
        {
            collisionEvents = new ParticleCollisionEvent[collCount];
        }

        int eventCount = ParticlePhysicsExtensions.GetCollisionEvents( me, other, collisionEvents );

        for( int i = 0; i < eventCount; i++ )
        {

            ParticleCollisionEvent collision = collisionEvents[i];
            Vector3 worldPos = collision.intersection;
			
			//Debug.Log( "collided with " + other.name + " at " + worldPos.ToString() );

            // show a splash, slightly higher than collision point
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = splashEmitter.transform.InverseTransformPoint( worldPos + splashVerticleOffset * Vector3.up );
            splashEmitter.Emit( emitParams, count: 1 );

            // TODO: check if collidedWith is a hand rock and if so, inform that hand
            if( collision.colliderComponent != null )
            {
                GameObject collidedWith = collision.colliderComponent.gameObject;

            }
        }
    }
}
