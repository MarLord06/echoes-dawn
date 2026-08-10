using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class SceneChanger : MonoBehaviour
{
    public string sceneToLoad; // Para nueva partida
    public string teleportTag;
    public GameObject player;

    public Animator FadeAnim;
    public float fadeTime = 0.5f;
    public Vector2 newPlayerPosition;

    private bool shouldSaveData = false; // Variable booleana para controlar el guardado
    private bool isExiting = false;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (SceneManager.GetActiveScene().name == "MainMenu")
            newPlayerPosition = new Vector2(-14.48f, 58.18f);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        if (c.CompareTag("Player"))
        {
            GameObject teleportador = gameObject;
            string tagDelTeleportador = teleportador.tag;

            bool sceneChanged = false;

            if (SceneManager.GetActiveScene().name == "Town" && tagDelTeleportador == "Town_To_TravelPath")
            {
                newPlayerPosition = new Vector2(-4.479f, -50.32956f);
                sceneToLoad = "TravelPath";
                sceneChanged = true;
            }
            else if (SceneManager.GetActiveScene().name == "TravelPath" && tagDelTeleportador == "TravelPath_To_Outside")
            {
                newPlayerPosition = new Vector2(-3.35f, -49.63f);
                sceneToLoad = "Outside";
                sceneChanged = true;
            }
            else if (SceneManager.GetActiveScene().name == "TravelPath" && tagDelTeleportador == "TravelPath_To_Town")
            {
                newPlayerPosition = new Vector2(-4.4895f, 64.50013f);
                sceneToLoad = "Town";
                sceneChanged = true;
            }
            else if (SceneManager.GetActiveScene().name == "Outside" && tagDelTeleportador == "Outside_To_TravelPath")
            {
                newPlayerPosition = new Vector2(-4.5f, 34.1f);
                sceneToLoad = "TravelPath";
                sceneChanged = true;
            }

            if (sceneChanged)
            {
                FadeAnim.Play("FadeToBlack");
                StartCoroutine(DelayFade(c.transform));
            }
            else
            {
                Debug.LogWarning("No se pudo cambiar de escena. Verifica las etiquetas y la lógica.");
            }
        }
    }

    IEnumerator DelayFade(Transform player)
    {
        yield return new WaitForSeconds(fadeTime);
        player.transform.position = new Vector3(newPlayerPosition.x, newPlayerPosition.y, player.transform.position.z);
        SceneManager.LoadScene(sceneToLoad);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        GameObject player = GameObject.FindWithTag("Player");
        SceneChanger sceneChanger = GameManager.Instance.persistentObjects[14].GetComponent<SceneChanger>();
        if (player == null) return;

        var vcam = Object.FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Follow = player.transform;
            vcam.enabled = false;
            vcam.enabled = true;

            var brain = Object.FindFirstObjectByType<CinemachineBrain>();
            if (brain != null)
            {
                brain.enabled = false;
                brain.enabled = true;
            }

            StartCoroutine(ForceCamNextFrame(vcam, player.transform.position));
            sceneChanger.FadeAnim.Play("FadeFromBlack");
        }

        if (scene.name == "Town" && shouldSaveData)
        {
            DataSaver dataSaver = GameManager.Instance.persistentObjects[8].GetComponent<DataSaver>();
            dataSaver.SaveDataFn();
            shouldSaveData = false;
        }
    }

    public IEnumerator ForceCamNextFrame(CinemachineCamera vcam, Vector3 targetPos)
    {
        yield return null;
        if (vcam != null)
        {
            vcam.ForceCameraPosition(targetPos, Quaternion.identity);
        }
    }

    public void StartNewGame()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
            newPlayerPosition = new Vector2(-14.48f, 58.18f);

        FadeAnim.Play("FadeToBlack");
        GameObject player = GameManager.Instance.persistentObjects[1];

        if (!player.activeSelf)
            player.SetActive(true);

        // Reset stats
        StatsManager stats = GameManager.Instance.persistentObjects[6].GetComponent<StatsManager>();
        stats.maxHealth = 10;
        stats.currentHealth = 10;
        stats.damage = 1;
        stats.speed = 8.6f;
        stats.RefreshHealthUI();

        if (SkillTreeManager.Instance != null)
        {
            SkillTreeManager.Instance.ResetProgress();
        }

        // Reset experience
        ExpManager exp = GameManager.Instance.persistentObjects[3].GetComponent<ExpManager>();
        exp.level = 0;
        exp.currentExp = 0;
        exp.expToLevel = 10;
        exp.UpdateUI();

        // Reset score
        ScoreManager score = GameManager.Instance.persistentObjects[10].GetComponent<ScoreManager>();
        score.enemiesDefeated = 0;
        score.totalLevel = 0;
        score.totalGold = 0;

        // Reset inventory
        InventoryManager inventoryManager = GameManager.Instance.persistentObjects[4].GetComponent<InventoryManager>();
        inventoryManager.gold = 0;
        inventoryManager.ClearInventory();
        inventoryManager.goldText.text = inventoryManager.gold.ToString();

        // ✅ Reset playtime
        PlaytimeTracker playtime = GameManager.Instance.persistentObjects[15].GetComponent<PlaytimeTracker>();
        playtime.playtimeInSeconds = 0f;

        // Set target scene
        sceneToLoad = "Town";
        shouldSaveData = true;
        StartCoroutine(DelayFade(player.transform));
    }

    public void ExitGame()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            Application.Quit();
        }
        else if (!isExiting)
        {
            StartCoroutine(SaveAndExitToMainMenu());
        }
    }

    private IEnumerator SaveAndExitToMainMenu()
    {
        isExiting = true;

        if (SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsOpen)
        {
            SkillTreeManager.Instance.CloseTree();
        }

        DataSaver dataSaver = GameManager.Instance.persistentObjects[8].GetComponent<DataSaver>();
        if (dataSaver == null)
        {
            Debug.LogError("No se encontro DataSaver. La salida se cancelo para no perder el progreso.");
            isExiting = false;
            yield break;
        }

        yield return dataSaver.SaveAndWait(false);
        if (!dataSaver.LastSaveSucceeded)
        {
            Debug.LogError("No se pudo confirmar el guardado en Firebase. La salida se cancelo para no perder el progreso.");
            isExiting = false;
            yield break;
        }

        Menu menu = GameManager.Instance.persistentObjects[9].GetComponent<Menu>();
        RespawnMenu respawnMenu = GameManager.Instance.persistentObjects[12].GetComponent<RespawnMenu>();
        GameObject player = GameManager.Instance.persistentObjects[1];

        FadeAnim.Play("FadeToBlack");

        if (menu != null && menu.isMenuOpen)
        {
            StartCoroutine(menu.CloseMenu());
        }

        if (respawnMenu != null && respawnMenu.isRespawnOpen)
        {
            respawnMenu.SetLoadingState(true);
            StartCoroutine(respawnMenu.CloseRespawnMenu());
        }

        sceneToLoad = "MainMenu";
        StartCoroutine(DelayFade(player.transform));

        if (!player.activeSelf)
        {
            player.SetActive(true);
        }
    }
}
