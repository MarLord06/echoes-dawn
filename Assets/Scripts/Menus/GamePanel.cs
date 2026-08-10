using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Asegúrate de incluir esto para usar botones

public class GamePanelManager : MonoBehaviour
{
    [SerializeField]
    private Button playButton; // Botón para jugar

    private FirebaseAuthManager firebaseAuthManager; // Referencia al FirebaseAuthManager

    private void Awake()
    {
        // Obtener referencia al FirebaseAuthManager
        firebaseAuthManager = FindFirstObjectByType<FirebaseAuthManager>();

        // Verificar que la referencia se haya encontrado
        if (firebaseAuthManager == null)
        {
            Debug.LogError("No se encontró FirebaseAuthManager.");
            return;
        }

        // Asignar la función al botón
        if (playButton != null)
        {
            //playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogError("El botón de jugar no está asignado.");
        }
    }

    /*private void OnPlayButtonClicked()
    {
        // Llama al método OpenAuthScene del FirebaseAuthManager
        Debug.Log("Botón de jugar presionado.");

        if (SceneManager.GetActiveScene().name == "MainMenu")
        firebaseAuthManager.OpenAuthScene(); // Llama al método para abrir la escena de autenticación
    }*/

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
        // Aquí puedes mostrar la información del usuario en el panel
        Debug.Log("Welcome message username: " + References.userName);
        // Actualiza el UI con el nombre de usuario
    }
}
