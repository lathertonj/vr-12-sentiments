﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Scene5SeedlingController : MonoBehaviour
{
    public Transform seedlingPrefab;
    public Transform room, head;
    public int numSeedlings = 40;
    public Vector3 spawnRadius = 3f * Vector3.one;
    private Rigidbody[] mySeedlings;
    private bool[] mySeedlingsActive;
    public ControllerAccessors leftController, rightController;
    private ParticleSystem leftHand, rightHand;
    public float moveSpeed = 5f;
    public float lossChance = 0.05f;
    //public Color handColor;

    private Scene5SonifyFlowerSeedlings mySonifier;
    private int numSqueezed = 0;

    // Use this for initialization
    void Start()
    {
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
        mySeedlingsActive = new bool[mySeedlings.Length];
        for( int i = 0; i < mySeedlingsActive.Length; i++ ) { mySeedlingsActive[i] = true; }

        mySonifier = GetComponent<Scene5SonifyFlowerSeedlings>();
        mySonifier.StartChuck( jumpDelay: 0.25f, launchASeedling: LaunchASeedling, numSeedlings: numSeedlings );

        leftHand = leftController.GetComponentInChildren<ParticleSystem>();
        rightHand = rightController.GetComponentInChildren<ParticleSystem>();

    }

    int currentSeedling = 0;
    void LaunchASeedling()
    {
        if( mySeedlingsActive[currentSeedling] )
        {
            Rigidbody seedling = mySeedlings[currentSeedling];
            seedling.AddForce( 0.15f * Vector3.up + 0.45f * transform.forward, ForceMode.VelocityChange );
            Vector3 randomAngularVelocity = new Vector3(
                Random.Range( -1f, 1f ),
                Random.Range( -1f, 1f ),
                Random.Range( -1f, 1f )
            );
            seedling.AddTorque( randomAngularVelocity, ForceMode.VelocityChange );

            if( Random.Range( 0f, 1f ) < lossChance )
            {
                // no longer active
                mySeedlingsActive[currentSeedling] = false;
                // change sonification
                mySonifier.InformLostSeedling( currentSeedling );
                // leave behind
                seedling.transform.parent = null;
                // give it a little forward movement, and also movement to the side, to make it seem less like it's just stopping
                int leftRight = Random.Range( 0f, 1f ) < 0.5f ? 1 : -1;
                seedling.AddForce( 2.5f * transform.forward + leftRight * 1.2f * transform.right, ForceMode.VelocityChange );
                // make it spin a little more too
                seedling.AddTorque( randomAngularVelocity * 4, ForceMode.VelocityChange );

            }
        }
        else
        {
            // do nothing
        }

        currentSeedling++; currentSeedling %= mySeedlings.Length;
    }

    // Update is called once per frame
    void Update()
    {
        //ProcessControllerInput( leftController, leftHand );
        //ProcessControllerInput( rightController, rightHand );

        // Testing lost seedlings
        /*if( Random.Range(0.0f, 1.0f ) < 0.05 )
        {
            mySonifier.InformLostSeedling( Random.Range( 0, mySeedlings.Length ) );
        }*/
        room.position += moveSpeed * Time.deltaTime * room.forward;
    }

    void ProcessControllerInput( ControllerAccessors controller, ParticleSystem hand )
    {
        if( controller.IsFirstSqueezed() )
        {
            controller.RecordSqueezeStartTime();
            numSqueezed++;
            if( numSqueezed == 1 )
            {
                //mySonifier.InformSqueezed();
            }
        }

        if( controller.IsSqueezed() )
        {
            float timeElapsed = controller.ElapsedSqueezeTime();

            // map within low intensity values. this movement is not intense so vibration is not strong.
            ushort intensity = (ushort) timeElapsed.MapClamp( 0, 5, 30, 180 );
            controller.Vibrate( intensity );

            // hand gets pinker and pinker colors as you squeeze
            // float fractionElapsed = Mathf.Clamp01( timeElapsed / maxSqueezeTime );
            // ParticleSystem.MainModule main = hand.main;
            // main.startColor = fractionElapsed * handPinkColor + ( 1f - fractionElapsed ) * Color.white;
        }
        else
        {
            // hand gets white particles
            // ParticleSystem.MainModule main = hand.main;
            // main.startColor = Color.white;
        }

        if( controller.IsUnSqueezed() )
        {
            numSqueezed--;
            if( numSqueezed == 0 )
            {
                //mySonifier.InformUnsqueezed();
            }

            float squeezeTime = controller.ElapsedSqueezeTime();
            Vector3 velocity = controller.Velocity();
            Vector3 angularVelocity = controller.AngularVelocity();

            // map time to number of seedlings affected
            int numSeedlingsToAffect = (int) squeezeTime.MapClamp( 0, 5, 0, mySeedlings.Length - 0.01f );
            // pick which ones by traversing in a random order;
            // the first numSeedlingsToAffect are affected in a primary way
            // and the remaining seedlings are affected in a secondary way
            System.Random random = new System.Random();
            int numSeedlingsProcessed = 0;
            foreach( int i in Enumerable.Range( 0, mySeedlings.Length ).OrderBy( x => random.Next() ) )
            {
                Rigidbody seedling = mySeedlings[i];
                Vector3 randomAngularVelocity = new Vector3(
                    Random.Range( -1f, 1f ),
                    Random.Range( -1f, 1f ),
                    Random.Range( -1f, 1f )
                );
                if( numSeedlingsProcessed < numSeedlingsToAffect )
                {
                    // primary action
                    float randomLargeMultiplier = Random.Range( 0.4f, 0.6f );
                    seedling.AddForce( randomLargeMultiplier * velocity, ForceMode.VelocityChange );
                    seedling.AddTorque( angularVelocity * 0.05f + randomAngularVelocity, ForceMode.VelocityChange );
                }
                else
                {
                    // secondary action
                    float randomSmallMultiplier = Random.Range( 0.001f, 0.2f );
                    seedling.AddForce( randomSmallMultiplier * velocity, ForceMode.VelocityChange );
                    seedling.AddTorque( randomAngularVelocity, ForceMode.VelocityChange );
                }
                numSeedlingsProcessed++;

            }

            // sonify arpeggio by num seedlings affected
            //mySonifier.PlayArpeggio( numSeedlingsToAffect );

            // TODO: do something to the hand animation when the controller is unsqueezed
        }
    }

    private Vector3 SeedlingCenter()
    {
        Vector3 sumPosition = Vector3.zero;
        foreach( Rigidbody rb in mySeedlings )
        {
            sumPosition += rb.transform.position;
        }
        return sumPosition / mySeedlings.Length;
    }

}
