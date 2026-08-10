using UnityEngine;

public class Player_Combat : MonoBehaviour
{
    public Transform attackPoint;
    public LayerMask enemyLayer;

    public Animator anim;

    public float cooldown = 2;
    private float timer;

    public bool isAttacking = false;

    public AudioManager audioManager;

    private void Update()
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }


    public void Attack()
    {
        if (timer <= 0)
        {
            anim.SetBool("isAttacking", true);
            audioManager.PlaySFX(audioManager.slash);
            isAttacking = true;

            timer = cooldown;
        }

    }

    public void DealDamage()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, StatsManager.Instance.weaponRange, enemyLayer);

        foreach (Collider2D enemy in CombatTargetResolver.GetDistinctDamageableColliders(enemies))
        {
            if (!enemy.TryGetComponent(out Enemy_Health enemyHealth) ||
                !enemy.TryGetComponent(out Enemy_Knockback enemyKnockback))
            {
                continue;
            }

            enemyHealth.ChangeHealth(-StatsManager.Instance.damage);
            enemyKnockback.Knockback(transform, StatsManager.Instance.knockbackForce, StatsManager.Instance.knockbackTime, StatsManager.Instance.stunTime);
        }
    }

    public void FinishAttacking()
    {
        anim.SetBool("isAttacking", false);
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, StatsManager.Instance.weaponRange);
    }

}
