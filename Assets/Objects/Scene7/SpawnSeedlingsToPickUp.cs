using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSeedlingsToPickUp : MonoBehaviour
{
    public Transform seedlingPrefab;
    public int numToSpawn = 5;
    public Vector3 spawnRadius;
	private Vector3[] forces;
    // Use this for initialization
    void Start()
    {
        for( int i = 0; i < numToSpawn; i++ )
        {
            Vector3 newPos = transform.position + new Vector3(
                Random.Range( -spawnRadius.x, spawnRadius.x ),
                Random.Range( -spawnRadius.y, spawnRadius.y ),
                Random.Range( -spawnRadius.z, spawnRadius.z )
            );

            Quaternion newRotation =
                Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.up ) *
                Quaternion.AngleAxis( Random.Range( 0, 360 ), Vector3.left );

            Instantiate( seedlingPrefab, newPos, newRotation, transform );
        }
		forces = new Vector3[numToSpawn];
		for( int i = 0; i < forces.Length; i++ )
		{
			forces[i] = new Vector3(
				Random.Range( -1f, 1f ),
				Random.Range( -1f, 1f ),
				Random.Range( -1f, 1f )
			);
		}
    }

    // Update is called once per frame
    void Update()
    {
		
    }

    void FixedUpdate()
    {
		Rigidbody[] seedlings = GetComponentsInChildren<Rigidbody>();
		for( int i = 0; i < seedlings.Length; i++ )
		{
			seedlings[i].AddTorque( 0.001f * forces[i], ForceMode.Force );
		}
    }
}
