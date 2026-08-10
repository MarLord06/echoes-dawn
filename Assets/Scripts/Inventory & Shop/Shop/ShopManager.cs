using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    
    [SerializeField] private ShopSlot[] shopSlots;

    [SerializeField] private InventoryManager inventoryManager;



    public void PopulateShopItems(List<ShopItems> shopItems)
    {
        for (int i = 0; i < shopItems.Count && i < shopSlots.Length; i++)
        {
            ShopItems shopItem = shopItems[i];
            shopSlots[i].Initialize(shopItem.itemSO, shopItem.price);
            shopSlots[i].gameObject.SetActive(true);
        }

        for (int i = shopItems.Count; i < shopSlots.Length; i++)
        {
            shopSlots[i].gameObject.SetActive(false);
        }
    }

    public bool TryBuyItem(ItemSO itemSO, int price)
    {
        if (itemSO == null || !ShopTransactionPolicy.CanBuy(inventoryManager.gold, price, HasSpaceForItem(itemSO)))
        {
            return false;
        }

        inventoryManager.gold -= price;
        inventoryManager.goldText.text = inventoryManager.gold.ToString();
        inventoryManager.AddItem(itemSO, 1);
        GameManager.Instance.UpdateData();
        return true;
    }

    private bool HasSpaceForItem(ItemSO itemSO)
    {
        foreach (var slot in inventoryManager.itemSlots)
        {
            if (slot.itemSO == itemSO && slot.quantity < itemSO.stackSize)
                return true;
            else if (slot.itemSO == null)
                return true;
        }
        return false;
    }




    public bool TrySellItem(ItemSO itemSO)
    {
        if (itemSO == null)
        {
            return false;
        }

        ShopSlot matchingSlot = null;
        foreach (ShopSlot slot in shopSlots)
        {
            if (slot.itemSO == itemSO)
            {
                matchingSlot = slot;
                break;
            }
        }

        if (!ShopTransactionPolicy.CanSell(matchingSlot != null))
        {
            return false;
        }

        inventoryManager.gold += matchingSlot.price - 1;
        inventoryManager.goldText.text = inventoryManager.gold.ToString();
        return true;
    }

}

[System.Serializable]
public class ShopItems
{
    public ItemSO itemSO;
    public int price;
}
