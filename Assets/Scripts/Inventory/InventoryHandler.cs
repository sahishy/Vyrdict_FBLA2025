using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryHandler : MonoBehaviour
{
    public static InventoryHandler instance;
    private Dictionary<Buildable, ItemData> inventory = new Dictionary<Buildable, ItemData>();
    public int maxCapacity;

    [Header("References")]
    public List<Buildable> testItems;
    public GameObject inventoryItemPrefab;
    public Transform inventoryItemHolder;
    [SerializeField] private TMP_Text capacityText;

    void Awake() {
        instance = this;
    }

    void Start() {
        foreach(Buildable item in testItems) {
            TryAddItem(item, 2);
        }
    }
    
    public bool TryAddItem(Buildable buildable, int _amount = 1) {

        if(inventory.Count < maxCapacity) {
            if(inventory.ContainsKey(buildable)) {

                inventory[buildable].amount += _amount;

            } else {

                GameObject itemUI = Instantiate(inventoryItemPrefab, inventoryItemHolder);
                itemUI.GetComponent<InventoryItemUI>().Initialize(buildable);         

                inventory.Add(buildable, new ItemData() {
                    amount = _amount,
                    itemUI = itemUI.GetComponent<InventoryItemUI>()
                });

            }
            
            inventory[buildable].itemUI.UpdateAmount(inventory[buildable].amount);

            UpdateUI();        

            return true;    
        } else {
            return false;
        }

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

        UpdateUI();
    }

    private void UpdateUI() {
        int amount = inventory.Count;
        capacityText.text = $"{amount}/{maxCapacity}";
        capacityText.color = amount == maxCapacity ? new Color32(255, 255, 255, 100) : new Color32(255, 255, 255, 255);
        capacityText.gameObject.SetActive(amount > 0);
    }
}

public class ItemData {
    public int amount;
    public InventoryItemUI itemUI;
}
