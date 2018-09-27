using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
            Invoke( "SwitchToNextScene", 9 );
        }
    }

    void SwitchToNextScene()
    {
        SceneManager.LoadScene( "5_UnsureMournfulPeaceful" );
    }

    private void OnTriggerExit( Collider other )
    {
        if( other.gameObject.CompareTag( "Seedling" ) )
        {
            numSeedlingsInsideMe--;
        }
    }
}
