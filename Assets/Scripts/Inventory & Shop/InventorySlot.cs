using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    public ItemSO itemSO;
    public int quantity;

    public Image itemImage;
    public TMP_Text quantityText;

    private InventoryManager inventoryManager;
    private static ShopManager activeShop;
    

    private void Start()
    {
        inventoryManager = GetComponentInParent<InventoryManager>();
    }


    void OnEnable()
    {
        ShopKeeper.OnShopStateChanged += HandleShopStateChanged;
    }

    void OnDisable()
    {
        ShopKeeper.OnShopStateChanged -= HandleShopStateChanged;
    }

    private void HandleShopStateChanged(ShopManager shopManager, bool isOpen)
    {
        activeShop = isOpen ? shopManager : null;
    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (quantity > 0)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {

                if (activeShop != null)
                {
                    if (activeShop.TrySellItem(itemSO))
                    {
                        quantity--;
                        UpdateUI();
                        GameManager.Instance.UpdateData();
                    }
                }
                else
                {
                    if (itemSO.currentHealth > 0 && StatsManager.Instance.currentHealth >= StatsManager.Instance.maxHealth)
                        return;
                    if (itemSO.Usable)
                        inventoryManager.UseItem(this);
                }

            }

            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                inventoryManager.DropItem(this);
            }
        }
    }



    public void UpdateUI()
    {
        if (quantity <= 0)
            itemSO = null;
        
        if (itemSO != null)
            {
                itemImage.sprite = itemSO.icon;
                itemImage.gameObject.SetActive(true);
                quantityText.text = quantity.ToString();
            }
            else
            {
                itemImage.gameObject.SetActive(false);
                quantityText.text = "";
            }
    }
}
