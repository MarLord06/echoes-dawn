using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int enemiesDefeated;
    public int totalLevel;
    public int totalGold;

    public void AddEnemyDefeated()
    {
        enemiesDefeated++;
        GameManager.Instance.UpdateData();
    }

    public void AddGold(int amount)
    {
        totalGold += amount;
        GameManager.Instance.UpdateData();
    }

    public void UpdateLevel(int level)
    {
        totalLevel = level;
        GameManager.Instance.UpdateData();
    }
}
