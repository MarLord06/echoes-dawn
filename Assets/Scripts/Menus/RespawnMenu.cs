using UnityEngine;
using System.Collections;

public class RespawnMenu : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public Animator fadeAnim;
    public float fadeTime = 0.2f;
    public CanvasGroup respawnCanvasGroup;
    public bool isTransitioning = false;
    public bool isRespawnOpen = false;
    public bool isLoading = false; // Nuevo estado para indicar que se está cargando

    [Header("Animation Settings")]
    [SerializeField] private float closeAnimationTime = 0.1f;

    private PlayerHealth playerHealth;

    void Awake()
    {
        playerHealth = GameManager.Instance.persistentObjects[1].GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth != null && !playerHealth.gameObject.activeSelf && !isRespawnOpen && !isLoading)
        {
            OpenRespawnMenu();
        }
    }

    public void OpenRespawnMenu()
    {
        isRespawnOpen = true;
        isTransitioning = true;

        StartCoroutine(ShowRespawnMenu());
    }

    private IEnumerator ShowRespawnMenu()
    {
        fadeAnim.Play("FadeToGray");
        yield return new WaitForSeconds(fadeTime);

        // Mostrar el menú de respawn
        respawnCanvasGroup.alpha = 1;
        respawnCanvasGroup.interactable = true;
        respawnCanvasGroup.blocksRaycasts = true;
        anim.Play("OpenRespawn");

        isTransitioning = false;
    }

    public IEnumerator CloseRespawnMenu()
    {
        isTransitioning = true;
        anim.Play("CloseRespawn");
        fadeAnim.Play("FadeFromGray");
        yield return new WaitForSeconds(closeAnimationTime);

        respawnCanvasGroup.alpha = 0;
        respawnCanvasGroup.interactable = false;
        respawnCanvasGroup.blocksRaycasts = false;

        isRespawnOpen = false;
        isTransitioning = false;
    }

    public void SetLoadingState(bool loading)
    {
        isLoading = loading; // Método para establecer el estado de carga
    }
}
