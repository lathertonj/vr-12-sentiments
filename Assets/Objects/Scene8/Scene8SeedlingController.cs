using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Scene8SeedlingController : MonoBehaviour
{
    public Transform seedlingPrefab;
    public int numSeedlings = 12;
    public Vector3 spawnRadius = 1f * Vector3.one;
    private Rigidbody[] mySeedlings;
    public ControllerAccessors leftController, rightController;
    public Transform room;
    private ParticleSystem leftHand, rightHand;
    public float maxSqueezeTime = 5f;

    private Scene8SonifySeedlings mySonifier;
    private ParticleSystem myParticleEmitter;
    private int numSqueezed = 0;

    // arpeggios
    public static bool shouldPlayArpeggios = true;
    private List<Transform> arpeggioSeedlings;
    private int currentArpeggioSeedling;

    // TIME
    float originalTimeScale, originalDeltaTime;
    bool currentlySlowingTime = false;
    public float minimumTimeScale = 0.5f;
    public float timeBeforeDecrease = 0.5f;
    public float timeToDecreaseTimeScale = 0.3f;
    public float realTimeSpentAtSlowTimeScale = 2f;
    public float timeToIncreaseTimeScale = 3f;

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
        mySonifier = GetComponent<Scene8SonifySeedlings>();
        mySonifier.StartChuck( jumpDelay: 0.8f, launchASeedling: LaunchASeedling, animateArpeggioSeedling: AnimateArpeggioSeedling );

        leftHand = leftController.GetComponentInChildren<ParticleSystem>();
        rightHand = rightController.GetComponentInChildren<ParticleSystem>();
    }

    void LaunchASeedling()
    {
        Rigidbody seedling = mySeedlings[Random.Range( 0, mySeedlings.Length - 1 )];
        seedling.AddForce( 0.5f * Vector3.up, ForceMode.VelocityChange );
        Vector3 randomAngularVelocity = new Vector3(
            Random.Range( -1f, 1f ),
            Random.Range( -1f, 1f ),
            Random.Range( -1f, 1f )
        );
        seedling.AddTorque( randomAngularVelocity, ForceMode.VelocityChange );

        // animate particle
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = myParticleEmitter.transform.InverseTransformPoint( seedling.transform.TransformPoint( 0.2f * Vector3.up ) );
        emitParams.velocity = 0.15f * Vector3.up + 0.45f * transform.forward; // match to seedling?
            //seedling.velocity;//seedling.velocity.y * Vector3.up;  // take only the y velocity?
        myParticleEmitter.Emit( emitParams, count: 1 );
    }

    // Update is called once per frame
    void Update()
    {
        ProcessControllerInput( leftController, leftHand );
        ProcessControllerInput( rightController, rightHand );
    }

    void ProcessControllerInput( ControllerAccessors controller, ParticleSystem hand )
    {
        if( controller.IsFirstSqueezed() )
        {
            controller.RecordSqueezeStartTime();
            numSqueezed++;
            if( numSqueezed == 1 )
            {
                mySonifier.InformSqueezed();
            }
        }

        if( controller.IsSqueezed() )
        {
            float timeElapsed = controller.ElapsedSqueezeTime();
            
            // map within low intensity values. this movement is not intense so vibration is not strong.
            ushort intensity = (ushort) timeElapsed.MapClamp( 0, maxSqueezeTime, 30, 180 );
            controller.Vibrate( intensity );
        }

        if( controller.IsUnSqueezed() )
        {
            // try to slow time
            SlowTime();

            // process seedlings
            numSqueezed--;
            if( numSqueezed == 0 )
            {
                mySonifier.InformUnsqueezed();
            }

            float squeezeTime = controller.ElapsedSqueezeTime();
            Vector3 velocity = controller.Velocity();
            // local space to world space, I hope?
            velocity = room.TransformDirection( velocity );

            Vector3 angularVelocity = controller.AngularVelocity();
            // local space to world space, I hope?
            angularVelocity = room.TransformDirection( angularVelocity );

            // map time to number of seedlings affected
            int numSeedlingsToAffect = (int) squeezeTime.MapClamp( 0, maxSqueezeTime, 0, mySeedlings.Length - 0.01f );
            // pick which ones by traversing in a random order;
            // the first numSeedlingsToAffect are affected in a primary way
            // and the remaining seedlings are affected in a secondary way
            System.Random random = new System.Random();
            int numSeedlingsProcessed = 0;
            arpeggioSeedlings = new List<Transform>();
            currentArpeggioSeedling = 0;
            foreach( int i in Enumerable.Range( 0, mySeedlings.Length ).OrderBy( x => random.Next() ) )
            {
                Rigidbody seedling = mySeedlings[i];
                Vector3 randomAngularVelocity = new Vector3(
                    Random.Range( -1f, 1f ),
                    Random.Range( -1f, 1f ),
                    Random.Range( -1f, 1f )
                );
                if( numSeedlingsProcessed < numSeedlingsToAffect )
                {
                    // primary action
                    float randomLargeMultiplier = Random.Range( 0.4f, 0.6f );
                    seedling.AddForce( randomLargeMultiplier * velocity, ForceMode.VelocityChange );
                    seedling.AddTorque( angularVelocity * 0.05f + randomAngularVelocity, ForceMode.VelocityChange );

                    // animate for primary action
                    arpeggioSeedlings.Add( seedling.transform );
                    
                }
                else
                {
                    // secondary action
                    float randomSmallMultiplier = Random.Range( 0.001f, 0.2f );
                    seedling.AddForce( randomSmallMultiplier * velocity, ForceMode.VelocityChange );
                    seedling.AddTorque( randomAngularVelocity, ForceMode.VelocityChange );
                }
                numSeedlingsProcessed++;

            }

            // sonify arpeggio by num seedlings affected
            if( shouldPlayArpeggios && numSeedlingsToAffect > 0 )
            {
                mySonifier.PlayArpeggio( numSeedlingsToAffect );
            }
        }
    }

    void AnimateArpeggioSeedling()
    {
        // animate
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
        emitParams.position = myParticleEmitter.transform.InverseTransformPoint( 
            arpeggioSeedlings[currentArpeggioSeedling].TransformPoint( 0.2f * Vector3.up ) 
        );
        emitParams.velocity = 0.15f * Vector3.up;
        myParticleEmitter.Emit( emitParams, count: 1 );

        // choose next one
        currentArpeggioSeedling++;
        currentArpeggioSeedling %= arpeggioSeedlings.Count;
    }

    void SlowTime()
    {
        if( currentlySlowingTime ) return;
        currentlySlowingTime = true;
        StartCoroutine( "ChangeTimeScale" );
    }

    IEnumerator ChangeTimeScale()
    {
        originalTimeScale = Time.timeScale;
        originalDeltaTime = Time.fixedDeltaTime;

        // wait
        yield return new WaitForSecondsRealtime( timeBeforeDecrease );

        // decrease time speed
        float startTime = Time.unscaledTime;
        float endTime = startTime + timeToDecreaseTimeScale;
        while( Time.unscaledTime < endTime )
        {
            float timeScalar = Time.unscaledTime.PowMapClamp( startTime, endTime, 1, minimumTimeScale, 0.5f );
            Time.timeScale = originalTimeScale * timeScalar;
            Time.fixedDeltaTime = originalDeltaTime * timeScalar;
            // wait for one frame
            yield return null;
        }

        // wait
        yield return new WaitForSecondsRealtime( realTimeSpentAtSlowTimeScale );

        // increase time
        startTime = Time.unscaledTime;
        endTime = startTime + timeToIncreaseTimeScale;
        while( Time.unscaledTime < endTime )
        {
            float timeScalar = Time.unscaledTime.PowMapClamp( startTime, endTime, minimumTimeScale, 1, 0.1f );
            Time.timeScale = originalTimeScale * timeScalar;
            Time.fixedDeltaTime = originalDeltaTime * timeScalar;
            // wait for one frame
            yield return null;
        }
        // done
        currentlySlowingTime = false;
    }

}
