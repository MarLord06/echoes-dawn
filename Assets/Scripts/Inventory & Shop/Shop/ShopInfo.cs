using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopInfo : MonoBehaviour
{
    
    public CanvasGroup infoPanel;

    public TMP_Text itemNameText;
    public TMP_Text itemDescriptionText;

    [Header("Stats Fields")]
    public TMP_Text[] statsTexts;

    private RectTransform infoPanelRect;

    public ItemSO itemSO;

    private void Awake()
    {
        infoPanelRect = GetComponent<RectTransform>();
    }

    public void ShowItemInfo(ItemSO itemSO)
    {
        infoPanel.alpha = 1;

        itemNameText.text = itemSO.itemName;
        itemDescriptionText.text = itemSO.itemDescription;

        if (itemSO.Usable)
        {
            List<string> stats = new List<string>();
            if (itemSO.currentHealth > 0) stats.Add("Vida: " + itemSO.currentHealth);
            if (itemSO.damage > 0) stats.Add("Daño: " + itemSO.damage);
            if (itemSO.speed > 0) stats.Add("Velocidad: " + itemSO.speed);
            if (itemSO.duration > 0) stats.Add("Duración: " + itemSO.duration);

            if (stats.Count <= 0)
                return;

            for (int i = 0; i < statsTexts.Length; i++)
            {
                if (i < stats.Count)
                {
                    statsTexts[i].text = stats[i];
                    statsTexts[i].gameObject.SetActive(true);
                }
                else
                {
                    statsTexts[i].gameObject.SetActive(false);
                }

            }
        }
        else
        {
            foreach (var statText in statsTexts)
            {
                statText.gameObject.SetActive(false);
            }
        }

    }


    public void HideItemInfo()
    {
        infoPanel.alpha = 0;
        itemNameText.text = "";
        itemDescriptionText.text = "";

    }


    public void FollowMouse()
    {
        Vector3 mousePosition = Input.mousePosition;
        Vector3 offset = new Vector3(10, -10, 0);

        infoPanelRect.position = mousePosition + offset;
    }
}
