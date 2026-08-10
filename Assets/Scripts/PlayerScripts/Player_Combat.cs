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

        foreach (Collider2D enemy in enemies)
        {
            if (enemy.isTrigger) continue;
            {
                if (enemies.Length > 0)
                {
                    enemies[0].GetComponent<Enemy_Health>().ChangeHealth(-StatsManager.Instance.damage);
                    enemies[0].GetComponent<Enemy_Knockback>().Knockback(transform, StatsManager.Instance.knockbackForce, StatsManager.Instance.knockbackTime, StatsManager.Instance.stunTime);
                }
            }

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
