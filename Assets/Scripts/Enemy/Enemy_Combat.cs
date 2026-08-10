using UnityEngine;

public class Enemy_Combat : MonoBehaviour
{
    public int damage = 1;
    public Transform attackPoint;
    public float weaponRange;
    public float knockbackForce;
    public float stunTime;
    public LayerMask playerLayer;

    public void Attack()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, weaponRange, playerLayer);

        if (hits.Length > 0)
        {
            var playerHealth = hits[0].GetComponent<PlayerHealth>();
            var playerMovement = hits[0].GetComponent<PlayerMovement>();

            if (playerHealth != null)
                playerHealth.ChangeHealth(-damage);

            if (playerMovement != null && playerMovement.gameObject.activeInHierarchy)
                playerMovement.Knockback(transform, knockbackForce, stunTime);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(attackPoint.position, weaponRange);
    }
}
