using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSeedlings : MonoBehaviour
{

    public float minRotation = 0;
    public float maxRotation = 60;
    private float currentRotation;
    public AnimationCurve rotationAmount = AnimationCurve.EaseInOut( 0, 1, 0, 1 );
    // Start is called before the first frame update
    void Start()
    {
        currentRotation = minRotation;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation *= Quaternion.AngleAxis( currentRotation * Time.deltaTime, Vector3.up );
    }

    public void SetAmount( float amount )
    {
        currentRotation = rotationAmount.Evaluate( Mathf.Clamp01( amount ) ).Map( 0, 1, minRotation, maxRotation );
    }
}
