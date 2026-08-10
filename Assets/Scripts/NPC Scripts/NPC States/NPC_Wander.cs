using System.Collections;
using UnityEngine;

public class NPC_Wander : MonoBehaviour
{
    [Header("Wander Area")]
    public float wanderWidht = 5;
    public float wanderHeight = 5;
    public Vector2 startingPosition;

    public float pauseDuration = 1;
    public float speed;
    public Vector2 target;

    private Rigidbody2D rb;
    private Animator anim;
    private bool isPaused;

    // Para el Blend Tree
    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    static readonly int IsMovingHash = Animator.StringToHash("isMoving");
    private float lastX, lastY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        StartCoroutine(PauseAndPickNewDestination());
    }

    private void Update()
    {
        if (isPaused)
        {
            // Detenemos física y forzamos Idle en última dirección
            rb.linearVelocity = Vector2.zero;
            anim.SetBool(IsMovingHash, false);
            anim.SetFloat(MoveXHash, lastX);
            anim.SetFloat(MoveYHash, lastY);
            return;
        }

        if (Vector2.Distance(transform.position, target) < .1f)
            StartCoroutine(PauseAndPickNewDestination());

            if (rb.linearVelocity.sqrMagnitude < 0.9f)
                target = GetRandomTarget();

        Move();
    }

    private void Move()
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;

        // Flip horizontal si el movimiento X predomina
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            if (direction.x > 0 && transform.localScale.x < 0 ||
                direction.x < 0 && transform.localScale.x > 0)
            {
                transform.localScale = new Vector3(
                    -transform.localScale.x,
                    transform.localScale.y,
                    transform.localScale.z
                );
            }
        }

        // Movimiento físico
        rb.linearVelocity = direction * speed;

        // Alimentar Blend Tree
        bool isMoving = direction.sqrMagnitude > 0.001f;
        anim.SetBool(IsMovingHash, isMoving);

        if (isMoving)
        {
            lastX = direction.x;
            lastY = direction.y;
            anim.SetFloat(MoveXHash, direction.x);
            anim.SetFloat(MoveYHash, direction.y);
        }
        else
        {
            anim.SetFloat(MoveXHash, lastX);
            anim.SetFloat(MoveYHash, lastY);
        }
    }

    IEnumerator PauseAndPickNewDestination()
    {
        isPaused = true;

        // Durante la pausa Update() forzará el Idle
        yield return new WaitForSeconds(pauseDuration);

        target = GetRandomTarget();
        isPaused = false;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }
        if (!enabled) return;
        StartCoroutine(PauseAndPickNewDestination());
    }

    private Vector2 GetRandomTarget()
    {
        float halfWidth = wanderWidht / 2;
        float halfHeight = wanderHeight / 2;
        int edge = Random.Range(0, 4);

        return edge switch
        {
            0 => new Vector2(startingPosition.x - halfWidth,Random.Range(startingPosition.y - halfHeight, startingPosition.y + halfHeight)),
            1 => new Vector2(startingPosition.x + halfWidth,Random.Range(startingPosition.y - halfHeight, startingPosition.y + halfHeight)),
            2 => new Vector2(Random.Range(startingPosition.x - halfWidth, startingPosition.x + halfWidth),startingPosition.y - halfHeight),
            _ => new Vector2(Random.Range(startingPosition.x - halfWidth, startingPosition.x + halfWidth),startingPosition.y + halfHeight),
        };
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(startingPosition, new Vector3(wanderWidht, wanderHeight, 0));
    }
}
