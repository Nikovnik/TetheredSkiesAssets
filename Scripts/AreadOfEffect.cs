using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class AreadOfEffect : MonoBehaviour
{
    public float dps;
    public float range;
    public float duration;

    CircleCollider2D circle;

    private void Start()
    {
        circle = GetComponent<CircleCollider2D>();
        circle.radius = range;
        Destroy(gameObject, duration);
    }
}
