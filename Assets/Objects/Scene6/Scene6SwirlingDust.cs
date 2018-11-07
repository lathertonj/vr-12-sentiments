using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene6SwirlingDust : MonoBehaviour
{
	private static Scene6SwirlingDust theDust;
	
    static void SetDustIntensity( float intensity )
	{
		theDust.goalIntensity = intensity;
	}

	private ParticleSystem myParticles;
	private ParticleSystemRenderer myRenderer;
	public float maxAlpha = 0.6f;
	private Color baseColor;
	public float maxRotatePerSecond = 10;

	private float currentIntensity, goalIntensity, intensitySlew;

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
		currentIntensity += Time.deltaTime * intensitySlew * ( goalIntensity - currentIntensity );
		SetIntensity( currentIntensity );
	}


	void SetIntensity( float intensity )
	{
		intensity = Mathf.Clamp01( intensity );

		// color
		baseColor.a = intensity * maxAlpha;
		myRenderer.material.SetColor( "_TintColor", baseColor );

		// swirl
		transform.rotation *= Quaternion.AngleAxis( intensity * maxRotatePerSecond * Time.deltaTime, Vector3.up );
	}
}
