using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleWithCamera : MonoBehaviour
{
    private Camera cam;
    public float factor = 1f;
    private Vector3 scaleO;

    // Start is called before the first frame update
    void Start()
    {
        scaleO = transform.localScale;
        cam = Camera.main;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.localScale = new Vector3( scaleO.x * cam.orthographicSize * factor, scaleO.y * cam.orthographicSize * factor, 1);
    }
}
