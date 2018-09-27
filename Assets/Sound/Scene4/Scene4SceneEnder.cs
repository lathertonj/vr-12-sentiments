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
            // End scene soon
            haveFaded = true;
            Invoke( "EndScene", 5 );
        }
    }

    void EndScene()
    {
        SteamVR_Fade.Start( skyColor, duration: 5 );
        Scene4SeedlingController.shouldPlayArpeggios = false;
        TheChuck.instance.BroadcastEvent( "scene4EndEvent" );
        Invoke( "SwitchToNextScene", 9 );
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
