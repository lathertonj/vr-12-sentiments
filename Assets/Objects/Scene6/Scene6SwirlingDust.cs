using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6SwirlingDust : MonoBehaviour
{
	private static Scene6SwirlingDust theDust;
	
    public static void SetDustIntensity( float intensity )
	{
		theDust.goalIntensity = intensity;
	}

	public static void TurnOffDustVisualsButLeaveSwirl()
	{
		theDust.goalIntensity = 0;
		theDust.overrideSwirlIntensity = theDust.currentIntensity;
	}

	private ParticleSystem myParticles;
	private ParticleSystemRenderer myRenderer;
	public float maxAlpha = 0.6f;
	private Color baseColor;
	public float maxRotatePerSecond = 10;

	private float currentIntensity, goalIntensity, intensitySlew;
	private float overrideSwirlIntensity = -1f;

	void Awake()
	{
		theDust = this;
		myParticles = GetComponent<ParticleSystem>();
		myRenderer = GetComponent<ParticleSystemRenderer>();
		baseColor = Color.white;
		SetIntensity( 0 );

		currentIntensity = goalIntensity = 0f;
		intensitySlew = 1f; // multiplied by deltaTime, so seconds-y
	}

	void Update()
	{
		// compute interpolation
		currentIntensity += Time.deltaTime * intensitySlew * ( goalIntensity - currentIntensity );
		
		// stop overriding once we turn it all the way down
		if( currentIntensity < 0.001f )
		{
			overrideSwirlIntensity = -1f;
		}

		// set
		SetIntensity( currentIntensity );
	}


	void SetIntensity( float intensity )
	{
		intensity = Mathf.Clamp01( intensity );

		// color
		SetVisualIntensity( intensity );

		// swirl
		SetSwirlIntensity( intensity );
	}

	void SetVisualIntensity( float intensity )
	{
		baseColor.a = intensity * maxAlpha;
		myRenderer.material.SetColor( "_TintColor", baseColor );
	}

	void SetSwirlIntensity( float intensity )
	{
		if( overrideSwirlIntensity > 0 )
		{
			intensity = overrideSwirlIntensity;
		}
		transform.rotation *= Quaternion.AngleAxis( -intensity * maxRotatePerSecond * Time.deltaTime, Vector3.up );
	}
}
