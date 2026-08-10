using UnityEngine;

public class NPC_Talk : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    public Animator interactAnim;
    public DialogueSO dialogueSO;

    static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    public PlayerMovement playerMovement; // Referencia al controlador del jugador
    private Rigidbody2D playerRigidbody; // Referencia al Rigidbody2D del jugador

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        rb.linearVelocity = Vector2.zero; // Cambiado de linearVelocity a velocity
        rb.bodyType = RigidbodyType2D.Kinematic;
        anim.SetBool(IsMovingHash, false);
        interactAnim.Play("Open");

        playerMovement = FindFirstObjectByType<PlayerMovement>(); // Encuentra el controlador del jugador
        playerRigidbody = playerMovement.GetComponent<Rigidbody2D>(); // Encuentra el Rigidbody2D del jugador
    }

    private void OnDisable()
    {
        interactAnim.Play("Close");

        if (CompareTag("Kinematic"))
            rb.bodyType = RigidbodyType2D.Kinematic;
        else
            rb.bodyType = RigidbodyType2D.Dynamic;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Interact"))
        {
            if (DialogueManager.Instance.isDialogueActive)
            {
                // Si el texto se está escribiendo, completa el texto
                if (DialogueManager.Instance.dialogueTyper.IsTyping())
                {
                    DialogueManager.Instance.dialogueTyper.QuickSkip(); // Completa el texto actual
                }
                else
                {
                    DialogueManager.Instance.AdvanceDialogue();
                    // Aquí no es necesario cambiar el estado de "hablando" porque el diálogo sigue activo
                }
            }
            else
            {
                DialogueManager.Instance.StartDialogue(dialogueSO); // Inicia el diálogo
                SetPlayerTalkingState(true);
                playerRigidbody.linearVelocity = Vector2.zero; // Cambia el estado del jugador a "hablando"
            }
        }
    }

    public void EndDialogue()
    {
        // Desactivar el estado de "hablando"
        SetPlayerTalkingState(false);
        var playermov = GameManager.Instance.persistentObjects[1].GetComponent<PlayerMovement>();
        //force the player to stfu
        if (playermov.isTalking)
            playermov.isTalking = false;

        // Asegúrate de que el jugador pueda moverse nuevamente
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector2.zero;
            playerRigidbody.bodyType = RigidbodyType2D.Dynamic;
        }

        // Log para saber si el estado no se ha desactivado correctamente
        if (playerMovement != null && playerMovement.isTalking)
        {
            Debug.LogWarning("¡isTalking no se desactivó al finalizar el diálogo!");
        }

        Debug.Log("Diálogo terminado, estado de 'isTalking' desactivado");
    }



    public void SetPlayerTalkingState(bool talking)
    {
        if (playerMovement != null)
        {
            playerMovement.SetTalkingState(talking);
            Debug.Log("Estado de 'isTalking' ahora: " + playerMovement.isTalking);
        }
    }

}
