using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene4SceneEnder : MonoBehaviour
{
    public Color skyColor;
    private int numSeedlingsInsideMe;

    void Start()
    {
        numSeedlingsInsideMe = GetComponentInParent<Scene4SeedlingController>().numSeedlings;
    }

    bool haveFaded = false;
    void Update()
    {
        if( !haveFaded && numSeedlingsInsideMe < 5 )
        {
            // End scene
            haveFaded = true;
            SteamVR_Fade.Start( skyColor, duration: 5 );
        }
    }

    private void OnTriggerExit( Collider other )
    {
        if( other.gameObject.CompareTag( "Seedling" ) )
        {
            numSeedlingsInsideMe--;
        }
    }
}
