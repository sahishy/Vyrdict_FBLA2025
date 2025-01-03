using System.Collections.Generic;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    public static InventoryHandler instance;
    private Dictionary<Buildable, ItemData> inventory = new Dictionary<Buildable, ItemData>();

    [Header("References")]
    public List<Buildable> testItems;
    public GameObject inventoryItemPrefab;
    public Transform inventoryItemHolder;

    void Awake() {
        instance = this;
    }

    void Start() {
        foreach(Buildable item in testItems) {
            AddItem(item, 10);
        }
    }
    
    public void AddItem(Buildable buildable, int _amount = 1) {

        if(inventory.ContainsKey(buildable)) {

            inventory[buildable].amount += _amount;

        } else {

            GameObject itemUI = Instantiate(inventoryItemPrefab, inventoryItemHolder);
            itemUI.GetComponent<InventoryItem>().Initialize(buildable);         

            inventory.Add(buildable, new ItemData() {
                amount = _amount,
                itemUI = itemUI.GetComponent<InventoryItem>()
            });

        }
        
        inventory[buildable].itemUI.UpdateAmount(inventory[buildable].amount);
    }

    public void RemoveItem(Buildable buildable, int _amount = 1) {

        if(inventory.ContainsKey(buildable)) {
            inventory[buildable].amount -= _amount;

            inventory[buildable].itemUI.UpdateAmount(inventory[buildable].amount);

            if(inventory[buildable].amount <= 0) {

                Destroy(inventory[buildable].itemUI.gameObject);
                inventory.Remove(buildable);

            }
        }

    }
}

public class ItemData {
    public int amount;
    public InventoryItem itemUI;
}
