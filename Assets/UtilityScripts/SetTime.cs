using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetTime : MonoBehaviour
{
	float originalTimeScale;
	float originalDeltaTime;

	void Awake()
	{
		originalTimeScale = Time.timeScale;
		originalDeltaTime = Time.fixedDeltaTime;
	}


    public void Scale( float timeScalar )
	{
		Time.timeScale = originalTimeScale * timeScalar;
		Time.fixedDeltaTime = originalDeltaTime * timeScalar;
	}
}
