using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PilotScript : MonoBehaviour
{

    private AircraftController ac;
    private FactionManager tm;

    public int team_index;
    [HideInInspector]
    public FactionManager.Team team;

    public int squadIndex = 1;

    [HideInInspector]
    public bool manual = false;
    bool shoot_MCA, shoot_SCA, auto_shoot_MCA, auto_shoot_SCA = false;
    Vector2 gun_point_target;
    float rng_factor = 1f;
    float aim_sway = 1f;

    private bool knocked_out = false;
    private float pilot_consciousness;
    private float pilot_cons_max = 100f;
    private float pilot_wake_up_rate = 20f;
    private float knockout_threshold = 0.4f;
    [Range(0.5f, 1f)]
    public float timidness = 0.75f;
    [Range(0f, 100f)]
    public float precission = 5;
    [Range(0f, 5f)]
    public float evasion = 1;
    public float awareness_range = 100f;

    [HideInInspector]
    public ArnamentP main_arnament;

    //private int planeTactic;                   //valid plane tactic: 0 = convoy/escort, 1 = hunter, 2 = fighter
    bool isCruising = true;

    CursorScript Cursor;
    WorldControl WC;

    [HideInInspector]
    public PilotScript Leader;
    [HideInInspector]
    public Deployment Station;

    public enum State
    {
        Dormant,
        Attacking,
        Avoiding,
        Following,
        Patrolling
    } 
    public State currentState;

    public enum Goal
    {
        None,
        SearchAndDestroy,
        Escort
    }
    public Goal currentGoal;

    ArnamentP[] MCAs;
    ArnamentP[] SCAs;

    public void ChangeTeam(int teamIndex)
    {
        if(teamIndex != team_index) //team_index is initial team, difference should refer that it's called from other script as a new change
        {
            print(ac.name + "'s team change to " + teamIndex + " is called externally.");
        }
        
        team = tm.Teams[teamIndex];
        print(team.teamColor);
        ac.team = team;
        ac.GetComponentInChildren<TargetIndicator>().ChangeColor(team.teamColor);
    }

    private void Awake()
    {
        WC = FindObjectOfType<WorldControl>();
        tm = FindObjectOfType<FactionManager>();
        Cursor = FindObjectOfType<CursorScript>();
        pilot_consciousness = pilot_cons_max;
    }

    public void Start()
    {
        ac = transform.parent.GetComponent<AircraftController>();

        ChangeTeam(team_index);


        foreach (var gun in ac.GetComponentsInChildren<ArnamentP>())
        {
            gun.team = team;
        }

        MCAs = System.Array.FindAll(ac.GetComponentsInChildren<ArnamentP>(), x => x.tag == "MCArnament");
        SCAs = System.Array.FindAll(ac.GetComponentsInChildren<ArnamentP>(), x => x.tag == "SCArnament");

        if(MCAs.Length != 0)
            main_arnament = MCAs[MCAs.Length -1];
    }

    // Update is called once per frame
    void Update()
    {
        if (!ac.isDead() && !knocked_out && !ac.deploying && !ac.redeploying && !ac.MovementOverride)
        {
            //Manual(player) controll
            if (manual)
            {
                Vector2 inputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

                if (inputVector.magnitude > 1)
                {
                    inputVector.Normalize();
                }

                Cursor.PlayerOnScreen = Camera.main.WorldToScreenPoint(ac.transform.position);

                float baseSpeed = WC.world_speed.magnitude;

                if (isCruising && baseSpeed - ac.minSpeed > 25)
                {
                    CruiseTowards(ac.rb.position + inputVector * ac.manuverbility, inputVector.magnitude);
                    
                    if (GetClosestEnemy() != null)
                    {
                        Vector2 targetLock = GetClosestEnemy().transform.position - transform.position;

                        if (targetLock.magnitude < 25)
                        {
                            Cursor.LockOn = targetLock;
                        }
                    }
                }
                else
                {
                    if (Input.GetAxisRaw("Vertical") != 0 && !ac.MovementOverride)
                    {
                        ac.inputV = Input.GetAxisRaw("Vertical");
                    }
                    else if (!ac.MovementOverride)
                    {
                        ac.inputV = 0;
                    }

                    if (Input.GetButton("Jump") && !ac.MovementOverride)
                    {
                        float offset = -45 * Input.GetAxisRaw("Horizontal");
                        RotateTowards(Camera.main.ScreenToWorldPoint(Input.mousePosition), offset);
                    }
                    else
                    if (Input.GetAxisRaw("Horizontal") != 0 && !ac.MovementOverride)
                    {
                        ac.inputH = Input.GetAxisRaw("Horizontal");
                    }
                    else if (!ac.MovementOverride)
                    {
                        ac.inputH = 0;
                    }
                }

                //manual shooting
                if (Input.GetKeyDown("1"))
                {
                    auto_shoot_MCA = !auto_shoot_MCA;
                }
                if (Input.GetKeyDown("2"))
                {
                    auto_shoot_SCA = !auto_shoot_SCA;
                }
                if (Input.GetKeyDown("x") || true) //temporal override
                {
                    auto_shoot_MCA = false;
                    auto_shoot_SCA = false;
                }

                shoot_MCA = Input.GetAxisRaw("Fire1") != 0;
                shoot_SCA = Input.GetAxisRaw("Fire2") != 0;

                gun_point_target = Cursor.transform.position;

                if (auto_shoot_MCA || auto_shoot_SCA)
                {
                    AutoTurret(auto_shoot_MCA, auto_shoot_SCA);
                }
                if (!auto_shoot_MCA || !auto_shoot_SCA)
                {
                    ManualTurret(shoot_MCA, shoot_SCA, auto_shoot_MCA, auto_shoot_SCA);
                }

                //others

                if (Input.GetButtonDown("Use"))
                {
                    ac.PickUpItem();
                    //ac.redeploying = true;
                }
            }
            else
            {
                //AI flying

                if (Time.time % (0.25f + (5 - evasion)/4) < 0.2f)
                {
                    rng_factor = Random.Range(-1f, 1f);
                }

                aim_sway = (Mathf.Sin(2 * Time.time) + Mathf.Cos(1 / 2 * Time.time)) * (100 - precission)/100 + 1;


                auto_shoot_MCA = true;
                auto_shoot_SCA = true;

                if(GetClosestAlly() && (GetClosestAlly().rb.position - ac.rb.position).magnitude < 1)
                {
                    CruiseAway(GetClosestAlly().rb.position);
                }

                switch (currentGoal)
                {
                    case Goal.Escort:
                        currentState = State.Following;
                        break;

                    case Goal.SearchAndDestroy:

                        if (GetClosestEnemy() != null && Vector3.Distance(GetClosestEnemy().transform.position, transform.position) < awareness_range)
                        {
                            currentState = State.Attacking;
                        }
                        else
                        {
                            if (Leader == gameObject)
                            {
                                currentState = State.Patrolling;
                            }
                            else
                            {
                                currentState = State.Following;
                            }
                        }
                        break;

                    case Goal.None:
                        break;
                }

                AutoTurret();

                switch (currentState)
                {
                    case State.Dormant:
                        break;

                    case State.Attacking:
                        if (GetClosestEnemy() != null)
                        {
                            Engage(GetClosestEnemy());
                        }
                        break;

                    case State.Avoiding:
                        break;

                    case State.Following:
                        if (Leader)
                        {
                            Guard(Leader.ac);
                        } else
                        {
                            Engage(GetClosestEnemy());
                        }
                        break;

                    case State.Patrolling:
                        Patrol(Station.gameObject);
                        break;
                }
            }
        } else if (knocked_out)
        {
            ac.inputH = 0;
            ac.inputV = 0;
        } else if (Leader == gameObject || ac.isDead())
        {
            //Station.ChangeSquadLead();
            
        }

        //pilot knockout state


        //float cente_force = Mathf.Pow(ac.rb.velocity.magnitude, 2) * Mathf.Abs(ac.rb.angularVelocity * Mathf.Deg2Rad) / 100f;  //realistic

        float cente_force = Mathf.Abs(ac.rb.angularVelocity * Mathf.Deg2Rad) * 2;    //balanced

        if (cente_force > knockout_threshold)
        {
            pilot_consciousness -= cente_force * Time.deltaTime;
            
        } else
        {
            pilot_consciousness += pilot_wake_up_rate * Time.deltaTime;
        }

        pilot_consciousness = Mathf.Clamp(pilot_consciousness, 0, pilot_cons_max);

        if (knocked_out && pilot_consciousness / pilot_cons_max >= 1) { knocked_out = false; }
        if (!knocked_out && pilot_consciousness / pilot_cons_max <= 0) { knocked_out = true; }

        if (manual)
        {
            if (knocked_out)
            {
                Camera.main.GetComponent<CameraControler>().consciousness = 0;
            }
            else
            {
                Camera.main.GetComponent<CameraControler>().consciousness = pilot_consciousness / pilot_cons_max;
            }
            
        }
    }

    AircraftController GetClosestEnemy(Vector3 selfPosition = default)
    {
        if (selfPosition == default)
        {
            selfPosition = transform.position;
        }

        AircraftController[] Target = System.Array.FindAll(FindObjectsOfType<AircraftController>(), x => x.team != team);

        if (Target.Length > 0)
        {
            float minDistance = Mathf.Infinity;
            int minIndex = 0;
            for (int i = 0; i < Target.Length; i++)
            {
                if (Vector3.Distance(selfPosition, Target[i].transform.position) <= minDistance)
                {
                    minDistance = Vector3.Distance(selfPosition, Target[i].transform.position);
                    minIndex = i;
                }
            }
            return Target[minIndex];
        }
        return null;
        
    }

    AircraftController GetClosestAlly(Vector3 selfPosition = default)
    {
        AircraftController[] Ally = System.Array.FindAll(FindObjectsOfType<AircraftController>(), x => x.team == ac.team);
        if (Ally.Length > 0)
        {
            float AminDistance = Vector3.Distance(transform.position, Ally[0].transform.position);
            int AminIndex = 0;

            for (int x = 0; x < Ally.Length; x++)
            {
                if (Ally[x] != ac && Vector3.Distance(transform.position, Ally[x].transform.position) <= AminDistance)
                {
                    AminDistance = Vector3.Distance(transform.position, Ally[x].transform.position);
                    AminIndex = x;
                    if (AminDistance < 1)
                    {
                        return Ally[AminIndex];

                    }
                }
            }
        }
        return null;
    }
    Vector2 Distance2(Vector2 Target, Vector2 origin_pos = default)
    {
        if (origin_pos == default)
        {
            origin_pos = ac.rb.position;
        }

        return (new Vector2(Target.x, Target.y) - origin_pos);
        
    }

    private Vector2 predictedPosition(AircraftController Target, Vector2 shooterPosition, float projectileSpeed, float predict_factor = 1)
    {
        Rigidbody2D targetRigidbody = Target.rb;
        Vector2 targetVelocity = targetRigidbody.velocity - ac.rb.velocity;
        Vector2 targetPosition = Target.rb.position;

        Vector2 displacement = targetPosition - shooterPosition;
        float targetMoveAngle = Vector2.Angle(-displacement, targetRigidbody.velocity) * Mathf.Deg2Rad;
        //projectileSpeed += ac.rb.velocity.magnitude;
        

        //if the target is stopping or if it is impossible for the projectile to catch up with the target (Sine Formula)
        if (targetVelocity.magnitude == 0 || targetVelocity.magnitude > projectileSpeed && Mathf.Sin(targetMoveAngle) / projectileSpeed > Mathf.Cos(targetMoveAngle) / targetVelocity.magnitude)
        {
            
            return targetPosition;
        }
        //also Sine Formula
        float shootAngle = Mathf.Asin(Mathf.Sin(targetMoveAngle) * targetVelocity.magnitude / projectileSpeed);

        shootAngle = 0;
        /// Mathf.Sin(Mathf.PI - targetMoveAngle - shootAngle) * Mathf.Sin(shootAngle) / targetVelocity.magnitude
        Vector2 finalTargetPosition = targetPosition
                                    + targetVelocity * displacement.magnitude / projectileSpeed * predict_factor;

        Debug.DrawLine(shooterPosition, finalTargetPosition);
        return finalTargetPosition;
    }

    private void Engage(AircraftController Target)
    {
        Vector2 targetPos = Target.rb.position;

        Vector2 deltaPos = targetPos - ac.rb.position;

        Vector2 tangentVector = new Vector2(-deltaPos.y, deltaPos.x).normalized;

        CruiseTowards(-deltaPos.normalized * main_arnament.range * timidness + targetPos + tangentVector * rng_factor );

    }

    private void RotateTowards(Vector2 Tpos, float offset = default)
    {
        if (!ac.MovementOverride)
        {
            ac.RotateTowards(Tpos, offset);
        }
    }

    private void CruiseTowards(Vector2 Tpos, float speedFactor = 1)
    {
        if (!ac.MovementOverride)
        {
            ac.CruiseTowards(Tpos, speedFactor);
        }
    }

    private void CruiseAway(Vector2 Tpos, float speedFactor = 1)
    {
        Tpos -= ac.rb.position;

        if (!ac.MovementOverride)
        {
            ac.CruiseTowards(-Tpos + ac.rb.position, speedFactor);
        }
    }

    private void AutoTurret(bool MCA_auto = true, bool SCA_auto = true)
    {

        ArnamentP[] guns = ac.GetComponentsInChildren<ArnamentP>();
        foreach (var gun in guns)
        {
            if (MCA_auto && gun.tag == "MCArnament" || SCA_auto && gun.tag == "SCArnament" )
            {
                AircraftController Target = GetClosestEnemy(gun.transform.position);
                Vector2 target_pos;
                float target_distance, m_angle;
                if (Target != null)
                {
                    target_pos = predictedPosition(Target, gun.transform.position, gun.projectile_speed, aim_sway);
                    Vector2 target_dis = target_pos - new Vector2(gun.transform.position.x, gun.transform.position.y);
                    m_angle = Mathf.Atan2(target_dis.y, target_dis.x) * Mathf.Rad2Deg + 270f;
                    target_distance = target_dis.magnitude;
                    gun.targetVector = target_pos;
                } else
                {
                    m_angle = ac.rb.rotation;
                    target_distance = Mathf.Infinity;
                }

                if (m_angle < -0) { m_angle += 360; }
                if (m_angle > 360) { m_angle -= 360; }

                if (gun.ammo > 0 && Mathf.Abs(m_angle - gun.transform.rotation.eulerAngles.z) < 10)
                {
                    if (target_distance <= gun.range)
                    {
                        gun.Activate();
                    }
                }
            }
            
        }
        
    }

    private void ManualTurret(bool MCA, bool SCA, bool MCA_auto, bool SCA_auto)
    {
        foreach (var gun in ac.GetComponentsInChildren<ArnamentP>())
        {
            if (manual && gun != null)
            {
                if (gun.tag == "MCArnament" && !MCA_auto || gun.tag == "SCArnament" && !SCA_auto)
                {
                    gun.targetVector = gun_point_target;

                    if (gun.ammo > 0)
                    {
                        if (gun.tag == "MCArnament" && MCA || gun.tag == "SCArnament" && SCA)
                        {
                            gun.Activate();
                        }
                        
                    }
                }
            }
        }
    }

    private void Guard(AircraftController Target)
    {
        Vector2 offset = Vector2.right * 5f;
        CruiseTowards((Vector2)Target.transform.position + offset);
        
    }

    private void Patrol(GameObject Area, float patrol_radius = 30f)
    {
        float angularSpeed = ac.maxDeltaSpeed / patrol_radius;
        Vector2 CircleOffset = new Vector2(Mathf.Cos(angularSpeed), Mathf.Sin(angularSpeed));
        CruiseTowards((Vector2)Area.transform.position + CircleOffset*patrol_radius);
    }

    void OnDestroy()
    {
        if (Station != null && Station.gameObject.activeSelf)
        {
            if (Leader == gameObject)
            {
                Station.RefreshSquadLead();
            }
            Station.SquadMates.Find(x => x.Aircraft == ac).alive = false;

        }
    }

}
