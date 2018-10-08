﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6RockColorCycler : MonoBehaviour
{
    public Color[] colors;
    public int startColor = 0;
    private MeshRenderer myRenderer;
    private int currentColor, prevColor;
    public float switchTime = 0.5f;

    // Use this for initialization
    void Start()
    {
        myRenderer = GetComponent<MeshRenderer>();
        myRenderer.material.color = colors[startColor];
        currentColor = startColor;
        InvokeRepeating( "SwitchToNextColor", 1f, 2f );
    }

    // Update is called once per frame
    void Update()
    {

    }

    void SwitchToNextColor()
    {
        prevColor = currentColor;
        currentColor++;
        currentColor %= colors.Length;
        StartCoroutine( "DoFade" );
    }

    IEnumerator DoFade()
    {
        /*float startH, startS, startV, finishH, finishS, finishV;
        Color.RGBToHSV( colors[prevColor], out startH, out startS, out startV );
        Color.RGBToHSV( colors[currentColor], out finishH, out finishS, out finishV );
		

        // hue is circular
        if( finishH - startH > 0.5f )
        {
            startH += 1;
        }
        else if( finishH - startH < -0.5f )
        {
            finishH += 1;
        }
		*/

        float startTime = Time.time;
        while( Time.time < startTime + switchTime )
        {
            float normElapsedTime = ( Time.time - startTime ) / switchTime;
			// don't HSV interp
            /*Color theColor = Color.HSVToRGB( 
				startH + normElapsedTime * ( finishH - startH ),
				startS + normElapsedTime * ( finishS - startS ),
				startV + normElapsedTime * ( finishV - startV )
			);*/
			
			// just straight up interp
            Color theColor = colors[prevColor] + normElapsedTime * ( colors[currentColor] - colors[prevColor] );
            myRenderer.material.color = theColor;

            yield return null;

        }
        // finally, make sure it ends on the correct color
        myRenderer.material.color = colors[currentColor];
    }
}
