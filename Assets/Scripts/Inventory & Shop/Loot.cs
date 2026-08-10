using System;
using UnityEngine;

public class Loot : MonoBehaviour
{
    public ItemSO itemSO;
    public SpriteRenderer sr; // Asigna el SpriteRenderer del hijo manualmente en el inspector
    public Animator anim;

    public bool canBePickedUp = true;
    public int quantity;
    public bool isDropped;
    public string uniqueID;
    // ID único para este objeto en el mundo

    public static event Action<ItemSO, int> OnItemLooted;

    AudioManager audioManager;

    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }

    private void OnValidate()
    {
        if (itemSO == null)
            return;

        UpdateAppearance();
        sr.transform.localScale = itemSO.spriteScale;
    }

    public void Initialize(ItemSO itemSO, int quantity)
    {
        this.itemSO = itemSO;
        this.quantity = quantity;
        canBePickedUp = false;
        uniqueID = System.Guid.NewGuid().ToString(); // Generar un ID único
        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        sr.sprite = itemSO.icon;
        this.name = itemSO.itemName;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && canBePickedUp == true)
        {
            audioManager.PlaySFX(audioManager.itemPick);
            anim.Play("LootPickup");
            OnItemLooted?.Invoke(itemSO, quantity);
            Destroy(gameObject, .5f);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canBePickedUp = true;
        }
    }
}
