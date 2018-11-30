using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaindropInteractor : MonoBehaviour
{
    private ControllerAccessors controller;

    // Use this for initialization
    void Start()
    {
        controller = GetComponent<ControllerAccessors>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter( Collider other )
    {
        Raindrop maybeRaindrop = other.GetComponentInParent<Raindrop>();
        if( maybeRaindrop != null )
        {
            RaycastHit hit;
            if( Physics.Raycast( transform.position, maybeRaindrop.transform.position - transform.position, out hit ) )
            {
                maybeRaindrop.DeformRaindrop( hit.point, Mathf.Clamp( 2 * controller.Velocity().magnitude, 0, 5 ) );
            }
        }
    }


}
