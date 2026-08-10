using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class PlayerLightController : MonoBehaviour
{
    public Light2D playerLight;

    public string[] scenesWithLight;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        if (playerLight == null)
        {
            playerLight = GetComponent<Light2D>();
        }

        if (playerLight != null)
        {
            playerLight.enabled = false;
        }

        CheckSceneAndActivateLight();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CheckSceneAndActivateLight();
    }

    void CheckSceneAndActivateLight()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        foreach (string scene in scenesWithLight)
        {
            if (currentScene == scene)
            {
                if (playerLight != null)
                {
                    playerLight.enabled = true;
                }
                return;
            }
        }

        if (playerLight != null)
        {
            playerLight.enabled = false;
        }
    }
}
