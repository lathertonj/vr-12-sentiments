using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene12SeedlingController : MonoBehaviour
{

    public Transform seedlingPrefab;
    public int numSeedlings = 20;
    public Vector3 spawnRadius = 3f * Vector3.one;
    private Rigidbody[] mySeedlings;
    private bool[] mySeedlingsEncountered;
    private Dictionary<Rigidbody, int> mySeedlingIndices;
    private ParticleSystem myParticleEmitter;
    private float maxSqueezeTime = 5f;
    public Transform audioListenerPosition;


    private Scene12SonifyFlowerSeedlings mySonifier;

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
        mySeedlingIndices = new Dictionary<Rigidbody, int>();
        mySeedlingsEncountered = new bool[mySeedlings.Length];
        for( int i = 0; i < mySeedlings.Length; i++ )
        {
            mySeedlingsEncountered[i] = false;
            mySeedlingIndices[mySeedlings[i]] = i;
        }

        mySonifier = GetComponent<Scene12SonifyFlowerSeedlings>();
        mySonifier.StartChuck( jumpDelay: 0.4f, launchASeedling: LaunchASeedling, numSeedlings: numSeedlings );

    }


    int currentSeedling = 0;
    void LaunchASeedling()
    {
        if( mySeedlingsEncountered[currentSeedling] )
        {
            Rigidbody seedling = mySeedlings[currentSeedling];

            // TODO: which direction should seedlings move in?
            Vector3 seedlingVelocity = /* 0.15f * room.GetDirection() + */ 0.02f * RandomVector3();
            seedling.AddForce( seedlingVelocity, ForceMode.VelocityChange );

            Vector3 randomAngularVelocity = RandomVector3();
            seedling.AddTorque( randomAngularVelocity, ForceMode.VelocityChange );


            // animate particle
            ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
            emitParams.position = myParticleEmitter.transform.InverseTransformPoint( seedling.position );
            emitParams.velocity = seedlingVelocity; // take only the y component?
            myParticleEmitter.Emit( emitParams, count: 1 );
        }

        currentSeedling++; currentSeedling %= mySeedlings.Length;

        // tell chuck what the next distance will be
        if( mySeedlingsEncountered[currentSeedling] )
        {
            mySonifier.InformOfNextDistance( ( audioListenerPosition.position - mySeedlings[currentSeedling].transform.position ).magnitude );
        }
        else
        {
            mySonifier.MuteNextNote();
        }

    }

    public void EnableSeedling( Rigidbody seedling )
    {
        mySeedlingsEncountered[mySeedlingIndices[seedling]] = true;
        mySonifier.IncreaseNoteSpeed();
    }

    void Update()
    {

    }


    Vector3 RandomVector3()
    {
        return new Vector3(
            Random.Range( -1f, 1f ),
            Random.Range( -1f, 1f ),
            Random.Range( -1f, 1f )
        );
    }


}