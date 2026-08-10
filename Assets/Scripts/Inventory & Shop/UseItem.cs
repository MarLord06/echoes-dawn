using System.Collections;
using UnityEngine;

public class UseItem : MonoBehaviour
{

    public void ApplyItemEffects(ItemSO itemSO)
    {
        if (itemSO.currentHealth > 0)
        {
            StatsManager.Instance.UpdateHealth(itemSO.currentHealth);
            GameManager.Instance.UpdateData();
        }

        if (itemSO.maxHealth > 0)
        {
            StatsManager.Instance.UpdateMaxHealth(itemSO.maxHealth);
            GameManager.Instance.UpdateData();
        }

        if (itemSO.speed > 0)
        {
            StatsManager.Instance.UpdateSpeed(itemSO.speed);
            GameManager.Instance.UpdateData();
        }

        if (itemSO.duration > 0)
        {
            StartCoroutine(EffectTimer(itemSO, itemSO.duration));
            GameManager.Instance.UpdateData();
        }     

    }
    

    private IEnumerator EffectTimer(ItemSO itemSO, float duration)
    {
        yield return new WaitForSeconds(duration);

        if (itemSO.currentHealth > 0)
            StatsManager.Instance.UpdateHealth(-itemSO.currentHealth);

        if (itemSO.maxHealth > 0)
            StatsManager.Instance.UpdateMaxHealth(-itemSO.maxHealth);

        if (itemSO.speed > 0)
            StatsManager.Instance.UpdateSpeed(-itemSO.speed);
    }   
}
