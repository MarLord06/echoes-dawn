using System.Collections;
using UnityEngine;

public class NPC_Patrol : MonoBehaviour
{
    public Vector2[] patrolPoints;
    public float speed = 3f;
    public float pauseDuration = 1.5f;

    private int currentPatrolIndex;
    private Vector2 target;
    private bool isPaused;

    private Rigidbody2D rb;
    private Animator anim;

    // Para el Blend Tree
    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private float lastX, lastY;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        // Primer punto
        currentPatrolIndex = 0;
        target = patrolPoints[0];
        StartCoroutine(SetPatrolPoint());
    }

    void Update()
    {
        if (isPaused)
        {
            // Cuando pausa, detener movimiento y forzar Idle
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(IsMovingHash, false);
            // mantener última dirección en el Blend Tree
            anim.SetFloat(MoveXHash, lastX);
            anim.SetFloat(MoveYHash, lastY);
            return;
        }

        // 1) Calcula la dirección hacia el objetivo
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        // 2) Flip horizontal solo si no hay movimiento vertical
        if (direction.x != 0 && Mathf.Abs(direction.y) < 0.1f)
        {
            if ((direction.x < 0 && transform.localScale.x > 0) ||
                 (direction.x > 0 && transform.localScale.x < 0))
            {
                transform.localScale = new Vector3(
                    -transform.localScale.x,
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }

        // 3) Movimiento físico
        rb.linearVelocity = direction * speed;

        // 4) Parameters para Blend Tree
        bool isMoving = direction.sqrMagnitude > 0.001f;
        anim.SetBool(IsMovingHash, isMoving);

        if (isMoving)
        {
            // actualizar última dirección
            lastX = direction.x;
            lastY = direction.y;
            anim.SetFloat(MoveXHash, direction.x);
            anim.SetFloat(MoveYHash, direction.y);
        }
        else
        {
            // en raras ocasiones de pausa que no usamos 
            anim.SetFloat(MoveXHash, lastX);
            anim.SetFloat(MoveYHash, lastY);
        }

        // 5) Al llegar al punto, pausa y luego cambia
        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            StartCoroutine(SetPatrolPoint());
        }
    }

    IEnumerator SetPatrolPoint()
    {
        isPaused = true;

        // Damos tiempo para que el Update ponga la Idle
        yield return new WaitForSeconds(pauseDuration);

        // Avanza al siguiente punto
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        target = patrolPoints[currentPatrolIndex];
        isPaused = false;
    }
}
