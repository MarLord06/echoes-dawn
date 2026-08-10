using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using System.Collections;

public class PlayerCameraAssigner : MonoBehaviour
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
        StartCoroutine(WaitAndAssignCameraTarget());
    }

    private IEnumerator WaitAndAssignCameraTarget()
    {
        yield return new WaitForSeconds(0.1f); // o yield return null;

        GameObject player = GameObject.FindWithTag("Player");

        if (player == null)
        {
            Debug.LogWarning("⚠ Player no encontrado al intentar asignar cámara.");
            yield break;
        }

        var cineCam = GetComponent<CinemachineCamera>();
        if (cineCam == null)
        {
            Debug.LogError("❌ CinemachineCamera no encontrado en el objeto.");
            yield break;
        }

        cineCam.Follow = player.transform;

        Debug.Log("🎯 Cámara asignada correctamente al player.");
    }
}
