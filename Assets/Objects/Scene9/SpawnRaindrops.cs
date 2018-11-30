using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnRaindrops : MonoBehaviour
{
	public Transform raindropPrefab;
	public Vector3 radius;

    // Use this for initialization
    void Start()
    {
		InvokeRepeating( "SpawnARaindrop", 0, 1f );
    }

	void SpawnARaindrop()
	{
		Vector3 newPosition = new Vector3(
			Random.Range( -radius.x, radius.x ),
			Random.Range( -radius.y, radius.y ),
			Random.Range( -radius.z, radius.z )
		);

		Instantiate( raindropPrefab, transform.position + newPosition, Quaternion.identity );
	}

    // Update is called once per frame
    void Update()
    {

    }
}
