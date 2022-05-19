using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectScroller : MonoBehaviour
{
    WorldControl WC;
    Vector2 world_speed;
    public float offset = 25;

    // Start is called before the first frame update
    void Start()
    {
        WC = FindObjectOfType<WorldControl>();
        world_speed = WC.world_speed;
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(-world_speed / offset * Time.deltaTime, Space.World);
    }
}
