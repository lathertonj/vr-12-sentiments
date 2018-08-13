using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateRay : MonoBehaviour
{
    
    // Update is called once per frame
    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis( 10 * Time.deltaTime, Vector3.up );
    }
}
