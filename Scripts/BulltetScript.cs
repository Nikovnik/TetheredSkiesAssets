using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulltetScript : MonoBehaviour
{
    public float damage = 1;
    public float penetration = 1;
    public float speed;
    public Vector2 start_speed;
    
    public bool missle = false;
    public bool proximityCharge = false;

    private int sharpel_count = 10;
    public float range = 1f;
    public GameObject spark;
    public GameObject gSpark;
    public GameObject sharpel;
    public GameObject onDestroyEffect;

    public FactionManager.Team team;
    public float tracking_speed = 10;
    private Material mat;
    private Color col;
    private float t = 0;
    private float endT;
    private bool isColliding;

    public enum BulletType
    {
        AP,
        TI,
        HE,
        EM
    }
    public BulletType ammoType;

    public AircraftController ac;

    private Vector3 moveVector;

    // Start is called before the first frame update
    void Start()
    {
        col = Color.blue;

        mat = GetComponent<Renderer>().material;
        col = mat.color;

        endT = range;
        Invoke("BeforeDestroy", endT);
    }

    private void Update()
    {
        t += Time.deltaTime;

        mat.color = Color.Lerp(Color.white, Color.clear, t / endT);

        if (missle)
        {
            AircraftController[] Target = System.Array.FindAll(FindObjectsOfType<AircraftController>(), x => x.team != team);

            if (Target.Length > 0)
            {
                float minDistance = Vector3.Distance(transform.position, Target[0].transform.position);
                int minIndex = 0;
                for (int i = 0; i < Target.Length; i++)
                {

                    if (Vector3.Distance(transform.position, Target[i].transform.position) <= minDistance)
                    {
                        minDistance = Vector3.Distance(transform.position, Target[i].transform.position);
                        minIndex = i;
                    };

                }
                print(Target[minIndex].name);
                Quaternion target_angle = Quaternion.Euler(0, 0, Mathf.Atan2(Target[minIndex].transform.position.y - transform.position.y, Target[minIndex].transform.position.x - transform.position.x) * Mathf.Rad2Deg -90f);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target_angle, tracking_speed);

                
            }
        }
        moveVector = transform.up * speed + new Vector3(start_speed.x, start_speed.y, 0);
        transform.Translate(moveVector * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isColliding) return;
        isColliding = true;

        if (collision.gameObject.GetComponent<BulltetScript>())
        {
            print(name + " has hit " + collision.name + " at time: " + Time.time);

            BulltetScript cbullet = collision.gameObject.GetComponent<BulltetScript>();

            if (penetration < cbullet.penetration)
            {
                AfterHitSFX(spark);
                BeforeDestroy();
            }
        }
        else

        if (collision.gameObject.GetComponent<AircraftController>())
        {
            AircraftController tAc = collision.gameObject.GetComponent<AircraftController>();

            if (tAc != ac)
            {
                print(name + " has hit " + collision.name + " at time: " + Time.time);

                if (tAc.shield > 0)
                {
                    AfterHitSFX(gSpark);
                }
                else
                {
                    AfterHitSFX(spark);
                }
                penetration--;
                if (penetration <= 0)
                {
                    BeforeDestroy();
                }
            }
        }
        StartCoroutine(Trigger2DReset());
    }

    private void AfterHitSFX(GameObject ps)
    {
        Instantiate(ps, transform.position, transform.rotation);
        
        if (ps.GetComponent<AfterHitSFX>())
        {
            AfterHitSFX afterHit = ps.GetComponent<AfterHitSFX>();
            afterHit.hitAmount = damage;
        }
    }

    IEnumerator Trigger2DReset()
    {
        yield return new WaitForEndOfFrame();
        isColliding = false;
    }

    private void BeforeDestroy()
    {
        if (sharpel != null)
        {
            for (int i = 0; i < sharpel_count; i++)
            {
                Instantiate(sharpel, transform.position, Quaternion.Euler(0, 0, transform.localRotation.eulerAngles.z + Random.Range(-45, 45)));
            }

        }

        if (onDestroyEffect != null)
        {
            Instantiate(onDestroyEffect, transform.position, Quaternion.identity);
        }

        transform.DetachChildren();
        Destroy(gameObject);
    }
}
