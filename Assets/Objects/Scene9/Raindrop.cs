using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Raindrop : MonoBehaviour
{

	private MeshDeformer myDeformer;

    // Use this for initialization
    void Start()
    {
		myDeformer = GetComponentInChildren<MeshDeformer>();
    }

    // Update is called once per frame
    void Update()
    {

    }

	public void DeformRaindrop( Vector3 worldPoint, float force )
	{
		myDeformer.AddDeformingForce( worldPoint, force );
	}
}
