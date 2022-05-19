using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class flash : MonoBehaviour
{
    public float duration = 0.01f;

    void Update()
    {
        Destroy(gameObject,duration);
    }
}
