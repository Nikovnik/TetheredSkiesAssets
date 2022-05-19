using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AirBattle : MonoBehaviour
{

    Deployment Squads;

    List<AircraftController> Allies;
    List<AircraftController> Enemies;

    private WorldControl worldControl;

    private void Awake()
    {
        worldControl = FindObjectOfType<WorldControl>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
