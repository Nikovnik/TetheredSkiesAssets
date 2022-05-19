using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public ParticleSystem smoke;
    public ParticleSystem fire;
    public Rigidbody2D rb;
    

    public float health;
    public float max_health = 100;

    private float damage;
    private bool IsDead;

    public float particle_inertia = 0;
    private float smoke_alpha;
    private float smoke_white;
    private float particle_speed;

    public bool isDead() { return IsDead; }

    // Start is called before the first frame update
    void Start()
    {
        //smoke = GetComponentInChildren<ParticleSystem>();
        //fire = GetComponentInChildren<ParticleSystem>();
        health = max_health;
        IsDead = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKey("u"))
        {
            health = 0;
        }

        particle_speed = -rb.velocity.magnitude / particle_inertia;

        var fire_m = fire.main;
        var main = smoke.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(smoke_white, smoke_white, smoke_white, smoke_alpha));
        main.startSpeed = particle_speed;
        fire_m.startSpeed = particle_speed;


        if (health / max_health > 0.85f)
        {
            fire.Stop();
            smoke.Stop();
            smoke_white = 1f;
            smoke_alpha = 0f;
        } else if (health / max_health < 0.85f & health / max_health > 0.35f) {
            fire.Stop();
            smoke.Play();
            smoke_white = 1f;
            if (smoke_alpha < 0.5f)
            {
                smoke_alpha = 1 - health / max_health - 0.15f;
            } else
            {
                smoke_alpha = 0.5f;
            }
        } else if (health / max_health <= 0.35f)
        {
            fire.Stop();
            smoke.Play();
            smoke_white = 0.5f;
            smoke_alpha = 0.5f;
        }

        if (health / max_health <= 0)
        {
            fire.Play();
            IsDead = true;

        }
    }

    void OnCollisionEnter2D(Collision2D col)
    {
        col.gameObject.GetComponent<Rigidbody2D>();
        damage = col.relativeVelocity.magnitude * col.rigidbody.mass;

        GameObject.Find("Main Camera").GetComponent<CameraControler>().shakeAmount = damage/2;
        if (health > 0)
        {
            health -= damage;
        }
        damage = 0;
    }
}
