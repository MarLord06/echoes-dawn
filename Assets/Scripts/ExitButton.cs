using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitButton : MonoBehaviour
{
    public void ExitGame()
    {
        if (SceneManager.GetActiveScene().name == "Auth")
        {
            Application.Quit();
        }
    }
}
