using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIMenuManager : MonoBehaviour
{
    public static UIMenuManager Instance;

    [Header("UI Panels")]
    [SerializeField] private GameObject menuLoginCanvas;
    [SerializeField] private GameObject menuRegisterCanvas;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject menuRegisterVerifCanvas;

    [Header("Verification Text")]
    [SerializeField] private TMP_Text emailVerificationText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ClearUI();
        // Iniciar verificación de usuario al comenzar la escena
        StartCoroutine(CheckUserAndOpenPanel());
    }

    private IEnumerator CheckUserAndOpenPanel()
    {
        yield return null; // Esperar 1 frame por seguridad

        if (SceneManager.GetActiveScene().name != "MainMenu")
        {
            if (FirebaseAuthManager.Instance != null && FirebaseAuthManager.Instance.IsUserAuthenticated())
            {
                Debug.Log("Usuario autenticado. Abriendo GamePanel.");
                OpenGamePanel();
            }
            else
            {
                Debug.Log("Usuario no autenticado. Abriendo LoginPanel.");
                OpenLoginPanel();
            }
        }

    }

    private void ClearUI()
    {
        if (menuLoginCanvas != null) menuLoginCanvas.SetActive(false);
        if (menuRegisterCanvas != null) menuRegisterCanvas.SetActive(false);
        if (menuRegisterVerifCanvas != null) menuRegisterVerifCanvas.SetActive(false);
        if (gamePanel != null) gamePanel.SetActive(false);
    }

    public void OpenLoginPanel()
    {
        Debug.Log("Abriendo panel de inicio de sesión");
        ClearUI();
        if (menuLoginCanvas != null) menuLoginCanvas.SetActive(true);
    }

    public void OpenRegistrationPanel()
    {
        Debug.Log("Abriendo panel de registro");
        ClearUI();
        if (menuRegisterCanvas != null) menuRegisterCanvas.SetActive(true);
    }

    public void OpenGamePanel()
    {
        Debug.Log("Abriendo panel de juego");
        ClearUI();
        if (gamePanel != null) gamePanel.SetActive(true);
    }

    public void ShowVerificationResponse(bool isEmailSent, string emailId, string errorMessage)
    {
        Debug.Log("Mostrando respuesta de verificación de correo");
        ClearUI();
        if (menuRegisterVerifCanvas != null) menuRegisterVerifCanvas.SetActive(true);

        if (emailVerificationText != null)
        {
            emailVerificationText.text = isEmailSent
                ? $"Por favor verifica tu correo\nSe envió un email a: {emailId}"
                : $"No se pudo enviar el correo: {errorMessage}";
        }
    }

    public void LoadAuthScene()
    {
        SceneManager.LoadScene("Auth");
    }
}
