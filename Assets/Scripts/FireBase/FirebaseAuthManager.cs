using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;

public class FirebaseAuthManager : MonoBehaviour
{
    public static FirebaseAuthManager Instance { get; private set; }

    // Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public bool IsFirebaseReady { get; private set; } = false;


    public event Action OnUserAuthenticated;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(CheckAndFixDependenciesAsync());
    }

    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        dependencyStatus = dependencyTask.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            InitializeFirebase();
            yield return new WaitForEndOfFrame();
            StartCoroutine(CheckForAutoLogin());
        }
        else
        {
            Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
        }
    }

    private void InitializeFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    private IEnumerator CheckForAutoLogin()
    {
        user = auth.CurrentUser;

        if (user != null)
        {
            var reloadUserTask = user.ReloadAsync();

            yield return new WaitUntil(() => reloadUserTask.IsCompleted);

            AutoLogin();
        }
        else
        {
            // No hay usuario => abrir login
            Debug.Log("No hay usuario. Mostrando LoginPanel.");
            UIMenuManager.Instance.OpenLoginPanel();
        }
    }

    private void AutoLogin()
    {
        if (user != null)
        {
            if (user.IsEmailVerified)
            {
                References.userName = user.DisplayName;
                Debug.Log(References.userName);
                OnUserAuthenticated?.Invoke();
                UIMenuManager.Instance.OpenGamePanel();

            }
            else
            {
                SendEmailForVerification();
            }
        }
        else
        {
            UIMenuManager.Instance.OpenLoginPanel();
        }
    }

    private void AuthStateChanged(object sender, EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                DataSaver dataSaver = GameManager.Instance.persistentObjects[8].GetComponent<DataSaver>();
                if (dataSaver != null)
                {
                    dataSaver.userId = user.UserId;
                }
            }
        }
    }

    // Cambié para recibir los datos como parámetros
    public void Login(string email, string password)
    {
        StartCoroutine(LoginAsync(email, password));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);
            // Aquí puedes agregar manejo de errores para la UI
        }
        else
        {
            user = loginTask.Result.User;

            if (user.IsEmailVerified)
            {
                References.userName = user.DisplayName;
                Debug.Log(References.userName);
                OnUserAuthenticated?.Invoke();
                UIMenuManager.Instance.OpenGamePanel();

                DataSaver dataSaver = FindFirstObjectByType<DataSaver>();
                if (dataSaver != null)
                {
                    dataSaver.userId = user.UserId;
                }
            }
            else
            {
                SendEmailForVerification();
            }
        }
    }

    // Cambié para recibir los datos como parámetros
    public void Register(string name, string email, string password, string confirmPassword)
    {
        StartCoroutine(RegisterAsync(name, email, password, confirmPassword));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("User Name is empty");
            yield break;
        }
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("Email field is empty");
            yield break;
        }
        if (password != confirmPassword)
        {
            Debug.LogError("Password does not match");
            yield break;
        }

        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            Debug.LogError(registerTask.Exception);
            // Aquí puedes agregar manejo de errores para la UI
        }
        else
        {
            user = registerTask.Result.User;
            UserProfile userProfile = new UserProfile { DisplayName = name };

            var updateProfileTask = user.UpdateUserProfileAsync(userProfile);
            yield return new WaitUntil(() => updateProfileTask.IsCompleted);

            if (updateProfileTask.Exception != null)
            {
                user.DeleteAsync();
                Debug.LogError(updateProfileTask.Exception);
            }
            else
            {
                Debug.Log("Registration Successful. Welcome " + user.DisplayName);
                if (user.IsEmailVerified)
                {
                    UIMenuManager.Instance.OpenGamePanel();
                }
                else
                {
                    SendEmailForVerification();
                }
            }
        }
    }

    public void SendEmailForVerification()
    {
        StartCoroutine(SendEmailForVerificationAsync());
    }

    private IEnumerator SendEmailForVerificationAsync()
    {
        if (user != null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(() => sendEmailTask.IsCompleted);

            if (sendEmailTask.Exception != null)
            {
                FirebaseException firebaseException = sendEmailTask.Exception.GetBaseException() as FirebaseException;
                AuthError error = (AuthError)firebaseException.ErrorCode;

                string errorMessage = "Unknow Error : Please try again later";

                switch (error)
                {
                    case AuthError.Cancelled:
                        errorMessage = "Email Verification Was Cancelled";
                        break;
                    case AuthError.TooManyRequests:
                        errorMessage = "Too Many Request";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        errorMessage = "The Email You Entered is Invalid";
                        break;
                }

                UIMenuManager.Instance.ShowVerificationResponse(false, user.Email, errorMessage);
            }
            else
            {
                Debug.Log("Email has successfully sent");
                UIMenuManager.Instance.ShowVerificationResponse(true, user.Email, null);
            }
        }
    }

    public void Logout()
    {
        auth.SignOut();
        References.userName = null;
        UIMenuManager.Instance.OpenLoginPanel();
    }

    public void OpenMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public bool IsUserAuthenticated()
    {
        return user != null;
    }

    public bool IsInitialized()
    {
        IsFirebaseReady = true;
        return dependencyStatus == DependencyStatus.Available;
    }
}
