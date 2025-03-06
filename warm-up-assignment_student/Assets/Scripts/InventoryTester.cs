using NaughtyAttributes;
using UnityEngine;


public class InventoryTester : MonoBehaviour
{
    public GameObject inventory;

    [Button("additems")]
    void Start()
    {
        InventorySystem inventorySystem = inventory.GetComponent<InventorySystem>();
        inventorySystem.AddItem("Health");
        inventorySystem.AddItem("Mana");
        
    }

}
