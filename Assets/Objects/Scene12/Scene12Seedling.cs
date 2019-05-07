using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene12Seedling : MonoBehaviour
{
    Scene12SeedlingController parent;
    Rigidbody me;
    // Start is called before the first frame update
    void Start()
    {
        parent = GetComponentInParent<Scene12SeedlingController>();
        me = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter( Collider other )
    {
        if( other.gameObject.CompareTag( "Room" ) )
        {
            parent.EnableSeedling( me );
            Destroy( this );
        }
    }
}
