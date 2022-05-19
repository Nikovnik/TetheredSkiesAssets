using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SquadUIManager : MonoBehaviour
{
    public ObjectManager ObjectManager;

    public Deployment SquadObject;

    public int squadmateIndex = 0;

    Dropdown dropdown;

    public string value;

    // Start is called before the first frame update
    void Awake()
    {
        dropdown = transform.GetComponent<Dropdown>();

        dropdown.options.Clear();

        foreach (var item in ObjectManager.Items)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = item.name });
        }

    }

    private void OnGUI()
    {
        value = dropdown.options[squadmateIndex].text;
    }



}
