using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class AircraftController : MonoBehaviour
{
    public ParticleSystem[] smoke, fire;
    public ParticleSystem splash;
    private ParticleSystem.MainModule m_smoke, m_fire;
    private float smoke_alpha;
    private float smoke_white;
    public Rigidbody2D rb;
    //[Range(10f, 100f)]
    private float smoothFactor = 50f;

    [HideInInspector]
    public FactionManager.Team team = null;

    [HideInInspector]
    public float shield, health, inputV, inputH, Thrust, acceleration, rotation_speed, maxSpeed, minSpeed;

    [HideInInspector]
    public bool isDead() { return IsDead; }


    [Header("Survivability")]
    public float max_health = 100;
    public float max_shield = 100;
    public float shield_recharge_rate = 5;
    public float shield_delay = 5;
    public float shield_flux_dissipation = 2;
    [Space(10)]

    [Header("Mobility")]
    public float maxDeltaSpeed = 100;
    public float manuverbility = 1;
    [Space(10)]

    private float infoBarTileSize = 25f;  //how many points does represent one tile of bar

    private bool ignoreSpeedConstrains = false;
    
    [HideInInspector]
    public bool skipAerodynamics = false;
    private Vector2 AerodynamicalDirection;
    //private bool aeroRedirecting;

    private float last_hit;
    private bool IsDead;
    private float crashDir;
    private bool onIsDead = true;

    private float min_pitch, Thrust_pitch;

    [HideInInspector]
    public bool MovementOverride = false;

    private bool outOfBounds;
    private bool animating = false;

    [HideInInspector]
    public bool stationary;

    [HideInInspector]
    public bool redeploying, deploying = false;

    //public float malfuction_level = 0;

    private Canvas CraftInfo;
    private Vector3 canvas_size;
    private WorldControl WordControl;

    public PilotScript pilot;


    private void Awake()
    {
        if (GetComponentInChildren<PilotScript>())
        {
            pilot = GetComponentInChildren<PilotScript>();
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Weaponry");
        }

        WordControl = FindObjectOfType<WorldControl>();

        CraftInfo = GetComponentInChildren<Canvas>();
        canvas_size = CraftInfo.transform.localScale;

        acceleration = manuverbility;
        rotation_speed = manuverbility;

        Thrust = WordControl.world_speed.magnitude;

        maxSpeed = Thrust + maxDeltaSpeed;
        minSpeed = Thrust - maxDeltaSpeed;

        /*if (maxSpeed / rotation_speed > 1)
        {
            hit_n_run = true;
        } else
        {
            dog_fighter = true;
        }
        */

        min_pitch = GetComponent<AudioSource>().pitch;
        Thrust_pitch = maxSpeed * 0.9f;

        health = max_health;
        shield = max_shield;
        IsDead = false;

        
        rb = GetComponent<Rigidbody2D>();
        
    }

    public void Start()
    {
        if (GetComponentInChildren<PilotScript>())
        {
            pilot = GetComponentInChildren<PilotScript>();
        }
        else
        {
            gameObject.layer = LayerMask.NameToLayer("Weaponry");
        }

        inputH = 0;
        inputV = 0;

        //pilloted state
        if (pilot == default)
        {
            stationary = true;
        }
        else
        {
            stationary = false;
        }

        foreach (Collider2D c in GetComponents<Collider2D>())
        {
            c.enabled = true;
        }

        if (deploying)
        {
            Thrust = 0;
            transform.localScale = Vector3.one * 0.5f;
            transform.position = new Vector3(transform.position.x, transform.position.y, 3);
        }
    }

    private void LateUpdate()
    {
        

        if (CraftInfo != null)
        {
            CraftInfo.transform.rotation = Camera.main.transform.rotation;
            CraftInfo.transform.position = transform.position + new Vector3(1f, 0f);

            Vector2 mouse_pos = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;



            if (mouse_pos.magnitude < 10 || true)
            {
                CraftInfo.transform.localScale = Vector3.Lerp(CraftInfo.transform.localScale, canvas_size * Camera.main.orthographicSize / 10, 10 * Time.deltaTime);
            }
            else
            {
                CraftInfo.transform.localScale = Vector3.Lerp(CraftInfo.transform.localScale, new Vector3(canvas_size.x, 0) * Camera.main.orthographicSize / 10, 10 * Time.deltaTime);
                if (CraftInfo.transform.localScale.y < 0.3f)
                {
                    CraftInfo.transform.localScale = new Vector3(canvas_size.x, 0) * Camera.main.orthographicSize / 10;
                }
            }
            

            CraftInfo.GetComponentInChildren<Text>().text = "SP: " + Thrust.ToString("0");

            foreach (var infoUI in CraftInfo.GetComponentsInChildren<Image>())
            {
                if (infoUI.name == "Shield")
                {
                    infoUI.pixelsPerUnitMultiplier = max_shield / infoBarTileSize;
                    infoUI.rectTransform.sizeDelta = new Vector2(2 * shield / max_shield, infoUI.rectTransform.sizeDelta.y);
                }
                if (infoUI.name == "Health")
                {
                    infoUI.pixelsPerUnitMultiplier = max_health / infoBarTileSize;
                    infoUI.rectTransform.sizeDelta = new Vector2(2 * health / max_health, infoUI.rectTransform.sizeDelta.y);
                }
            }
        }
    }


    private void FixedUpdate()
    {

        //rigidbody rotation normalization
        if (rb.rotation < 0)
        {
            rb.rotation += 360;
        } else if (rb.rotation > 360)
        {
            rb.rotation -= 360;
        }

        //print(name + "'s rotation: " + rb.rotation);

        MovementOverride = outOfBounds || animating || IsDead || stationary;

        //aircraft basic movement
        if (!IsDead && !stationary)
        {



            //rb.angularVelocity = rotation_speed * -inputH;

            //print(name + "'s angular velocity: " + rb.angularVelocity);



            //regulations
            
            
            if (shield < max_shield && Time.time - last_hit > shield_delay)
            {
                shield += shield > 0 ? shield_recharge_rate * Time.fixedDeltaTime : shield_flux_dissipation * Time.fixedDeltaTime;
            }

            shield = Mathf.Clamp(shield, -max_shield, max_shield);
            health = Mathf.Clamp(health, 0, max_health);

            if (!(redeploying | deploying) && Thrust > maxSpeed + acceleration * Time.fixedDeltaTime * 5)
            {
                Thrust -= acceleration * 5 * Time.fixedDeltaTime;
            }

            if (Mathf.Abs(rb.position.x) > WordControl.World_Border.x || Mathf.Abs(rb.position.y) > WordControl.World_Border.y)
            {
                outOfBounds = true;
                CruiseTowards(new Vector2(-rb.position.x, -rb.position.y));
            }
            else
            {
                outOfBounds = false;
            }
        }

        if (inputV < 0)
        {
            Thrust = Mathf.MoveTowards(Thrust, minSpeed, acceleration * Time.fixedDeltaTime);
        }

        if (inputV > 0 )
        {
            Thrust = Mathf.MoveTowards(Thrust, maxSpeed, acceleration * Time.fixedDeltaTime);
        }

        AerodynamicalDirection = new Vector2(Mathf.Cos((rb.rotation + 90) * Mathf.Deg2Rad), Mathf.Sin((rb.rotation + 90) * Mathf.Deg2Rad));
        rb.velocity = AerodynamicalDirection * Thrust / 10 - FindObjectOfType<WorldControl>().world_speed / 10;

        rb.angularVelocity = rotation_speed * -inputH;


        if (!stationary)
        {
            //engine sound
            GetComponent<AudioSource>().pitch = min_pitch + Thrust / Thrust_pitch;

            //health properties, smoke and fire
            HealthStatus();
        }
        //death animation
        if (IsDead)
        {
            inputV = -1f;
            if (onIsDead)
            {
                StartCoroutine(DeathAnimation());

                onIsDead = false;
            }
        }
        
    }

    IEnumerator DeathAnimation()
    {
        animating = true;

        inputH = Random.Range(-1f, 1f);

        gameObject.layer = LayerMask.NameToLayer("Misc");

        if (CraftInfo != null)
        {
            //Destroy(CraftInfo.gameObject);
        }

        tag = "Untagged";


        yield return new WaitForSeconds(1f);

        Instantiate(splash, new Vector3(transform.position.x, transform.position.y, 3), transform.rotation);

        foreach (var ps in smoke)
        {
            m_smoke = ps.main;
            m_smoke.stopAction = ParticleSystemStopAction.Destroy;
        }

        foreach (var ps in fire)
        {
            m_fire = ps.main;
            m_fire.stopAction = ParticleSystemStopAction.Destroy;
        }

        foreach (var ps in fire)
        {
            DetachParticles(ps);
        }

        foreach (var ps in smoke)
        {
            DetachParticles(ps);
        }

        Destroy(gameObject);

    }

    public IEnumerator ChangeHeight(float tHeight, float tScale, float tSpeed)
    {
        float sHeight = transform.position.z;
        float sScale = transform.localScale.x;
        float sSpeed = Thrust;

        animating = true;

        while (Thrust != tSpeed)
        {
            ignoreSpeedConstrains = true;
            ThrotleToThrust(tSpeed);
            yield return new WaitForEndOfFrame();
            float t;
            if (sSpeed < tSpeed)
            {
                t = (Thrust - sSpeed) / (tSpeed - sSpeed);
            }
            else
            {
                t = (Thrust - sSpeed) / (tSpeed - sSpeed);
            }
            
            transform.localScale = Vector3.Lerp(Vector3.one * sScale, Vector3.one * tScale, t);
            transform.position = Vector3.Lerp(new Vector3(transform.position.x, transform.position.y, sHeight), new Vector3(transform.position.x, transform.position.y, tHeight), t);

            
        }
        ignoreSpeedConstrains = false;
        transform.localScale = Vector3.one * tScale;
        transform.position = new Vector3(transform.position.x, transform.position.y, tHeight);
        if (Thrust == 0)
        {
            stationary = true;
        } else
        {
            stationary = false;
        }
        animating = false;
    }

    public void PickUpItem()
    {
        float pickUpRadius = 2.5f;

        Collider2D[] items = Physics2D.OverlapCircleAll(transform.position, pickUpRadius, LayerMask.GetMask("Modifiers", "Weaponry"));
        if (items.Length > 0)
        {
            float minDistance = Mathf.Infinity;
            int minIndex = 0;
            for (int i = 0; i < items.Length; i++)
            {
                if (Vector3.Distance(transform.position, items[i].transform.position) <= minDistance)
                {
                    minDistance = Vector3.Distance(transform.position, items[i].transform.position);
                    minIndex = i;
                }
            }

            Collider2D item = items[minIndex];

            if (item != null && item.transform.parent != transform)
            {

                if (item.GetComponent<ArnamentP>())
                {
                    Destroy(item.GetComponent<Collider2D>());
                    var gun_item = item.GetComponent<ArnamentP>();

                    ArnamentP[] arnaments = transform.GetComponentsInChildren<ArnamentP>();

                    foreach (var gun in arnaments)
                    {
                        if (gun.tag == gun_item.tag && gun.name != gun_item.name)
                        {
                            gun_item.transform.SetParent(transform);
                            gun.transform.localEulerAngles = new Vector3(0, 0, 0);
                            gun_item.transform.localPosition = gun.transform.localPosition;
                            gun_item.transform.localRotation = gun.transform.localRotation;
                            gun_item.Start();
                            

                            gun.transform.SetParent(null);
                            gun.transform.localRotation = Quaternion.identity;
                            gun.Start();
                            if (gun.Bullet1 == null)
                            {
                                Destroy(gun.gameObject);
                            }
                        }
                    }
                }
                else if (item.GetComponent<AircraftController>())
                {
                    AircraftController t_ac = item.GetComponent<AircraftController>();
                    pilot.transform.SetParent(t_ac.transform);
                    pilot.transform.localPosition = Vector3.zero;
                    pilot.transform.localRotation = Quaternion.identity;

                    t_ac.pilot = pilot;
                    pilot.Start();
                    t_ac.Start();
                    Start();


                } 
                else
                {
                    Destroy(item.GetComponent<Collider2D>());
                    item.transform.SetParent(transform);
                    item.transform.localPosition = Vector3.zero;
                    item.transform.localRotation = Quaternion.identity;
                }
            }
        }
    }

    public void RotateTowards(Vector2 target_pos, float offset = 0)
    {
        Vector2 target_dis = target_pos - rb.position;
        float target_dir = Mathf.Atan2(target_dis.y, target_dis.x) * Mathf.Rad2Deg - 90f + offset; if (target_dir < 0) { target_dir += 360; } else if (target_dir > 360) { target_dir -= 360; }
        float angle_field = target_dir - rb.rotation; if (angle_field < -180) { angle_field += 360; } else if (angle_field > 180) { angle_field -= 360; }

        float slow_angle = 750 / rotation_speed;

        float w_damper = angle_field * slow_angle / 180;

        inputH = -Mathf.Clamp(w_damper, -1f, 1f);
    }

    public void CruiseTowards(Vector2 Tpos, float speedFactor = 0f)
    {
        float baseSpeed = WordControl.world_speed.magnitude;

        Vector2 tDir = Vector2.up;

        /*
        ac.inputH = inputVector.x;
        ac.inputV = inputVector.y;*/

        float deltaSpeed = maxSpeed - baseSpeed < baseSpeed - minSpeed ? maxSpeed - baseSpeed : baseSpeed - minSpeed;

        Vector2 inputVector = Tpos - rb.position;

        float sF =  Mathf.Clamp01(inputVector.magnitude * manuverbility / smoothFactor);
        deltaSpeed *= speedFactor * sF;
        inputVector.Normalize();

        

        ThrotleToThrust(deltaSpeed * inputVector.y + baseSpeed / Mathf.Cos(rb.rotation * Mathf.Deg2Rad));

        //tDir = new Vector2(inputVector.x * Mathf.Sqrt(ac.maxSpeed * ac.maxSpeed - baseSpeed * baseSpeed), baseSpeed);
        tDir = new Vector2(inputVector.x * deltaSpeed, baseSpeed);

        RotateTowards(tDir + rb.position);
        
    }

    public void CopyDirection(AircraftController Target)
    {
        float t_rotation = Target.transform.eulerAngles.z;
        RotateTowards(Vector2.up + rb.position, t_rotation);
    }

    public bool CheckOutTurn(GameObject Target, float turnSpeed = default, float velocity = default, float distance = default)
    {
        Vector2 targetPos = new Vector2(Target.transform.position.x, Target.transform.position.y) - rb.position;
        if (velocity == default) velocity = rb.velocity.magnitude;
        if (distance == default) distance = targetPos.magnitude;
        if (turnSpeed == default) turnSpeed = rotation_speed;

        float targetAngle = Mathf.Atan2(targetPos.y, targetPos.x) * Mathf.Rad2Deg - 90f; if (targetAngle < 0) { targetAngle += 360; } else if (targetAngle > 360) { targetAngle -= 360; }
        float deltaAngle = targetAngle - rb.rotation; if (deltaAngle < -180) { deltaAngle += 360; } else if (deltaAngle > 180) { deltaAngle -= 360; }

        return distance / velocity / 4 > Mathf.Abs(deltaAngle) / turnSpeed;
    }

    public void ThrotleToThrust(float t_thrust, float factor = 1)
    {
        Thrust = Mathf.MoveTowards(Thrust, t_thrust, acceleration / factor * Time.deltaTime);
        if (!ignoreSpeedConstrains)
        {
            Thrust = Mathf.Clamp(Thrust, minSpeed, maxSpeed);
        }
    }

    public void CopySpeed(AircraftController Target)
    {

        float t_thrust = Target.GetComponent<AircraftController>().Thrust;
        ThrotleToThrust(t_thrust);

    }

    void OnDrawGizmosSelected()
    {
        float w_speed = rb.angularVelocity * Mathf.Deg2Rad;
        // Draw a yellow sphere at the transform's position
        Gizmos.color = Color.green;
        if (Mathf.Abs(w_speed) > 0.01f)
        {
            Vector3 L_R = transform.right;

            if (w_speed < 0)
            {
                L_R *= -1;
            }
            Gizmos.DrawWireSphere(transform.position + L_R * -rb.velocity.magnitude / Mathf.Abs(w_speed), rb.velocity.magnitude / Mathf.Abs(w_speed));
        } else
        {
            Gizmos.DrawRay(transform.position - transform.up * 100, transform.up * 200);
        }
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<BulltetScript>() != null)
        {
            BulltetScript bullet = collision.gameObject.GetComponent<BulltetScript>();

            if (bullet.ac != this)
            {

                float damage;

                BulltetScript.BulletType damageType = collision.gameObject.GetComponent<BulltetScript>().ammoType;

                float damageShield;
                float damageHealth;
                float damageMalfuction;



                damage = bullet.damage;




                switch (damageType)   // TI/HV, AP, HE, EM
                {
                    case BulltetScript.BulletType.AP:
                        damageShield = 0.5f;
                        damageHealth = 1f;
                        damageMalfuction = 1f;
                        break;
                    case BulltetScript.BulletType.HE:
                        damageShield = 0.5f;
                        damageHealth = 1.5f;
                        damageMalfuction = 0.5f;
                        break;
                    case BulltetScript.BulletType.EM:
                        damageShield = 1f;
                        damageHealth = 0.1f;
                        damageMalfuction = 2f;
                        break;
                    default:
                        damageShield = 1;
                        damageHealth = 1;
                        damageMalfuction = 0.1f;
                        break;
                }



                if (shield > 0)
                {
                    shield -= damage * damageShield;

                    if (pilot != null && pilot.manual)
                    {
                        Camera.main.GetComponent<CameraControler>().suppressAmount += damage * damageShield / 20;
                    }
                }
                else if (health > 0)
                {
                    if (health < max_health * 0.9f && Random.Range(0f, 1f) < damage * 0.02f * damageMalfuction)
                    {
                        ObjectManager[] Managers = FindObjectsOfType<ObjectManager>();

                        foreach (var manager in Managers)
                        {
                            if (manager.name == "EffectManager")
                            {
                                manager.AddObjectToParent("random", transform);
                            }
                        }

                    }
                    health -= damage * damageHealth;

                    if (GetComponentInChildren<PilotScript>().manual)
                    {
                        Camera.main.GetComponent<CameraControler>().suppressAmount += damage * damageHealth / 20;
                    }
                }



                damage = 0;
                last_hit = Time.time;
            }
        }

        
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.GetComponent<AreadOfEffect>() != null)
        {
            AreadOfEffect area = collision.gameObject.GetComponent<AreadOfEffect>();

            if (shield > 0)
            {
                shield -= area.dps * 0.25f * Time.deltaTime;

                if (pilot != null && pilot.manual)
                {
                    Camera.main.GetComponent<CameraControler>().suppressAmount += area.dps * 0.25f * Time.fixedDeltaTime;
                }
            }
            else if (health > 0)
            {
                if (health < max_health * 0.9f && Random.Range(0f, 1f) < area.dps * Time.fixedDeltaTime * 0.02f)
                {
                    ObjectManager[] Managers = FindObjectsOfType<ObjectManager>();

                    foreach (var manager in Managers)
                    {
                        if (manager.name == "EffectManager")
                        {
                            manager.AddObjectToParent("random", transform);
                        }
                    }

                }
                health -= area.dps * Time.fixedDeltaTime;

                if (pilot.manual)
                {
                    Camera.main.GetComponent<CameraControler>().suppressAmount += area.dps * Time.fixedDeltaTime / 20;
                }
            }
            last_hit = Time.time;
        }
    }

    void HealthStatus()
    {
        
        foreach (var ps in smoke)
        {
            if (health / max_health > 0.95f)
            {
                if (ps.isEmitting) { ps.Stop(); }
                smoke_white = 1f;
                smoke_alpha = 0f;
            }
            else if (health / max_health < 0.95f & health / max_health > 0.5f)
            {
                if (!ps.isEmitting) { ps.Play(); }

                smoke_white = 1f;
                smoke_alpha = 1 - health / max_health;
            }
            else if (health / max_health <= 0.5f)
            {
                if (!ps.isEmitting) { ps.Play(); }
                smoke_white = health / max_health * 2;
                smoke_alpha = 0.5f;
            }

            if (health / max_health <= 0)
            {
                if (!ps.isEmitting) { ps.Play(); }
                if (!IsDead) { IsDead = true; }

            }

            m_smoke = ps.main;
            m_smoke.startColor = new Color(smoke_white, smoke_white, smoke_white, smoke_alpha);
        }

        foreach (var ps in fire)
        {
            if (health / max_health <= 0)
            {
                if (!ps.isEmitting) { ps.Play(); }
                if (!IsDead) { IsDead = true; }

            }
        }

        if (health / max_health <= 0)
        {
            if (!IsDead) { IsDead = true; }

        }

        //var main = smoke.main.startColor;
        //main = new Color(smoke_white, smoke_white, smoke_white, smoke_alpha);
    }

    private void DetachParticles(ParticleSystem emit)
    {
        emit.transform.SetParent(null);
        emit.Stop();

    }
}


