﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowerAddSeedlings : MonoBehaviour
{
    public ControllerAccessors myController;
    public Transform seedlingPrefab;
    private SonifyFlowerSeedlings mySonifier;

    // Use this for initialization
    void Start()
    {
        mySonifier = GetComponent<SonifyFlowerSeedlings>();
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
        float chordOffsetWait = 0.09f;
        while( true )
        {
            bool aboutToBuzz = !shouldBuzz;
            float chordOffsetWaited = 0;
            if( aboutToBuzz && myController.IsSqueezed() )
            {
                // new seedling --> play chord
                mySonifier.PlayChord();

                // wait a bit then do visual / vibration changes
                chordOffsetWaited = chordOffsetWait;
                yield return new WaitForSeconds( chordOffsetWait );

                // signal to start buzzing next Update()
                shouldBuzz = aboutToBuzz;

                // spawn new seedling prefab
                CreateSeedling();
            }
            else if( !aboutToBuzz || !myController.IsSqueezed() )
            {
                // buzz is over --> turn off chord
                mySonifier.StopChord();
                // wait a bit then do vibration changes
                chordOffsetWaited = chordOffsetWait;
                yield return new WaitForSeconds( chordOffsetWait );

                // signal immediately to stop buzzing
                shouldBuzz = aboutToBuzz;
            }
            
            // wait
            yield return new WaitForSeconds( dutyCycleHalfPeriod - chordOffsetWaited );

            // TODO tweak speedup
            if( myController.IsSqueezed() )
            {
                dutyCycleHalfPeriod = Mathf.Max( dutyCycleHalfPeriod - 0.01f, 0.1f );
            }
        }
    }

    private void CreateSeedling()
    {
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
}
