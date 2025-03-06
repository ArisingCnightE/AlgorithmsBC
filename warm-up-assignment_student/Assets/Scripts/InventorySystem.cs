using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    public Dictionary<string, int> inventory = new Dictionary<string, int>();
    
    public void Start()
    {

    }

    public void AddItem(string name)
    {
        if (inventory.ContainsKey(name))
        {
            inventory[name] += 1;
        }
        else
        {
            inventory.Add(name, 1);
        }
    }
    public void RemoveItem(string name)
    {
        if (inventory.TryGetValue(name, out int numberOfItems))
        {
            inventory[name] = --numberOfItems;
            if (numberOfItems <= 0)
            {
                inventory.Remove(name);
            }
        }
    }
    [Button("display")]
    public void DisplayInventory()
    {
        foreach (var item in inventory)
        {
            Debug.Log(item.Key + ": " + item.Value);
        }
    }
}
