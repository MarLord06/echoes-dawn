using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI References")]
    public CanvasGroup canvasGroup;
    public Image portrait;
    public TMP_Text actorName;
    public TextTyperTMP dialogueTyper; // Cambia a TextTyperTMP
    public Button[] choiceButtons;

    public bool isDialogueActive;

    public DialogueSO currentDialogue;
    private int dialogueIndex;

    private float lastDialogueEndTime;
    private float dialogueCooldown = .1f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        foreach (var button in choiceButtons)
            button.gameObject.SetActive(false);
    }

    public void StartDialogue(DialogueSO dialogueSO)
    {
        if (Time.unscaledTime - lastDialogueEndTime < dialogueCooldown)
            return;

        currentDialogue = dialogueSO;
        dialogueIndex = 0;
        isDialogueActive = true;
        ShowDialogue();
    }

    public void AdvanceDialogue()
    {
        // Si el texto se está escribiendo, completa el texto
        if (dialogueTyper.IsTyping())
        {
            dialogueTyper.QuickSkip(); // Completa el texto actual
        }
        else
        {
            if (dialogueIndex < currentDialogue.lines.Length)
                ShowDialogue();
            else
            {
                ShowChoices();
            }
        }
    }

    private void ShowDialogue()
    {
        DialogueLine line = currentDialogue.lines[dialogueIndex];

        portrait.sprite = line.speaker.portrait;
        actorName.text = line.speaker.actorName;

        // Usar TextTyperTMP para mostrar el texto
        dialogueTyper.UpdateText(line.text); // Actualiza el texto en el TextTyperTMP

        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        dialogueIndex++;
    }

    private void ShowChoices()
    {
        ClearChoices();

        if (currentDialogue.options.Length > 0)
        {
            for (int i = 0; i < currentDialogue.options.Length; i++)
            {
                var option = currentDialogue.options[i];

                if (choiceButtons[i] != null) // Verifica si el botón no es nulo
                {
                    choiceButtons[i].GetComponentInChildren<TMP_Text>().text = option.optionText;
                    choiceButtons[i].gameObject.SetActive(true);
                    choiceButtons[i].onClick.RemoveAllListeners(); // Asegúrate de eliminar listeners anteriores
                    choiceButtons[i].onClick.AddListener(() => ChooseOption(option.nextDialogue));
                }
            }
        }
        else
        {
            if (choiceButtons[0] != null) // Verifica si el botón no es nulo
            {
                choiceButtons[0].GetComponentInChildren<TMP_Text>().text = "Terminar";
                choiceButtons[0].onClick.RemoveAllListeners(); // Asegúrate de eliminar listeners anteriores
                choiceButtons[0].onClick.AddListener(EndDialogue);
                choiceButtons[0].gameObject.SetActive(true);
            }
        }

        // Asegúrate de que el CanvasGroup esté interactuable
        canvasGroup.alpha = 1;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }



    private void ChooseOption(DialogueSO dialogueSO)
    {
        Debug.Log("Opción elegida: " + (dialogueSO != null ? dialogueSO.name : "Ninguna opción"));
        if (dialogueSO == null)
            EndDialogue();
        else
        {
            ClearChoices();
            StartDialogue(dialogueSO);
        }
    }

    public void EndDialogue()
    {
        dialogueIndex = 0;
        isDialogueActive = false;
        ClearChoices();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        lastDialogueEndTime = Time.unscaledTime;

        // Aquí debes llamar a EndDialogue en el NPC_Talk
        var npcTalk = FindFirstObjectByType<NPC_Talk>();
        if (npcTalk != null)
        {
            GameManager.Instance.UpdateData(); // Actualiza los datos del GameManager
            Debug.Log("Finalizando diálogo con NPC: " + npcTalk.name);
            npcTalk.EndDialogue(); // Asegúrate de que el estado de "hablando" se desactive
        }
    }


    private void ClearChoices()
    {
        foreach (var button in choiceButtons)
        {
            if (button != null) // Verifica si el botón no es nulo
            {
                button.gameObject.SetActive(false);
                button.onClick.RemoveAllListeners();
            }
        }
    }

}
