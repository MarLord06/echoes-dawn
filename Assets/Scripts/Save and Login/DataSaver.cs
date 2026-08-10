using UnityEngine;
using System;
using Firebase.Database;
using System.Collections;
using UObj = UnityEngine.Object;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

[Serializable]
public class DroppedItemData
{
    public string itemID;
    public int quantity;
    public string uniqueID;
    public Vector3 position;
}

[Serializable]
public class dataToSave
{
    [Header("Player Stats")]
    public int maxHealth;
    public int currentHealth;
    public int damage;
    public float speed;

    [Header("Level and Experience")]
    public int level;
    public int currentExp;
    public int expToLevel;

    [Header("Skill Tree")]
    public int skillTreeVersion;
    public int skillPoints;
    public int speedSkillLevel;
    public int damageSkillLevel;
    public int experienceSkillLevel;
    public int healthSkillLevel;

    [Header("Player Position")]
    public float playerPosX;
    public float playerPosY;
    public float playerPosZ = 0f;

    [Header("Sword State")]
    public bool isSwordEquipped;

    [Header("Inventory")]
    public string[] itemIDs;
    public int[] itemQuantities;
    public int gold;

    [Header("Scene Info")]
    public string currentScene;

    [Header("Dropped Items")]
    public DroppedItemData[] droppedItems;

    [Header("Score")]
    public int enemiesDefeated;
    public int totalLevel;
    public int totalGold;

    [Header("Playtime")]
    public float totalPlaytimeInSeconds;

    public void SyncFromGameManager()
    {
        StatsManager stats = GameManager.Instance.persistentObjects[6].GetComponent<StatsManager>();
        ExpManager exp = GameManager.Instance.persistentObjects[3].GetComponent<ExpManager>();
        GameObject player = GameManager.Instance.persistentObjects[1];
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        ScoreManager score = GameManager.Instance.persistentObjects[10].GetComponent<ScoreManager>();
        InventoryManager inventoryManager = GameManager.Instance.persistentObjects[4].GetComponent<InventoryManager>();
        PlaytimeTracker playtime = GameManager.Instance.persistentObjects[15].GetComponent<PlaytimeTracker>();

        maxHealth = stats.maxHealth;
        currentHealth = stats.currentHealth;
        damage = stats.damage;
        speed = stats.speed;

        level = exp.level;
        currentExp = exp.currentExp;
        expToLevel = exp.expToLevel;

        SkillTreeManager skillTree = SkillTreeManager.Instance;
        if (skillTree != null)
        {
            skillTreeVersion = SkillTreeManager.SaveVersion;
            skillPoints = skillTree.AvailableSkillPoints;
            speedSkillLevel = skillTree.SpeedSkillLevel;
            damageSkillLevel = skillTree.DamageSkillLevel;
            experienceSkillLevel = skillTree.ExperienceSkillLevel;
            healthSkillLevel = skillTree.HealthSkillLevel;
        }

        playerPosX = player.transform.position.x;
        playerPosY = player.transform.position.y;
        playerPosZ = player.transform.position.z;

        isSwordEquipped = playerMovement.isSwordEquipped;

        enemiesDefeated = score.enemiesDefeated;
        totalLevel = score.totalLevel;

        gold = inventoryManager.gold;
        totalGold = gold;
        score.totalGold = totalGold;
        itemIDs = new string[inventoryManager.itemSlots.Length];
        itemQuantities = new int[inventoryManager.itemSlots.Length];

        for (int i = 0; i < inventoryManager.itemSlots.Length; i++)
        {
            if (inventoryManager.itemSlots[i].itemSO != null)
            {
                itemIDs[i] = inventoryManager.itemSlots[i].itemSO.itemID;
                itemQuantities[i] = inventoryManager.itemSlots[i].quantity;
            }
            else
            {
                itemIDs[i] = null;
                itemQuantities[i] = 0;
            }
        }

        var lootInWorld = UObj.FindObjectsByType<Loot>(FindObjectsSortMode.None);
        droppedItems = new DroppedItemData[lootInWorld.Length];

        for (int k = 0; k < lootInWorld.Length; k++)
        {
            droppedItems[k] = new DroppedItemData
            {
                itemID = lootInWorld[k].itemSO.itemID,
                quantity = lootInWorld[k].quantity,
                uniqueID = lootInWorld[k].uniqueID,
                position = lootInWorld[k].transform.position
            };
        }

        currentScene = SceneManager.GetActiveScene().name;
        totalPlaytimeInSeconds = playtime.playtimeInSeconds;
    }
}

public class DataSaver : MonoBehaviour
{
    public dataToSave dts;
    public string userId;
    DatabaseReference dbRef;
    public SceneChanger sceneChanger;
    public GameObject lootPrefab;
    public Menu menu;
    public RespawnMenu respawnMenu;

    private FirebaseAuthManager firebaseAuthManager;
    private bool isSaving;
    private bool saveQueued;
    private bool closeMenuAfterSave;

    public bool LastSaveSucceeded { get; private set; }

    IEnumerator Start()
    {
        // Esperar hasta que Firebase esté listo
        yield return new WaitUntil(() => FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsFirebaseReady);

        firebaseAuthManager = FirebaseAuthManager.Instance;
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dts = new dataToSave();
        dts.SyncFromGameManager();

        sceneChanger = FindFirstObjectByType<SceneChanger>();

        GameManager.Instance.OnDataChanged += HandleDataChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnDataChanged -= HandleDataChanged;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        sceneChanger = FindFirstObjectByType<SceneChanger>();
    }

    void HandleDataChanged()
    {
        dts.SyncFromGameManager();
    }

    public void SaveDataFn()
    {
        Debug.Log("Saving");
        RequestSave(true);
    }

    public void SaveDataSilently()
    {
        RequestSave(false);
    }

    public IEnumerator SaveAndWait(bool closeMenu = false)
    {
        RequestSave(closeMenu);
        while (isSaving)
        {
            yield return null;
        }
    }

    private void RequestSave(bool closeMenu)
    {
        closeMenuAfterSave |= closeMenu;
        saveQueued = true;

        if (isSaving)
        {
            return;
        }

        isSaving = true;
        StartCoroutine(ProcessSaveQueue());
    }

    private IEnumerator ProcessSaveQueue()
    {
        while (saveQueued)
        {
            saveQueued = false;
            LastSaveSucceeded = false;

            if (firebaseAuthManager == null)
            {
                firebaseAuthManager = FirebaseAuthManager.Instance;
            }

            if (dbRef == null && firebaseAuthManager != null && firebaseAuthManager.IsFirebaseReady)
            {
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            }

            if (firebaseAuthManager == null || firebaseAuthManager.user == null || dbRef == null)
            {
                Debug.LogError("No user is authenticated. Cannot save data.");
                continue;
            }

            if (dts == null)
            {
                dts = new dataToSave();
            }

            dts.SyncFromGameManager();
            dts.currentScene = SceneManager.GetActiveScene().name;
            string json = JsonUtility.ToJson(dts);
            userId = firebaseAuthManager.user.UserId;

            var saveTask = dbRef.Child("users").Child(userId).SetRawJsonValueAsync(json);
            yield return new WaitUntil(() => saveTask.IsCompleted);

            if (saveTask.IsCanceled)
            {
                Debug.LogError("Firebase canceled the save operation.");
            }
            else if (saveTask.IsFaulted)
            {
                Debug.LogError("Firebase save failed: " + saveTask.Exception);
            }
            else
            {
                LastSaveSucceeded = true;
                Debug.Log("Partida guardada correctamente en Firebase.");
            }
        }

        bool shouldCloseMenu = closeMenuAfterSave;
        closeMenuAfterSave = false;
        isSaving = false;

        if (shouldCloseMenu && menu != null && menu.isMenuOpen)
        {
            StartCoroutine(menu.CloseMenu());
        }
    }

    public void LoadDataFn()
    {
        Debug.Log("Loading");
        StartCoroutine(LoadDataEnum());
    }

    IEnumerator LoadDataEnum()
    {
        StartCoroutine(menu.CloseMenu());

        if (sceneChanger != null)
        {
            StartCoroutine(menu.CloseMenu());

            if (respawnMenu != null && respawnMenu.isRespawnOpen)
            {
                respawnMenu.SetLoadingState(true);
                StartCoroutine(respawnMenu.CloseRespawnMenu());
            }
        }

        if (firebaseAuthManager != null && firebaseAuthManager.user != null)
        {
            userId = firebaseAuthManager.user.UserId;
            var serverData = dbRef.Child("users").Child(userId).GetValueAsync();
            yield return new WaitUntil(() => serverData.IsCompleted);

            if (serverData.Result.Exists)
            {
                dts = JsonUtility.FromJson<dataToSave>(serverData.Result.GetRawJsonValue());
                if (dts == null)
                {
                    Debug.LogError("Los datos guardados no tienen un formato valido.");
                    yield break;
                }

                sceneChanger.FadeAnim.Play("FadeToBlack");
                yield return new WaitForSeconds(sceneChanger.fadeTime);

                if (dts.currentScene != SceneManager.GetActiveScene().name)
                {
                    SceneManager.LoadScene(dts.currentScene);
                    yield return new WaitForSeconds(0.1f);
                    sceneChanger = FindFirstObjectByType<SceneChanger>();
                }

                GameObject player = GameManager.Instance.persistentObjects[1];
                player.SetActive(true);

                ApplyToGameManager();

                if (sceneChanger != null)
                {
                    sceneChanger.FadeAnim.Play("FadeFromBlack");
                    yield return new WaitForSeconds(sceneChanger.fadeTime);
                }

                if (menu != null)
                {
                    StartCoroutine(menu.CloseMenu());
                }
            }
            else
            {
                Debug.LogWarning("No hay datos guardados para este usuario.");
                yield return new WaitForSeconds(1f);
                StartCoroutine(menu.CloseMenu());
            }
        }
        else
        {
            Debug.LogError("No user is authenticated. Cannot load data.");
        }

        if (respawnMenu != null)
        {
            respawnMenu.SetLoadingState(false);
        }
    }

    public void ApplyToGameManager()
    {
        bool migratedLegacySkillTree = false;
        StatsManager stats = GameManager.Instance.persistentObjects[6].GetComponent<StatsManager>();
        ExpManager exp = GameManager.Instance.persistentObjects[3].GetComponent<ExpManager>();
        InventoryManager inventoryManager = GameManager.Instance.persistentObjects[4].GetComponent<InventoryManager>();
        ScoreManager score = GameManager.Instance.persistentObjects[10].GetComponent<ScoreManager>();
        PlaytimeTracker playtime = GameManager.Instance.persistentObjects[15].GetComponent<PlaytimeTracker>();
        GameObject player = GameManager.Instance.persistentObjects[1];
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        stats.maxHealth = dts.maxHealth;
        stats.currentHealth = dts.currentHealth;
        stats.damage = dts.damage;
        stats.speed = dts.speed > 0f ? dts.speed : 8.6f;
        stats.RefreshHealthUI();

        player.SetActive(true);
        player.transform.position = new Vector3(dts.playerPosX, dts.playerPosY, dts.playerPosZ);

        var vcam = UnityEngine.Object.FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = player.transform;
            StartCoroutine(sceneChanger.ForceCamNextFrame(vcam, player.transform.position));
        }

        exp.level = dts.level;
        exp.currentExp = dts.currentExp;
        exp.expToLevel = dts.expToLevel;
        exp.UpdateUI();

        if (SkillTreeManager.Instance != null)
        {
            migratedLegacySkillTree = SkillTreeManager.Instance.LoadProgress(
                dts.skillTreeVersion,
                dts.skillPoints,
                dts.speedSkillLevel,
                dts.damageSkillLevel,
                dts.experienceSkillLevel,
                dts.healthSkillLevel,
                exp.level);
        }

        if (dts.isSwordEquipped)
        {
            StartCoroutine(playerMovement.EquipSword());
        }
        else
        {
            StartCoroutine(playerMovement.UnequipSword());
        }

        for (int i = 0; i < inventoryManager.itemSlots.Length; i++)
        {
            if (dts.itemIDs[i] != null)
            {
                ItemSO itemSO = ItemDatabase.GetItemById(dts.itemIDs[i]);
                if (itemSO != null)
                {
                    inventoryManager.itemSlots[i].itemSO = itemSO;
                    inventoryManager.itemSlots[i].quantity = dts.itemQuantities[i];
                    inventoryManager.itemSlots[i].UpdateUI();
                }
            }
            else
            {
                inventoryManager.itemSlots[i].itemSO = null;
                inventoryManager.itemSlots[i].quantity = 0;
                inventoryManager.itemSlots[i].UpdateUI();
            }
        }

        inventoryManager.gold = dts.gold;
        inventoryManager.goldText.text = inventoryManager.gold.ToString();

        score.enemiesDefeated = dts.enemiesDefeated;
        score.totalLevel = dts.totalLevel;
        score.totalGold = dts.totalGold;

        playtime.StopTracking(); // Evita acumular tiempo antes de aplicar

        playtime.SetPlaytime(dts.totalPlaytimeInSeconds);

        playtime.StartTracking();

        Loot[] existingLoot = FindObjectsByType<Loot>(FindObjectsSortMode.None);
        foreach (Loot loot in existingLoot)
        {
            if (loot.isDropped)
            {
                Destroy(loot.gameObject);
            }
        }

        if (dts.droppedItems != null)
        {
            for (int j = 0; j < dts.droppedItems.Length; j++)
            {
                DroppedItemData itemData = dts.droppedItems[j];
                ItemSO itemSO = ItemDatabase.GetItemById(itemData.itemID);

                if (itemSO != null && lootPrefab != null)
                {
                    GameObject newLoot = Instantiate(lootPrefab, itemData.position, Quaternion.identity);
                    Loot loot = newLoot.GetComponent<Loot>();
                    loot.Initialize(itemSO, itemData.quantity);
                    loot.uniqueID = itemData.uniqueID;
                    loot.isDropped = true;
                }
            }
        }

        if (migratedLegacySkillTree)
        {
            SaveDataSilently();
        }
    }
}
