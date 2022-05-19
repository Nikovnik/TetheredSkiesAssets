using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine;

public class ArnamentP : MonoBehaviour
{
    public GameObject Bullet1;
    public AircraftController ac;

    [Range(1/2f, 2f)]
    public float quality = 1;

    public float damage = 10;
    public float range = 15; float proximityFuse = 1.5f;
    public float projectile_speed = 20;
    public float inaccuracy = 0;
    [Range(0, 1500)]
    public float fire_rate = 225;
    public float burst_delay = 0.3f;

    public int clip_capacity = 8;
    public int reload_period = 5;
    public int reload_impulse = 2;


    [Range(1, 12)]
    public int multi_shot = 1;

    [Range(1, 50)]
    public int burst_fire = 3;

    public bool sound_loop;
    public AudioClip gunshot;

    public bool activated;

    private Vector2 start_velocity;

    [HideInInspector]
    public FactionManager.Team team;

    [HideInInspector]
    public int ammo;

    public float rotation_speed = 50;
    [HideInInspector]
    public Vector2 targetVector;

    private float angle;

    private float rng = 1f;
    private float tollerance = 0.05f;

    public CircleCollider2D Col;

    private bool reloading = false;
    public AudioClip reaload_sfx;
    public AudioClip dry_shot;

    private void Awake()
    {
        damage *= quality;
        projectile_speed *= quality;
        range *= quality;

        if (gunshot != null) GetComponent<AudioSource>().clip = gunshot;

        /*if (Bullet1 != null && Bullet1.name == "HE round")
        {
            damage /= 10;
        }*/

        damage /= multi_shot;

        ammo = clip_capacity;
    }
    public void Start()
    {

        ac = GetComponentInParent<AircraftController>();

        if (ac != null)
        {
            team = ac.team;
        }
        if (transform.parent == null)
        {
            ac = null;
        }
    }

    private void Update()
    {
        if (ac == null && Col == null)
        {
            Col = gameObject.AddComponent<CircleCollider2D>();
            Col.radius = 5f;
        }

        //mounted form
        if (Bullet1 != null && transform.parent != null)
        {

            Vector2 localTarget = targetVector - new Vector2(transform.position.x, transform.position.y);

            proximityFuse = localTarget.magnitude;

            angle = Mathf.Atan2(localTarget.y, localTarget.x) * Mathf.Rad2Deg - 90f;       

            Vector3 currentRotation = transform.localEulerAngles;

            float curZ = transform.eulerAngles.z;
            
            currentRotation.z = Mathf.MoveTowardsAngle(curZ, angle, rotation_speed * Time.deltaTime);
            

            transform.rotation = Quaternion.Euler(currentRotation);
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(currentRotation), rotation_speed * Time.deltaTime);

            Debug.DrawRay(transform.position, transform.up * range, Color.yellow);

            Debug.DrawRay(transform.position, DegreeToVector2(transform.eulerAngles.z + inaccuracy/2) * range, Color.grey);
            Debug.DrawRay(transform.position, DegreeToVector2(transform.eulerAngles.z - inaccuracy/2) * range, Color.grey);

            
        }
    }

    public static Vector2 RadianToVector2(float radian)
    {
        return new Vector2(Mathf.Cos(radian + Mathf.PI / 2), Mathf.Sin(radian + Mathf.PI / 2));
    }

    public static Vector2 DegreeToVector2(float degree)
    {
        return RadianToVector2(degree * Mathf.Deg2Rad);
    }

    public void Activate()
    {
        if (activated==false)
        {
            StartCoroutine(Shoot());
        }
        activated = true;
        
    }

    IEnumerator Reload()
    {
        float preplay = 0.3f;
        reloading = true;
        while (ammo < clip_capacity)
        {
            yield return new WaitForSeconds(reload_period - preplay);
            GetComponent<AudioSource>().PlayOneShot(reaload_sfx);
            yield return new WaitForSeconds(preplay);
            ammo = ammo + reload_impulse > clip_capacity ? clip_capacity : ammo + reload_impulse;

            
        }
        reloading = false;
    }
    IEnumerator Shoot()
    {
        

        if (sound_loop)
        {
            GetComponent<AudioSource>().Play();
        }
        ammo--;
        if (!reloading)
        {
            StartCoroutine(Reload());
        }

        for (int b = 0; b < burst_fire; b++)
        {
            if (!sound_loop)
            {
                
                GetComponent<AudioSource>().Play();
               
            }
            
            rng = Random.Range(1 - tollerance / 2, 1 + tollerance / 2);
            GetComponent<AudioSource>().pitch = 1 * rng;

            if (ac.pilot.manual)
                FindObjectOfType<CameraControler>().shakeAmount += damage;

            for (int i = 0; i < multi_shot; i++)
            {

                
                //GetComponent<AudioSource>().PlayOneShot(gunshot);

                GameObject bullet1 = Instantiate(Bullet1, transform.position, Quaternion.Euler(0, 0, transform.rotation.eulerAngles.z + Random.Range(-inaccuracy/2, inaccuracy/2)));
                BulltetScript bs1 = bullet1.GetComponent<BulltetScript>();
                bs1.team = team;

                bs1.ac = ac;

                bs1.tracking_speed = 1 / inaccuracy;

                bs1.range = bs1.proximityCharge ? (Mathf.Clamp(proximityFuse, 0, range) / projectile_speed / rng) : (range / projectile_speed / rng);

                

                if (ac != null)
                {
                    bs1.start_speed = ac.rb.velocity;
                } else
                {
                    Debug.LogError("No parent rigidbody on arnament " + name);
                }
                bs1.damage = damage;
                bs1.speed = projectile_speed * rng;

                //rb1.AddForce(transform.up * (damage * projectile_speed * rng), ForceMode2D.Impulse);
            }
            if (ammo < 1  && b == burst_fire -1)
                GetComponent<AudioSource>().PlayOneShot(dry_shot);

        yield return new WaitForSeconds(1/fire_rate*60);
        }

        if (burst_fire > 1)
        {
            yield return new WaitForSeconds(burst_delay);
        }
        activated = false;
        
    }
}
