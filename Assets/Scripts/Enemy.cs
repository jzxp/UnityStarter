using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int health = 30;
    public Animator animator;

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            animator.Play("Hurt");
        }
    }

    private void Die()
    {
        animator.Play("Die");
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1f);
    }
}