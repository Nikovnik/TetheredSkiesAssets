using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ObjectManager : MonoBehaviour
{
    
    [System.Serializable]
    public class Item
    {
        public string name;
        public GameObject ItemGameObject;
        public Sprite pic;
        public string desc;
        public int ArnamentPoints;
    }

    public Item[] Items;

    private void OnEnable()
    {

        foreach (Item item in Items)
        {
            if (item.name != item.ItemGameObject.name)
            {
                item.name = item.ItemGameObject.name;
                
            }
        }
    }

    public Item SearchObjectManager(string e_name, int maxPoints = 999)
    {
        if (e_name == "random")
        {
            if (maxPoints < 999)
            {
                var conditionedItems = System.Array.FindAll(Items, x => x.ArnamentPoints < maxPoints);
                if (conditionedItems.Length > 0)
                {
                    return conditionedItems[Random.Range(0, conditionedItems.Length)];
                }
                else
                {
                    return null;
                }
            } else
            {
                return Items[Random.Range(0, Items.Length)];
            }
            
        }
        else if (e_name != "")
        {
            return System.Array.Find(Items, x => x.ItemGameObject.name == e_name);

        }
        else
        {
            return null;
        }
    }

    public void AddObjectToParent(string e_name, Transform parentObject)
    {
        if (e_name != "")
        {
            GameObject e = SearchObjectManager(e_name).ItemGameObject;

            if (e != null)
            {
                //print(parentObject.name + " recieved " + e_name + " from " + name);
                Instantiate(e, parentObject);
            }
            else
            {
                Debug.LogError(e_name + " is not found in " + name + "'s list");
            }
        }
    }

    public void ReplaceObject(string e_name, GameObject targetObject, bool copyTag = true)
    {
        if (e_name != "")
        {
            GameObject e = SearchObjectManager(e_name).ItemGameObject;

            if (e != null)
            {
                //print(targetObject.name + " of parent " + targetObject.transform.parent.name + " is placed with " + e.name);
                GameObject thing = Instantiate(e, targetObject.transform.position, targetObject.transform.rotation, targetObject.transform.parent);
                thing.name = e.name;

                if (copyTag)
                {
                    thing.tag = targetObject.tag;
                }

                Destroy(targetObject);

            } else
            {
                Debug.LogError(e_name + " is not found in " + name + "'s list");
            }
        }
    }
}
