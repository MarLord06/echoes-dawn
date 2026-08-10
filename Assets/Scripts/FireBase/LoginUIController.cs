using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LoginUIController : MonoBehaviour
{
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;

    void Start()
    {
        loginButton.onClick.AddListener(() =>
        {
            string email = emailInput.text;
            string password = passwordInput.text;

            FirebaseAuthManager.Instance.Login(email, password);
        });
    }
}
