using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunbeamController : MonoBehaviour
{
    public Transform partToRotate, partToRaise;
    //public Transform hiddenPart;
    public float fadeCycleSeconds = 10;
    public float maxBaseAngle = 25;
    public float maxSweepAngle = 8;
    public float maxRepositionRadius = 3f;
    private Color startColor;
    private float maxAlpha;
    private MeshRenderer visualRenderer, hiddenRenderer;
    private Vector3 originalPos;
    private float timePhase;
    private float currentStrength = 0;

    private bool shouldFade = false; // TODO back to true
    public static bool shouldStopFading = false;
    //private static int rendererOffset = 0;

    //private static List<SunbeamController> allSunbeams;

    //private static void PrioritizeSunbeamRenderer( SunbeamController sunbeam )
    //{
    //    // TODO: should this actually be reversed?
    //    //foreach( SunbeamController s in allSunbeams )
    //    //{
    //    //    // set it back down to 3000, 3001
    //    //    s.hiddenRenderer.material.renderQueue = 3000;
    //    //    s.visualRenderer.material.renderQueue = 3001;
    //    //}

    //    // make this one special, more prioritized
    //    sunbeam.hiddenRenderer.material.renderQueue += 2;
    //    sunbeam.visualRenderer.material.renderQueue += 2;
    //}

    //private static void DeprioritizeSunbeamRenderer( SunbeamController sunbeam )
    //{
    //    sunbeam.hiddenRenderer.material.renderQueue -= 2;
    //    sunbeam.visualRenderer.material.renderQueue -= 2;
    //}

    private void Start()
    {
        //if( allSunbeams == null )
        //{
        //    allSunbeams = new List<SunbeamController>();
        //}
        //allSunbeams.Add( this );

        visualRenderer = partToRotate.GetComponent<MeshRenderer>();
        startColor = visualRenderer.material.color;
        maxAlpha = startColor.a;
        originalPos = transform.localPosition;

        //// set renderer order
        //hiddenRenderer = hiddenPart.GetComponentInChildren<MeshRenderer>();
        //// 3000 = shader transparent
        //hiddenRenderer.material.renderQueue = 3000;// + rendererOffset;
        ////rendererOffset++;
        //visualRenderer.material.renderQueue = 3000 + 1;// + rendererOffset;
        ////rendererOffset++;


        timePhase = Random.Range( 1f, fadeCycleSeconds - 1f );
        InvokeRepeating( "SwitchLocation", timePhase, fadeCycleSeconds );
        visualRenderer.enabled = true;
        visualRenderer.material.color = new Color( 0, 0, 0, 0 );

        // show full ray at first
        ShowFullRay();
    }

    void ShowFullRay()
    {
        SetHeight( transform.position - 50 * transform.up, partToRaise );
    }

    void SetHeight( Vector3 globalPosition, Transform thingToMove )
    {
        // transform global position to local position
        Vector3 localPosition = transform.InverseTransformPoint( globalPosition );
        // take only the y part of it
        float hiddenHeight = localPosition.y;
        // set the localHiddenPosition.y to that
        Vector3 localHiddenPosition = thingToMove.localPosition;
        localHiddenPosition.y = hiddenHeight;
        thingToMove.localPosition = localHiddenPosition;
    }


    // TODO: store a list of intersecting hands
    // set my hidden height to the highest one
    // for that one, tell it it is receiving light
    // for all others, tell them that they are no longer receiving light
    // when one hand leaves, and actually every frame, find the new highest one,
    // and make sure it is the one receiving light
    // Update is called once per frame
    void Update()
    {
        // rotate the beam internally
        partToRotate.rotation *= Quaternion.AngleAxis( 10 * Time.deltaTime, Vector3.up );

        // rotate the beam externally
        if( shouldFade )
        {
            transform.rotation *= Quaternion.AngleAxis( rotationPerSecond.x * Time.deltaTime, Vector3.left ) * Quaternion.AngleAxis( rotationPerSecond.z * Time.deltaTime, Vector3.forward );
        }

        // color
        if( Time.time > timePhase && shouldFade )
        {
            // color tone down to 0 to make invisible
            // (subtract a little and clamp01 so it's 0 for longer)
            currentStrength = Mathf.Clamp01( 0.5f * ( 1 - Mathf.Cos( 2 * Mathf.PI * ( Time.time - timePhase ) / fadeCycleSeconds ) ) - 0.001f );
            Color newColor = new Color( startColor.r, startColor.g, startColor.b, currentStrength * maxAlpha );
            visualRenderer.material.color = newColor;
            if( shouldStopFading && shouldFade && currentStrength > 0.99f )
            {
                shouldFade = false;
            }
        }
        else if( !shouldFade )
        {
            currentStrength = 1;
            visualRenderer.material.color = startColor;
        }
    }

    Vector3 rotationPerSecond = Vector3.zero;

    // called every time the sunbeam moves
    void SwitchLocation()
    {
        if( !shouldFade ) return;

        // new angle 
        float newXRotation = Random.Range( -maxBaseAngle, maxBaseAngle );
        float newZRotation = Random.Range( -maxBaseAngle, maxBaseAngle );
        transform.rotation = Quaternion.AngleAxis( newXRotation, Vector3.left ) * Quaternion.AngleAxis( newZRotation, Vector3.forward );

        // new slow rotation
        rotationPerSecond.x = Random.Range( -maxSweepAngle, maxSweepAngle ) / fadeCycleSeconds;
        rotationPerSecond.z = Random.Range( -maxSweepAngle, maxSweepAngle ) / fadeCycleSeconds;

        // new position
        transform.localPosition = originalPos + new Vector3(
            Random.Range( -maxRepositionRadius, maxRepositionRadius ),
            0,
            Random.Range( -maxRepositionRadius, maxRepositionRadius )
        );

        // enable in case disabled
        //visualRenderer.enabled = true;
    }

    public float GetStrength()
    {
        return currentStrength;
    }

    // TODO: make it work for multiple hands. we need to undo our other one if this new one is higher...
    // v1: only works for one hand
    private GameObject myIntersectingHand = null;
    private void OnTriggerEnter( Collider other )
    {
        if( other.gameObject.CompareTag( "PlayerHandModel" ) )
        {
            myIntersectingHand = other.gameObject;

            // make my renderer more important
            //PrioritizeSunbeamRenderer( this );
        }
        Debug.Log( other.gameObject.name );
    }

    private void OnTriggerStay( Collider other )
    {
        if( other.gameObject == myIntersectingHand )
        {
            SetHeight( other.transform.position, partToRaise );
        }
    }

    private void OnTriggerExit( Collider other )
    {
        // stop
        if( other.gameObject == myIntersectingHand )
        {
            myIntersectingHand = null;
            ShowFullRay();

            // make my renderer less important
            //DeprioritizeSunbeamRenderer( this );
        }
    }
}
