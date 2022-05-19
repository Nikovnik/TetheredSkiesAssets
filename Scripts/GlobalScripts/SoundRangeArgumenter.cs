using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRangeArgumenter : MonoBehaviour
{
    private AudioSource Saudio;
    private float zoom;
    private float rangeMin = 50f;
    private float rangeMax = 100f;
    private float spatialMin = 0.5f;
    private float spatialMax = 1f;

    // Start is called before the first frame update
    void Start()
    {
        zoom = Camera.main.orthographicSize;
        Saudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (zoom < 25)
        {
            Saudio.spatialBlend = spatialMax;
            Saudio.maxDistance = rangeMin;
        } else
        {
            Saudio.spatialBlend = spatialMin;
            Saudio.maxDistance = rangeMax;
        }
    }
}
