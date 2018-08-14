using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunbeamController : MonoBehaviour
{
    public Transform visualPart;
    public float fadeCycleSeconds = 10;
    private Color startColor;
    private float maxAlpha;
    private MeshRenderer visualRenderer;

    private void Start()
    {
        visualRenderer = visualPart.GetComponent<MeshRenderer>();
        startColor = visualRenderer.material.color;
        maxAlpha = startColor.a;
        Debug.Log( maxAlpha );
    }

    // Update is called once per frame
    void Update()
    {
        visualPart.rotation *= Quaternion.AngleAxis( 10 * Time.deltaTime, Vector3.up );


        // color + glossiness tone down to 0 to make invisible
        float currentOpacity = 0.5f * ( 1 - Mathf.Cos( 2 * Mathf.PI * Time.time / fadeCycleSeconds ) );
        Color newColor = new Color( startColor.r, startColor.g, startColor.b, currentOpacity * maxAlpha );
        visualRenderer.material.color = newColor;
        visualRenderer.material.SetFloat( "_Glossiness", currentOpacity );
    }
}
