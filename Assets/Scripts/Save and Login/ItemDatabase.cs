using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    public ItemSO[] allItems; // Asigna todos tus ítems en el inspector

    public static ItemSO GetItemById(string id)
    {
        foreach (var item in Instance.allItems)
        {
            if (item.itemID == id)
            {
                return item;
            }
        }
        return null; // Si no se encuentra el ítem
    }

    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<ItemDatabase>();
            }
            return _instance;
        }
    }
}
