using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpManager : MonoBehaviour
{
    public int level;
    public int currentExp;
    public int expToLevel = 10;
    public float expGrowthMultiplier = 1.2f; //Add 20% more EXP to level
    public Slider expSlider;
    public TMP_Text currentLevelText;

    public ScoreManager scoreManager;


    private void Start()
    {
        if (scoreManager == null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
        }

        UpdateUI();
    }

    private void OnEnable()
    {
        Enemy_Health.OnMonsterDefeated += GainExperience;
    }
    private void OnDisable()
    {
        Enemy_Health.OnMonsterDefeated -= GainExperience;
    }

    public void GainExperience(int amount)
    {
        int experienceGained = SkillTreeManager.Instance != null
            ? SkillTreeManager.Instance.ApplyExperienceBonus(amount)
            : amount;

        currentExp += experienceGained;
        while (currentExp >= expToLevel)
        {
            LevelUP();
        }

        UpdateUI();

        GameManager.Instance.UpdateData();
    }

    private void LevelUP()
    {
        level++;
        currentExp -= expToLevel;
        expToLevel = Mathf.RoundToInt(expToLevel * expGrowthMultiplier);

        if (scoreManager != null)
        {
            scoreManager.UpdateLevel(level); // Actualizar el nivel total en el ScoreManager
        }

        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.AddSkillPoint();
        }

        GameManager.Instance.UpdateData();
    }


    public void UpdateUI()
    {
        expSlider.maxValue = expToLevel;
        expSlider.value = currentExp;
        currentLevelText.text = "Nivel: " + level;
    }
}
