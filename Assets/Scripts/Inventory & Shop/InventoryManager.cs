using TMPro;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] itemSlots;
    public UseItem useItem;
    public int gold;
    public TMP_Text goldText;
    public GameObject lootPrefab;
    public Transform player;

    public ScoreManager scoreManager;

 

    private void Start()
    {
        foreach (var slot in itemSlots)
        {
            slot.UpdateUI();
        }

        goldText.text = gold.ToString();
    }

    private void OnEnable()
    {
        Loot.OnItemLooted += AddItem;
    }

    private void OnDisable()
    {
        Loot.OnItemLooted -= AddItem;
    }

    public void AddItem(ItemSO itemSO, int quantity)
    {
        if (itemSO.isGold)
        {
            gold += quantity;
            scoreManager.AddGold(quantity); // Actualizar el total de oro en el ScoreManager
            goldText.text = gold.ToString();
            GameManager.Instance.UpdateData();
            return;
        }

        foreach (var slot in itemSlots) // It is the same item and there is room left
        {
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
            {
                int availableSpace = itemSO.stackSize - slot.quantity;
                int amountToAdd = Mathf.Min(quantity, availableSpace);

                slot.quantity += amountToAdd;
                quantity -= amountToAdd;

                slot.UpdateUI();

                if (quantity <= 0)
                {
                    return; // All items added
                }
            }
        }

        foreach (var slot in itemSlots) // If items remain we will now look at the empty slots
        {
            if (slot.itemSO == null)
            {
                int amountToAdd = Mathf.Min(itemSO.stackSize, quantity);
                slot.itemSO = itemSO;
                slot.quantity = amountToAdd;
                slot.UpdateUI();
                return;
            }
        }

        if (quantity > 0)
            DropLoot(itemSO, quantity);
    }

    public void DropItem(InventorySlot slot)
    {
        DropLoot(slot.itemSO, 1);
        slot.quantity--;
        if (slot.quantity <= 0)
        {
            slot.itemSO = null;
        }
        slot.UpdateUI();
    }

    private void DropLoot(ItemSO itemSO, int quantity)
    {
        Loot loot = Instantiate(lootPrefab, player.position, Quaternion.identity).GetComponent<Loot>();
        loot.Initialize(itemSO, quantity);
        loot.isDropped = true;
    }

    public void UseItem(InventorySlot slot)
    {
        if (slot.itemSO != null && slot.quantity > 0)
        {
            useItem.ApplyItemEffects(slot.itemSO);

            slot.quantity--;
            if (slot.quantity <= 0)
            {
                slot.itemSO = null;
            }
            slot.UpdateUI();
        }
    }

    // Método para limpiar el inventario
    public void ClearInventory()
    {
        foreach (var slot in itemSlots)
        {
            slot.itemSO = null; // Limpia el objeto del ítem
            slot.quantity = 0; // Restablece la cantidad a 0
            slot.UpdateUI(); // Actualiza la interfaz de usuario
        }
    }
}
