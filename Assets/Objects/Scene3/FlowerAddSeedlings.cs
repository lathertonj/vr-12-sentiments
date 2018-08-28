using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerAddSeedlings : MonoBehaviour
{
    public ControllerAccessors myController;
    public Transform seedlingPrefab;

    // Use this for initialization
    void Start()
    {
        StartCoroutine( "ControllerBuzzDutyCycle" );
    }

    // Update is called once per frame
    void Update()
    {
        if( shouldBuzz && myController.IsSqueezed() )
        {
            // TODO strength
            myController.Vibrate( 1000 );
        }
    }

    bool shouldBuzz = false;
    IEnumerator ControllerBuzzDutyCycle()
    {
        float dutyCycleHalfPeriod = 0.5f;
        while( true )
        {
            shouldBuzz = !shouldBuzz;
            if( shouldBuzz && myController.IsSqueezed() )
            {
                // spawn new seedling prefab
                Vector3 newPosition = new Vector3(
                    Random.Range( -0.05f, 0.05f ),
                    0,
                    Random.Range( -0.05f, 0.05f )
                );
                Quaternion newRotation = Quaternion.AngleAxis( Random.Range( 0, 359 ), Vector3.up );
                Transform newSeed = Instantiate( seedlingPrefab, newPosition + transform.position, transform.rotation, transform );
                newSeed.localRotation = newRotation;
                // set height to 0. why isn't it already?
                Vector3 p = newSeed.localPosition;
                p.y = 0;
                newSeed.localPosition = p;
            }
            yield return new WaitForSeconds( dutyCycleHalfPeriod );
            // TODO speedup
            if( myController.IsSqueezed() )
            {
                dutyCycleHalfPeriod = Mathf.Max( dutyCycleHalfPeriod - 0.01f, 0.1f );
            }
        }
    }
}
