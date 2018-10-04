using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6Seedling : MonoBehaviour
{
	Scene6SeedlingSonifier myParent;

    // Use this for initialization
    void Start()
    {
		myParent = GetComponentInParent<Scene6SeedlingSonifier>();
    }

    void OnCollisionEnter( Collision collision )
	{
		myParent.PlayASeedling();
	}
}
