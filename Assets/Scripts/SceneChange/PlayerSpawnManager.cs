using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerSpawnManager : MonoBehaviour
{
    private void OnEnable()
    {
        // Nos suscribimos al evento de escena cargada
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Nos desuscribimos para evitar memory leaks
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string spawnId = PlayerPrefs.GetString("LastSpawnId", "");
        if (string.IsNullOrEmpty(spawnId))
            return;

        // Buscamos todos los SpawnPoint en la escena
        var spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
        foreach (var sp in spawnPoints)
        {
            if (sp.spawnId == spawnId)
            {
                // Reposicionamos al jugador
                transform.position = sp.transform.position;
                break;
            }
        }
    }
}
