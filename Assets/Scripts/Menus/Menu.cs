using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{
    [Header("References")]
    public Animator anim;
    public CanvasGroup menuCanvasGroup;
    private bool isTransitioning = false;
    public bool isMenuOpen = false;

    [Header("Animation Settings")]
    [SerializeField] private float closeAnimationTime = 0.2f; 

    void Update()
    {
        if (SkillTreeManager.Instance != null && SkillTreeManager.Instance.IsOpen)
        {
            if (Input.GetButtonDown("Cancel"))
            {
                SkillTreeManager.Instance.CloseTree();
            }

            return;
        }

        // Abrir/cerrar con M (Menu) o ESC (Cancel)
        if ((Input.GetButtonDown("Menu") || Input.GetButtonDown("Cancel")) && !isTransitioning)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (isMenuOpen)
        {
            StartCoroutine(CloseMenu());
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        isMenuOpen = true;
        isTransitioning = true;

        anim.Play("OpenMenu");
        menuCanvasGroup.alpha = 1;
        menuCanvasGroup.interactable = true;
        menuCanvasGroup.blocksRaycasts = true;

        Time.timeScale = 0; 
        isTransitioning = false; 
    }

    public IEnumerator CloseMenu()
    {
        isTransitioning = true;
        anim.Play("CloseMenu");
        Time.timeScale = 1;
        yield return new WaitForSeconds(closeAnimationTime);

        menuCanvasGroup.alpha = 0;
        menuCanvasGroup.interactable = false;
        menuCanvasGroup.blocksRaycasts = false;

        isMenuOpen = false;
        isTransitioning = false;
    }
    
}
