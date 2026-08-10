using System.Collections.Generic;
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
        if (itemSO == null || quantity <= 0)
        {
            return;
        }

        if (itemSO.isGold)
        {
            gold += quantity;
            scoreManager.AddGold(quantity); // Actualizar el total de oro en el ScoreManager
            goldText.text = gold.ToString();
            GameManager.Instance.UpdateData();
            return;
        }

        var allocations = new List<InventoryStackAllocation>(itemSlots.Length);
        foreach (InventorySlot slot in itemSlots)
        {
            allocations.Add(new InventoryStackAllocation(
                slot.itemSO == itemSO,
                slot.itemSO == null,
                slot.quantity));
        }

        int remainder = InventoryStackAllocator.Allocate(quantity, itemSO.stackSize, allocations);

        for (int i = 0; i < itemSlots.Length; i++)
        {
            InventorySlot slot = itemSlots[i];
            InventoryStackAllocation allocation = allocations[i];

            if (slot.quantity == allocation.Quantity)
            {
                continue;
            }

            if (slot.itemSO == null && allocation.Quantity > 0)
            {
                slot.itemSO = itemSO;
            }

            slot.quantity = allocation.Quantity;
            slot.UpdateUI();
        }

        if (remainder > 0)
        {
            DropLoot(itemSO, remainder);
        }
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
