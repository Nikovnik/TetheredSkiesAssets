using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SharpelScript : MonoBehaviour
{
    private Rigidbody2D rb;
    public float force = 0.5f;
    public float lifetime = 0.5f;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.AddForceAtPosition(transform.up * force, transform.position - transform.up, ForceMode2D.Impulse);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
