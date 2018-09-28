using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunbeamController : MonoBehaviour
{
    public Transform visualPart;
    public float fadeCycleSeconds = 10;
    public float maxBaseAngle = 25;
    public float maxSweepAngle = 8;
    public float maxRepositionRadius = 3f;
    private Color startColor;
    private float maxAlpha;
    private MeshRenderer visualRenderer;
    private Vector3 originalPos;
    private float timePhase;
    private float currentStrength = 0;

    private bool shouldFade = true;
    public static bool shouldStopFading = false;
    public static bool shouldIncreaseSize = false;

    private void Start()
    {
        visualRenderer = visualPart.GetComponent<MeshRenderer>();
        startColor = visualRenderer.material.color;
        maxAlpha = startColor.a;
        originalPos = transform.localPosition;
        
        timePhase = Random.Range( 1f, fadeCycleSeconds - 1f );
        InvokeRepeating( "SwitchLocation", timePhase, fadeCycleSeconds );
        visualRenderer.enabled = true;
        visualRenderer.material.color = new Color( 0, 0, 0, 0 );
    }

    // Update is called once per frame
    void Update()
    {
        // rotate the beam internally
        visualPart.rotation *= Quaternion.AngleAxis( 10 * Time.deltaTime, Vector3.up );

        // rotate the beam externally
        if( shouldFade )
        {
            transform.rotation *= Quaternion.AngleAxis( rotationPerSecond.x * Time.deltaTime, Vector3.left ) * Quaternion.AngleAxis( rotationPerSecond.z * Time.deltaTime, Vector3.forward );
        }

        // increase size 
        if( shouldIncreaseSize )
        {
            visualPart.localScale *= 1 + 0.5f * Time.deltaTime;

            // if get too big, turn off for everyone
            if( visualPart.localScale.x > 25 )
            {
                shouldIncreaseSize = false;
            }
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
}
