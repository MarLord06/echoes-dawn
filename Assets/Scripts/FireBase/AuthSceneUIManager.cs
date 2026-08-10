using UnityEngine;
using UnityEngine.UI;

public class AuthSceneUIManager : MonoBehaviour
{
    public Button authSceneButton; // Arrastra el botón en el inspector

    private void Start()
    {
        if (authSceneButton != null)
        {
            authSceneButton.onClick.RemoveAllListeners();
            authSceneButton.onClick.AddListener(() =>
            {
                UIMenuManager.Instance.LoadAuthScene();
                ;
            });
        }
    }
}
