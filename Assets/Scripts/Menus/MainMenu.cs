using UnityEngine;
using UnityEngine.UI;
using System.Collections; // Asegúrate de incluir esto para usar coroutines
using Firebase.Database;

public class MainMenu : MonoBehaviour
{
    public Button[] menuButtons; // Array para almacenar los botones del menú
    public SceneChanger persistentSceneChanger; // Referencia al SceneChanger persistente
    public DataSaver dataSaver; // Referencia al DataSaver
    public ExitMenu exitMenu; // Referencia al ExitMenu

    public FirebaseAuthManager firebaseAuthManager; // Agregar referencia
    public DatabaseReference dbRef; // Agregar referencia

    // Referencias a los FadeAnim de cada SceneChanger
    public Animator persistentFadeAnim; // Animator para el SceneChanger persistente

    void Awake()
    {
        StartCoroutine(InitializeMenu());
    }

    private IEnumerator InitializeMenu()
    {
        // Obtener referencia al FirebaseAuthManager
        firebaseAuthManager = FindFirstObjectByType<FirebaseAuthManager>();

        // Esperar a que FirebaseAuthManager esté inicializado
        while (firebaseAuthManager == null || !firebaseAuthManager.IsInitialized())
        {
            yield return null; // Espera hasta que FirebaseAuthManager esté inicializado
        }

        // Inicializa dbRef
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        // Obtener referencias
        persistentSceneChanger = GameManager.Instance.persistentObjects[14].GetComponent<SceneChanger>();
        persistentFadeAnim = persistentSceneChanger.FadeAnim;

        dataSaver = GameManager.Instance.persistentObjects[8].GetComponent<DataSaver>();
        exitMenu = GameManager.Instance.persistentObjects[13].GetComponent<ExitMenu>();

        // Asignar funciones a los botones
        AssignButtonFunctions();

        // Verificar referencias
        Debug.Log("Persistent SceneChanger: " + (persistentSceneChanger != null ? "Encontrado" : "No encontrado"));
        Debug.Log("DataSaver: " + (dataSaver != null ? "Encontrado" : "No encontrado"));
        Debug.Log("ExitMenu: " + (exitMenu != null ? "Encontrado" : "No encontrado"));
        Debug.Log("FirebaseAuthManager: " + (firebaseAuthManager != null ? "Encontrado" : "No encontrado"));
        Debug.Log("dbRef: " + (dbRef != null ? "Encontrado" : "No encontrado"));
    }

    private void AssignButtonFunctions()
    {
        // Verifica que el array de botones tenga la cantidad correcta
        if (menuButtons.Length < 3)
        {
            Debug.LogError("No hay suficientes botones asignados en el array.");
            return;
        }

        // Botón 0: Nueva Partida
        menuButtons[0].onClick.AddListener(() => StartNewGame());

        // Botón 1: Cargar Partida
        menuButtons[1].onClick.AddListener(() => LoadGame());

        // Botón 2: Salir
        menuButtons[2].onClick.AddListener(() => ExitGame());
    }

    private void StartNewGame()
    {
        if (persistentSceneChanger != null)
        {
            persistentSceneChanger.StartNewGame(); // Llama al método StartNewGame del SceneChanger persistente
        }
        else
        {
            Debug.LogError("No se encontró un SceneChanger persistente.");
        }
    }

    private void LoadGame()
    {
        if (dataSaver != null)
        {
            StartCoroutine(LoadGameWithFade(persistentSceneChanger, dataSaver)); // Llama a la coroutine para manejar el fade y la carga
        }
        else
        {
            Debug.LogError("No se encontró un DataSaver.");
        }
    }

    private IEnumerator LoadGameWithFade(SceneChanger sceneChanger, DataSaver dataSaver)
    {
        // Verificar si hay datos guardados
        if (dataSaver != null && firebaseAuthManager != null)
        {
            if (!firebaseAuthManager.IsUserAuthenticated())
            {
                Debug.LogError("No hay usuario autenticado. No se puede cargar la partida.");
                yield break; // Salir si no hay usuario autenticado
            }

            string userId = firebaseAuthManager.user.UserId; // Obtener el UID
            var serverData = dbRef.Child("users").Child(userId).GetValueAsync();
            yield return new WaitUntil(() => serverData.IsCompleted);

            if (!serverData.Result.Exists)
            {
                Debug.LogWarning("No hay datos guardados para este usuario. No se puede cargar la partida.");
                // Aquí puedes mostrar un mensaje al usuario o realizar otra acción
                yield break; // Salir de la coroutine si no hay datos
            }
        }
        else
        {
            Debug.LogError("DataSaver no encontrado.");
            yield break; // Salir si no hay DataSaver
        }

        // Inicia el fade
        persistentFadeAnim.Play("FadeToBlack");
        yield return new WaitForSeconds(sceneChanger.fadeTime); // Espera el tiempo del fade

        // Llama al método de carga de datos
        dataSaver.LoadDataFn(); // Asegúrate de que este método maneje la carga de la escena
    }

    private void ExitGame()
    {
        if (exitMenu != null)
        {
            exitMenu.ExitGame(); // Llama al método ExitGame del ExitMenu
        }
        else
        {
            Debug.LogError("No se encontró un ExitMenu.");
        }
    }
}
