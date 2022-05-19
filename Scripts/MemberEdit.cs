using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemberEdit : MonoBehaviour
{
    public int index = 0;

    public Image CraftPreview;
    public Text SquadName;
    public Dropdown d_craft;
    public Dropdown d_main;
    public Dropdown d_side;

    Deployment SquadDeployment;

    ObjectManager m_craft, m_main, m_side;

    // Start is called before the first frame update
    void Start()
    {
        if(transform.parent.parent.parent.parent.parent.GetComponent<Deployment>() != null)
        {
            SquadDeployment = transform.parent.parent.parent.parent.parent.GetComponent<Deployment>();
        } else
        {
            Debug.LogError("No 5th parent object that is Squad Deployment");
        }

        

        ObjectManager[] Managers = FindObjectsOfType<ObjectManager>();

        foreach (var manager in Managers)
        {
            if (manager.name == "CraftManager")
            {
                m_craft = manager;
            }
            if (manager.name == "WeaponManager")
            {
                m_main = manager;
                m_side = manager;
            }
        }

        index = transform.GetSiblingIndex();
        print(name + "'s index: " + index + "/" + SquadDeployment.SquadMates.Count);

        d_craft.ClearOptions();
        d_main.ClearOptions();
        d_side.ClearOptions();

        d_craft.options.Add(new Dropdown.OptionData() { text = "random" });
        foreach (var c in m_craft.Items)
        {
            d_craft.options.Add(new Dropdown.OptionData() { text = c.name, image = c.pic });
        }

        d_main.options.Add(new Dropdown.OptionData() { text = "" });
        d_main.options.Add(new Dropdown.OptionData() { text = "random" });
        foreach (var c in m_main.Items)
        {
            d_main.options.Add(new Dropdown.OptionData() { text = c.name });
        }

        d_side.options.Add(new Dropdown.OptionData() { text = "" });
        d_side.options.Add(new Dropdown.OptionData() { text = "random" });
        foreach (var c in m_side.Items)
        {
            d_side.options.Add(new Dropdown.OptionData() { text = c.name });
        }

        ResetDropdowns();
    }

    public void ResetDropdowns()
    {
        SquadName.text = SquadDeployment.codename + "-" + (index < 10 ? "0" + index.ToString() : index.ToString());

        int d_craft_v = d_craft.options.IndexOf(d_craft.options.Find(x => x.text == SquadDeployment.SquadMates[index].Craft));
        int d_main_v = d_main.options.IndexOf(d_main.options.Find(x => x.text == SquadDeployment.SquadMates[index].sMCA));
        int d_side_v = d_side.options.IndexOf(d_side.options.Find(x => x.text == SquadDeployment.SquadMates[index].sSCA));

        d_craft.value = d_craft_v;
        CraftPreview.sprite = d_craft.options[d_craft_v].image;
        d_main.value = d_main_v;
        d_side.value = d_side_v;
    }

    public void ApplyLoadout(int i)
    {
        switch (i)
        {
            case 0:
                SquadDeployment.SquadMates[index].Craft = d_craft.options[d_craft.value].text;
                CraftPreview.sprite = d_craft.options[d_craft.value].image;
                break;
            case 1:
                SquadDeployment.SquadMates[index].sMCA = d_main.options[d_main.value].text;
                break;
            case 2:
                SquadDeployment.SquadMates[index].sSCA = d_side.options[d_side.value].text;
                break;
        }
    }
}
