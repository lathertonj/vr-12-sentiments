using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Scene5SeedlingController : MonoBehaviour
{
    public Transform seedlingPrefab;
    public Transform room, head;
    public int numSeedlings = 40;
    private int numSeedlingsReleased = 0;
    public Vector3 spawnRadius = 3f * Vector3.one;
    private Rigidbody[] mySeedlings;
    private bool[] mySeedlingsActive;
    public ControllerAccessors leftController, rightController;
    private ParticleSystem leftHand, rightHand;
    public float moveSpeed = 5f;
    private ParticleSystem myParticleEmitter;
    
    public float loseSeedTime = 20f;
    public float loseManySeedTime = 90f;

    private Scene5SonifyFlowerSeedlings mySonifier;

    // Use this for initialization
    void Start()
    {
        myParticleEmitter = GetComponentInChildren<ParticleSystem>();

        for( int i = 0; i < numSeedlings; i++ )
        {
            Transform newSeedling = Instantiate( seedlingPrefab, transform );
            newSeedling.localPosition = new Vector3(
                Random.Range( -spawnRadius.x, spawnRadius.x ),
                Random.Range( -spawnRadius.y, spawnRadius.y ),
                Random.Range( -spawnRadius.z, spawnRadius.z )
            );

            newSeedling.localRotation =
                Quaternion.AngleAxis( Random.Range( -20f, 20f ), Vector3.forward ) *
                Quaternion.AngleAxis( Random.Range( 5f, 355f ), Vector3.up );

        }

        mySeedlings = GetComponentsInChildren<Rigidbody>();
        mySeedlingsActive = new bool[mySeedlings.Length];
        for( int i = 0; i < mySeedlingsActive.Length; i++ ) { mySeedlingsActive[i] = true; }

        mySonifier = GetComponent<Scene5SonifyFlowerSeedlings>();
        mySonifier.StartChuck( jumpDelay: 0.25f, launchASeedling: LaunchASeedling, numSeedlings: numSeedlings );

        leftHand = leftController.GetComponentInChildren<ParticleSystem>();
        rightHand = rightController.GetComponentInChildren<ParticleSystem>();

    }

    float GetLossChance()
    {
        if( numSeedlingsReleased >= 30 )
        {
            return 0.0f;
        }
        else if( Time.time > loseManySeedTime )
        {
            return 0.15f;
        }
        else if( Time.time > loseSeedTime )
        {
            return 0.05f;
        }
        else
        {
            return 0.0f;
        }
    }

    int currentSeedling = 0;
    void LaunchASeedling()
    {
        if( mySeedlingsActive[currentSeedling] )
        {
            Rigidbody seedling = mySeedlings[currentSeedling];
            seedling.AddForce( 0.15f * Vector3.up + 0.45f * transform.forward, ForceMode.VelocityChange );
            Vector3 randomAngularVelocity = new Vector3(
                Random.Range( -1f, 1f ),
                Random.Range( -1f, 1f ),
                Random.Range( -1f, 1f )
            );
            seedling.AddTorque( randomAngularVelocity, ForceMode.VelocityChange );

            if( Random.Range( 0f, 1f ) < GetLossChance() )
            {
                // no longer active
                mySeedlingsActive[currentSeedling] = false;
                // change sonification
                mySonifier.InformLostSeedling( currentSeedling );
                // leave behind
                seedling.transform.parent = null;
                // give it a little forward movement, and also movement to the side, to make it seem less like it's just stopping
                int leftRight = Random.Range( 0f, 1f ) < 0.5f ? 1 : -1;
                seedling.AddForce( 2.5f * transform.forward + leftRight * 1.2f * transform.right, ForceMode.VelocityChange );
                // make it spin a little more too
                seedling.AddTorque( randomAngularVelocity * 4, ForceMode.VelocityChange );
                // track
                numSeedlingsReleased++;

            }

            // animate particle
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = myParticleEmitter.transform.InverseTransformPoint( seedling.position );
            emitParams.velocity = 0.15f * Vector3.up + 0.45f * transform.forward; // match to seedling?
                //seedling.velocity;//seedling.velocity.y * Vector3.up;  // take only the y velocity?
            myParticleEmitter.Emit( emitParams, count: 1 );
        }
        else
        {
            // do nothing
        }

        currentSeedling++; currentSeedling %= mySeedlings.Length;
    }
    
    void Update()
    {
        room.position += moveSpeed * Time.deltaTime * room.forward;
    }

}
