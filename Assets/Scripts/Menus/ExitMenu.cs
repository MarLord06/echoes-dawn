using UnityEngine;

public class ExitMenu : MonoBehaviour
{
    public SceneChanger sceneChanger;

    void Awake()
    {
        // Obtén la referencia al SceneChanger
        sceneChanger = GameManager.Instance.persistentObjects[14].GetComponent<SceneChanger>();
        if (sceneChanger == null)
        {
            Debug.LogError("No se encontró un SceneChanger en la escena.");
        }
        Debug.Log(sceneChanger.sceneToLoad);
    }

    public void ExitGame()
    {
        if (sceneChanger != null)
        {
            sceneChanger.ExitGame(); // Llama al método ExitGame del SceneChanger
        }
    }
}
