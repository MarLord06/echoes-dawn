using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using JetBrains.Annotations;
using System.Collections;
using System;

public class UserTextManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text welcomeText;

    [SerializeField]
    private TMP_Text user;


    private void Awake()
    {
        // Suscribirse al evento de autenticación
        FirebaseAuthManager firebaseAuthManager = FindFirstObjectByType<FirebaseAuthManager>();
        if (firebaseAuthManager != null)
        {
            firebaseAuthManager.OnUserAuthenticated += ShowUserInfo;
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento para evitar fugas de memoria
        FirebaseAuthManager firebaseAuthManager = FindFirstObjectByType<FirebaseAuthManager>();
        if (firebaseAuthManager != null)
        {
            firebaseAuthManager.OnUserAuthenticated -= ShowUserInfo;
        }
    }

    private void Start()
    {
        // Si el usuario ya está autenticado, muestra la información
        if (!string.IsNullOrEmpty(References.userName))
        {
            ShowUserInfo();
        }
        else
        {
            Debug.Log("User  name is empty, waiting for authentication.");
        }
    }

    private void ShowUserInfo()
    {
        Debug.Log("Welcome message username: " + References.userName);
        welcomeText.text = $"Hola de vuelta {References.userName}!";
        user.text = $"Usuario: {References.userName}";
    }

    public void GoToAuth()
    {
        StartCoroutine(GoToAuthEnum());
    }

    private IEnumerator GoToAuthEnum()
    {
        SceneManager.LoadScene("Auth");
        yield return new WaitForEndOfFrame();
        UIMenuManager.Instance.OpenGamePanel();
    }
    
}
