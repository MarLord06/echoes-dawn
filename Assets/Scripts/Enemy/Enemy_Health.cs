using UnityEngine;

public class Enemy_Health : MonoBehaviour
{
    public int expReward = 3;

    public delegate void MonsterDefeated(int exp);
    public static event MonsterDefeated OnMonsterDefeated;

    public int currentHealth;
    public int maxHealth;

    public ScoreManager scoreManager; // Cambiar a privado

    private void Start()
    {
        currentHealth = maxHealth;

        // Obtener referencia al ScoreManager dinámicamente
        scoreManager = FindFirstObjectByType<ScoreManager>(); 
        if (scoreManager == null)
        {
            Debug.LogError("ScoreManager no encontrado en la escena.");
        }
    }

    public void ChangeHealth(int amount)
    {
        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        else if (currentHealth <= 0)
        {
            if (scoreManager != null) // Verificar que scoreManager no sea nulo
            {
                scoreManager.AddEnemyDefeated(); // Aumentar el contador de enemigos derrotados
            }
            OnMonsterDefeated(expReward);
            Destroy(gameObject);
        }
    }
}
