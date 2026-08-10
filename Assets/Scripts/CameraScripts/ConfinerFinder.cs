
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ConfinerFinder : MonoBehaviour
{

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CinemachineConfiner2D confiner = GetComponent<CinemachineConfiner2D>();
        confiner.BoundingShape2D = GameObject.FindWithTag("Confiner").GetComponent<PolygonCollider2D>();
    }

    
}
