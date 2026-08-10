using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator anim;

    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    static readonly int IsMovingHash = Animator.StringToHash("isMoving");

    private float lastX, lastY;
    private int facingDirection = 1;

    public bool isSwordEquipped = false;
    private bool isSwitchingSword = false;
    public bool isTalking = false;

    private bool isKnockedBack;

    public Player_Combat player_Combat;

    void Update()
    {
        // Toggle sword equip/unequip on Q, incluso mientras se mueve
        if (Input.GetButtonDown("Equip/Unequip") && !isSwitchingSword && player_Combat.isAttacking == false)
        {
            if (!isSwordEquipped)
                StartCoroutine(EquipSword());
            else
                StartCoroutine(UnequipSword());
        }


        if (Input.GetButtonDown("Attack") && isSwordEquipped == true && isSwitchingSword == false)
        {

            transform.localScale = new Vector3(
                Mathf.Abs(transform.localScale.x) * facingDirection,
                transform.localScale.y,
                transform.localScale.z
            );
            player_Combat.Attack();
        }


    }

    void FixedUpdate()
    {
        if (!isKnockedBack && !isTalking)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            // Si me estoy moviendo, actualizo "última dirección"
            if (horizontal != 0f || vertical != 0f)
            {
                lastX = horizontal;
                lastY = vertical;
                facingDirection = horizontal > 0 ? 1 : -1;
            }


            // Flip en X
            if ((horizontal > 0 && transform.localScale.x < 0) && Input.GetAxisRaw("Vertical") == 0 ||
                (horizontal < 0 && transform.localScale.x > 0) && Input.GetAxisRaw("Vertical") == 0)
            {
                Flip();
            }




            // Actualizamos el Blend Tree
            anim.SetFloat(MoveXHash, horizontal);
            anim.SetFloat(MoveYHash, vertical);

            bool isMoving = (horizontal != 0f || vertical != 0f);
            anim.SetBool(IsMovingHash, isMoving);

            anim.SetFloat(MoveXHash, isMoving ? horizontal : lastX);
            anim.SetFloat(MoveYHash, isMoving ? vertical : lastY);

            // Movimiento físico
            rb.linearVelocity = new Vector2(horizontal, vertical) * StatsManager.Instance.speed;
        }
        else if (isTalking) // Si está hablando, detiene la animación de movimiento
        {
            anim.SetBool(IsMovingHash, false); // Detiene la animación
        }
    }

    void Flip()
    {
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
    }


    public IEnumerator EquipSword()
    {
        isSwitchingSword = true;
        anim.SetTrigger("EquipSword");

        // Espera a que termine la animación en la capa 0
        while (true)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Sword_EquipTree") && state.normalizedTime >= 1f)
                break;
            yield return null;
            isSwordEquipped = true;
        }

        // Aquí reseteamos los parámetros de movimiento justito antes de que vuelva el control
        anim.SetFloat("horizontal", 0);
        anim.SetFloat("vertical", 0);
        rb.linearVelocity = Vector2.zero;

        anim.SetLayerWeight(1, 1);

        isSwitchingSword = false;
        anim.SetBool("isEquiped", true);
    }

   public IEnumerator UnequipSword()
    {
        isSwitchingSword = true;
        anim.SetTrigger("UnequipSword");

        // Espera a que termine la animación en la capa 1
        while (true)
        {
            AnimatorStateInfo state = anim.GetCurrentAnimatorStateInfo(1);
            if (state.IsName("Sword_UnequipTree") && state.normalizedTime >= 1f)
                break;
            yield return null;
        }

        // Volver a cero para que no “salte” al reanudar
        anim.SetFloat("horizontal", 0);
        anim.SetFloat("vertical", 0);
        rb.linearVelocity = Vector2.zero;

        anim.SetLayerWeight(1, 0);
        isSwordEquipped = false;
        isSwitchingSword = false;
        anim.SetBool("isEquiped", false);
    }


    public void Knockback(Transform enemy, float force, float stunTime)
    {
        isKnockedBack = true;
        Vector2 direction = (transform.position - enemy.position).normalized;
        rb.linearVelocity = direction * force;
        StartCoroutine(KnockBackCounter(stunTime));
    }

    IEnumerator KnockBackCounter(float stunTime)
    {
        yield return new WaitForSeconds(stunTime);
        rb.linearVelocity = Vector2.zero;
        isKnockedBack = false;
    }

    public void SetTalkingState(bool talking)
    {
        isTalking = talking; // Cambia el estado de "hablando"
    }
}