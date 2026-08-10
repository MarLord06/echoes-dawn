using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Para TMP_InputField

public class LoginPanelUI : MonoBehaviour
{
    public Button logoutButton;
    public Button loginButton;
    public Button playButton;
    public Button registerButton;

    // Añade referencias a los campos de texto del UI
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;

    public TMP_InputField nameRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField confirmPasswordRegisterField;

    void Start()
    {
        if (logoutButton != null)
        {
            logoutButton.onClick.RemoveAllListeners();
            logoutButton.onClick.AddListener(() =>
            {
                FirebaseAuthManager.Instance.Logout();
            });
        }

        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(() =>
            {
                // Aquí tomas los valores de los campos y los pasas
                string email = emailLoginField.text;
                string password = passwordLoginField.text;
                FirebaseAuthManager.Instance.Login(email, password);
            });
        }

        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
            playButton.onClick.AddListener(() =>
            {
                FirebaseAuthManager.Instance.OpenMainMenu();
            });
        }

        if (registerButton != null)
        {
            registerButton.onClick.RemoveAllListeners();
            registerButton.onClick.AddListener(() =>
            {
                string name = nameRegisterField.text;
                string email = emailRegisterField.text;
                string password = passwordRegisterField.text;
                string confirmPassword = confirmPasswordRegisterField.text;

                FirebaseAuthManager.Instance.Register(name, email, password, confirmPassword);
            });
        }
    }
}
