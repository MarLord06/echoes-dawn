using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Persistent Objects")]
    public GameObject[] persistentObjects;

    [Header("Cached References")]
    public Camera shopCamera;
    public CanvasGroup canvasGroup;
    public ShopManager shopManager;

    public event Action OnDataChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            CleanUpAndDestroy();
            return;

        }

        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            MakePersistentObjects();
            EnsureSkillTreeManager();
        }
    }

    private void EnsureSkillTreeManager()
    {
        if (GetComponent<SkillTreeManager>() == null)
        {
            gameObject.AddComponent<SkillTreeManager>();
        }
    }

    private void MakePersistentObjects()
    {
        foreach (GameObject obj in persistentObjects)
        {
            if (obj != null)
            {
                DontDestroyOnLoad(obj);

            }
        }
    }


    private void CleanUpAndDestroy()
    {
        foreach (GameObject obj in persistentObjects)
        {
            Destroy(obj);
        }

        Destroy(gameObject);
    }


    // Método para actualizar datos y notificar cambios
    public void UpdateData()
    {
        StatsManager stats = persistentObjects[6].GetComponent<StatsManager>();
        ExpManager exp = persistentObjects[3].GetComponent<ExpManager>();
        ScoreManager score = persistentObjects[10].GetComponent<ScoreManager>();
        PlaytimeTracker playtime = persistentObjects[15].GetComponent<PlaytimeTracker>(); // NUEVO

        OnDataChanged?.Invoke();
    }

}
