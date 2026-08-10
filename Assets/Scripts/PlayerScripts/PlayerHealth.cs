using TMPro;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    
    public TMP_Text healthText;
    public Animator healthTextAnim;

    private void Start()
    {
        healthText.text = "HP: " + StatsManager.Instance.currentHealth + " / " + StatsManager.Instance.maxHealth;
    }

    public void ChangeHealth(int amount)
    {
        StatsManager.Instance.currentHealth += amount;
        healthTextAnim.Play("TextUpdate");

        healthText.text = "HP: " + StatsManager.Instance.currentHealth + " / " + StatsManager.Instance.maxHealth;

        if (StatsManager.Instance.currentHealth <= 0)
        {
            gameObject.SetActive(false);
            StatsManager.Instance.currentHealth = 0;
            healthText.text = "HP: 0 / " + StatsManager.Instance.maxHealth;
        }

        GameManager.Instance.UpdateData();
    }	

}
