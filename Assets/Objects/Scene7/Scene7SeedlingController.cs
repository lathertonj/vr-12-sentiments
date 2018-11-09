using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene7SeedlingController : MonoBehaviour , CollisionResponder
{

    public Transform seedlingPrefab;
    public int numSeedlings = 20;
    public Vector3 spawnRadius = 3f * Vector3.one;
    private Rigidbody[] mySeedlings;
    public ControllerAccessors leftController, rightController;
    private ParticleSystem leftHand, rightHand;
    private ParticleSystem myParticleEmitter;
	private ConstantDirectionMover room;
	private float maxSqueezeTime = 5f;


    private Scene7SonifyFlowerSeedlings mySonifier;

    // Use this for initialization
    void Start()
    {
        myParticleEmitter = GetComponentInChildren<ParticleSystem>();
		room = GetComponentInParent<ConstantDirectionMover>();

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

        mySonifier = GetComponent<Scene7SonifyFlowerSeedlings>();
        mySonifier.StartChuck( jumpDelay: 0.4f, launchASeedling: LaunchASeedling, numSeedlings: numSeedlings );

        leftHand = leftController.GetComponentInChildren<ParticleSystem>();
        rightHand = rightController.GetComponentInChildren<ParticleSystem>();

    }


    int currentSeedling = 0;
    void LaunchASeedling()
    {
		Rigidbody seedling = mySeedlings[currentSeedling];

		Vector3 seedlingVelocity = 0.15f * room.GetDirection() + 0.02f * RandomVector3();
		seedling.AddForce( seedlingVelocity, ForceMode.VelocityChange );

		Vector3 randomAngularVelocity = RandomVector3();
		seedling.AddTorque( randomAngularVelocity, ForceMode.VelocityChange );


		// animate particle
		ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();
		emitParams.position = myParticleEmitter.transform.InverseTransformPoint( seedling.position );
		emitParams.velocity = seedlingVelocity; // take only the y component?
		myParticleEmitter.Emit( emitParams, count: 1 );

        currentSeedling++; currentSeedling %= mySeedlings.Length;
		
    }

    void Update()
    {
		ProcessController( leftController );
        ProcessController( rightController );
    }

	void ProcessController( ControllerAccessors controller )
	{
        if( controller.IsFirstSqueezed() )
        {
            controller.RecordSqueezeStartTime();
        }

		if( controller.IsSqueezed() )
        {
            float timeElapsed = controller.ElapsedSqueezeTime();
            
            // map within low intensity values. this movement is not intense so vibration is not strong.
            ushort intensity = (ushort) timeElapsed.MapClamp( 0, maxSqueezeTime, 30, 180 );
            controller.Vibrate( intensity );
        }
	}

	Vector3 RandomVector3()
	{
		return new Vector3(
			Random.Range( -1f, 1f ),
			Random.Range( -1f, 1f ),
			Random.Range( -1f, 1f )
		);
	}

    public void RespondToCollision()
    {
        // when we hit the ground, stop the happiness
        mySonifier.SlowMovementHappened();
    }
}