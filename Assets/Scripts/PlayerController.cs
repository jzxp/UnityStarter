using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ���������ò���
    public float moveSpeed = 5f;
    public int maxHealth = 100;

    // �������
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // ״̬����
    private int currentHealth;
    private bool isDead = false;
    private bool isFacingRight = true;

    void Start()
    {
        // ��ȡ�������
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ��ʼ��״̬
        currentHealth = maxHealth;
        isFacingRight = true;

        // �Ż� Animator ����
        anim.updateMode = AnimatorUpdateMode.UnscaledTime; // ���� Time.timeScale Ӱ��
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate; // ʼ����Ⱦ����
    }

    void Update()
    {
        if (isDead) return; // ������ֹͣ���в���

        HandleMovement();
        HandleAttack();
    }

    void HandleMovement()
    {
        // ��ȡˮƽ���� (A/D �� ���Ҽ�ͷ)
        float moveX = Input.GetAxisRaw("Horizontal");

        // �ƶ���ɫ
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // ���¶�������
        anim.SetFloat("Speed", Mathf.Abs(moveX));

        // ��ɫ����ת
        if (moveX > 0 && !isFacingRight)
        {
            FlipCharacter();
        }
        else if (moveX < 0 && isFacingRight)
        {
            FlipCharacter();
        }
    }

    void FlipCharacter()
    {
        // �л����򲢷�ת����
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            // ȷ����������������
            if (!IsPlayingState("Attack") && !IsPlayingState("Hurt"))
            {
                anim.SetTrigger("Attack");
            }
        }
    }

    // ���˴������ɵ��˵��ã�
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        anim.SetTrigger("Hurt");

        // ��ʾ��ǰ����ֵ����ѡ��
        Debug.Log($"�ܵ��˺�: {damage}, ʣ������: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Dead");

        // �����������ײ
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;

        Debug.Log("��ɫ������");
    }

    // ����״̬��⸨������
    bool IsPlayingState(string stateName)
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    // �ڹ����������һ֡���ã�ͨ�������¼���
    public void OnAttackEnd()
    {
        anim.ResetTrigger("Attack");
    }

    // �����˶������һ֡���ã�ͨ�������¼���
    public void OnHurtEnd()
    {
        anim.ResetTrigger("Hurt");
    }

    public void ResetPlayer()
    {
        currentHealth = maxHealth;
        isDead = false;
        rb.simulated = true;
        GetComponent<Collider2D>().enabled = true;
        anim.Play("Idle");
    }
}