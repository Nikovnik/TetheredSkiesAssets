using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Deployment : MonoBehaviour
{
    public bool player_spawn = false;

    public string codename = "";

    [HideInInspector]
    private FactionManager tm;
    public int team_index;
    private FactionManager.Team team;

    public bool in_air = true;

    [System.Serializable]
    public class SquadMate        //squad blueprint
    {
        public string Craft;
        public string Pilot;
        public string sMCA;
        public string sSCA;
        public string[] Ability;
        public string[] Mods;

        //[HideInInspector]
        public RectTransform EditUI;
        public bool alive = false;
        public AircraftController Aircraft;
    }

    public PilotScript.Goal SquadGoal;

    public int ArnamentSupply = 120;

    public int spawn_cycle = 120;
    private bool activated = false;
    public bool playerAlive = false;

    public float spawn_delay = 0f;

    public int spawn_points = 0;
    private int count;

    public List<SquadMate> SquadMates;

    private AudioSource sfx;

    public PilotScript leader;

    ObjectManager CraftManager;

    ObjectManager WeaponManager;

    ObjectManager ModManager;

    public ScrollRect SquadEditPanel;
    public RectTransform MemberEdit;

    // Start is called before the first frame update
    void Awake()
    {
        if(spawn_points == 0)
        {
            spawn_points = SquadMates.Count;
        }

        tm = FindObjectOfType<FactionManager>();
        team = tm.Teams[team_index];

        sfx = GetComponent<AudioSource>();

        ObjectManager[] Managers = FindObjectsOfType<ObjectManager>();

        foreach (var manager in Managers)
        {
            if (manager.name == "CraftManager")
            {
                CraftManager = manager;
            }
            if (manager.name == "WeaponManager")
            {
                WeaponManager = manager;
            }
            if (manager.name == "ModManager")
            {
                ModManager = manager;
            }
        }
    }

    private void Start()
    {
        foreach (SquadMate mate in SquadMates)
        {
            if (mate.Craft != "") { ArnamentSupply -= CraftManager.SearchObjectManager(mate.Craft).ArnamentPoints; }
            if (mate.sMCA != "") { ArnamentSupply -= WeaponManager.SearchObjectManager(mate.sMCA).ArnamentPoints; }
            if (mate.sSCA != "") { ArnamentSupply -= WeaponManager.SearchObjectManager(mate.sSCA).ArnamentPoints; }
            
        }

        if (SquadEditPanel != null)
        {
            if (SquadMates.Count > SquadEditPanel.content.childCount)
            {
                int i = SquadEditPanel.content.childCount;
                for ( ; i < SquadMates.Count; i++)
                {
                    SquadMates[i].EditUI = Instantiate(MemberEdit, SquadEditPanel.content);
                    RectTransform t = SquadMates[i].EditUI;
                    t.localPosition = new Vector3(0, -MemberEdit.localScale.y * MemberEdit.rect.height * i, 0);

                    float con_width = SquadEditPanel.content.rect.width;
                    SquadEditPanel.content.sizeDelta = new Vector2(con_width, MemberEdit.localScale.y * MemberEdit.rect.height * (i + 1));
            }
            }
        }

    }



    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp("y") && SquadMates.Count > 1 && player_spawn)
        {
            ChangeSquadLead();
        }

        //squadron deployment
        

        if (!activated && spawn_points > 0 && SquadMates.FindAll(x => x.alive).Count < SquadMates.Count)
        {
            activated = true;
            StartCoroutine(StartDeploying());
            
        }

        if (!activated && player_spawn && spawn_points <= 0 && SquadMates.FindAll(x => x.alive).Count <= 0)
        {
            print("Reload");
            StartCoroutine(ReloadScene());
            activated = true;
        }
        
    }

    IEnumerator ReloadScene()
    {
        CameraControler cc = Camera.main.GetComponent<CameraControler>();
        cc.blackout = true;


        yield return new WaitForSecondsRealtime(3f);

        while (cc.consciousness > 0)
        {
            cc. consciousness = Mathf.Lerp(cc.consciousness, -2f, 1f * Time.deltaTime);
            yield return new WaitForEndOfFrame();
        }


        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator StartDeploying()
    {
        
        sfx.Play();
        for (int i = 0; i < SquadMates.Count; i++)
        {
            
            yield return new WaitForSeconds(spawn_delay);

            if (spawn_points < 1 || SquadMates.Count < 1)
            {
                break;
            }
            if (!SquadMates[i].alive)
            {
                DeployUnit(i);
                spawn_points--;
            }
            

        }
        activated = false;
    }

    void DeployUnit(int i = 0)
    {

        GameObject Unit = Instantiate<GameObject>(GetAircraft(i), transform.position + new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f)), transform.rotation);

        AircraftController ac = Unit.GetComponent<AircraftController>();

        Unit.SetActive(false);

        if (!in_air)
        {
            ac.deploying = true;
        }

        string nis = count < 10 ? "-0" : "-";
        Unit.name = codename + nis + count;

        ac.pilot.team_index = team_index;
        

        ac.pilot.Station = this;
        ac.pilot.currentGoal = SquadGoal;

        SquadMates[i].Aircraft = ac;
        SquadMates[i].alive = true;
        if(leader == null)
        {
            leader = ac.pilot;
        }


        count++;

        RefreshSquadLead();

        foreach (var gun in Unit.GetComponentsInChildren<ArnamentP>())
        {
            if (gun != null && gun.tag == "MCArnament")
            {
                EquipWeapon(gun.gameObject, SquadMates[i].sMCA);
            }
            if (gun != null && gun.tag == "SCArnament")
            {
                EquipWeapon(gun.gameObject, SquadMates[i].sSCA);
            }
        }

        Unit.SetActive(true);
    }

    public void AddSquadMate()
    {
        int i = SquadMates.Count;

        SquadMates.Add(new SquadMate { Craft = "random", sMCA = "random", sSCA = "", EditUI = Instantiate(MemberEdit, SquadEditPanel.content)});
        RectTransform t = SquadMates[i].EditUI;
        t.localPosition = new Vector3(0, -MemberEdit.localScale.y * MemberEdit.rect.height * i, 0);

        float con_width = SquadEditPanel.content.rect.width;
        SquadEditPanel.content.sizeDelta = new Vector2(con_width, MemberEdit.localScale.y * MemberEdit.rect.height * (i + 1));
        spawn_points++;
    }

    public void RemoveSquadMate()
    {
        if(SquadMates.Count > 0)
        {
            Destroy(SquadMates[SquadMates.Count - 1].EditUI.gameObject);
            SquadMates.RemoveAt(SquadMates.Count - 1);
        }

        float con_width = SquadEditPanel.content.rect.width;
        SquadEditPanel.content.sizeDelta = new Vector2(con_width, MemberEdit.localScale.y * MemberEdit.rect.height * SquadMates.Count);

        spawn_points--;
    }

    private void EquipWeapon(GameObject weaponSlot, string targetWeapon)
    {
        WeaponManager.ReplaceObject(targetWeapon, weaponSlot);
    }
    private GameObject GetAircraft(int i)
    {
        return CraftManager.SearchObjectManager(SquadMates[i].Craft).ItemGameObject;
    }

    public void ReTinker(AircraftController Unit)
    {
        int i = GetSquadID(Unit);

        AircraftController ac = Unit.GetComponent<AircraftController>();

        ac.health = ac.max_health;
        ac.shield = ac.max_shield;

        foreach (var gun in Unit.GetComponentsInChildren<ArnamentP>())
        {
            if (gun != null && gun.tag == "MCArnament")
            {
                EquipWeapon(gun.gameObject, SquadMates[i].sMCA);
            }
            if (gun != null && gun.tag == "SCArnament")
            {
                EquipWeapon(gun.gameObject, SquadMates[i].sSCA);
            }
        }
    }

    private int GetSquadID(AircraftController Unit)
    {
        return SquadMates.FindIndex(x => x.Aircraft == Unit);
    }

    private int GetSquadID(PilotScript Unit)
    {
        return SquadMates.FindIndex(x => x.Aircraft.pilot == Unit);
    }

    public void RefreshSquadLead()
    {
        foreach (var p in SquadMates)
        {
            if (p.alive && p.Aircraft != null)
            {
                PilotScript pilot = p.Aircraft.pilot;
                pilot.Leader = leader;
                if (player_spawn)
                {
                    pilot.manual = pilot == leader ? true : false;
                }
            }
        }
    }

    public void ChangeSquadLead()
    {
        int a = GetSquadID(leader);

        if (a > -1)
        {

            for (int i = 1; i < SquadMates.Count; i++)
            {
                int m = (i + a) % SquadMates.Count;
                print("m:"+m);
                if (SquadMates[m].alive && a != m)
                {
                    print(name + "'s squad lead changed from unit-" + a + " to unit-" + m);
                    leader = SquadMates[m].Aircraft.pilot;
                    break;
                }
            }
        } else
        {
            leader = SquadMates[0].Aircraft.pilot;
        }
        RefreshSquadLead();
    }

}
