using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlareArnament : MonoBehaviour
{
    public float cool_down = 5;
    public float defense_num = 10;
    public GameObject flare_particle;

    private float lastActive;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       

        if (Input.GetKey("f") & Time.time > cool_down + lastActive){
            for (int i = 0; i < defense_num; i++)
            {
                Instantiate(flare_particle, transform.position, transform.rotation);
            }

            lastActive = Time.time;
        }
    }
}
