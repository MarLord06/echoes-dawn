using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemSO : ScriptableObject
{
    public string itemID; // Agregar un ID único para cada ítem
    public string itemName;
    [TextArea] public string itemDescription;
    public Sprite icon;

    public bool isGold;
    public int stackSize = 3;

    [Header("Stats")]
    public int currentHealth;
    public int maxHealth;
    public int speed;
    public int damage;

    [Header("For Temporary Items")]
    public float duration;

    [Header("For Usable Items")]
    public bool Usable = true;

    [Header("Visual Settings")]
    public Vector3 spriteScale = new Vector3(2f, 2f, 1f); // Ajusta a lo que necesites
}
