using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    [Header("�ƶ�����")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("ս������")]
    public float attackRange = 1f;
    public int attackDamage = 10;
    public float attackCooldown = 0.5f;
    public float invincibleTime = 1f;

    [Header("��������")]
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip attackClip;
    public AnimationClip hurtClip;
    public AnimationClip deadClip;

    // �������
    private Rigidbody2D rb;
    private SpriteAnimator animator;
    private Transform groundCheck;

    // ״̬����
    private bool isGrounded;
    private bool isAttacking;
    private bool isHurt;
    private bool isDead;
    private float attackTimer;
    private float invincibleTimer;
    private int health = 100;

    // �������
    private float moveInput;
    private bool jumpInput;
    private bool attackInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<SpriteAnimator>();

        // �����������
        groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.SetParent(transform);
        groundCheck.localPosition = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        if (isDead) return;

        // ��ȡ�������
        GetPlayerInput();

        // ����״̬
        UpdateState();

        // ���¶���
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (isDead || isHurt) return;

        // �����ƶ�
        HandleMovement();
    }

    #region ���봦��
    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Horizontal");
        jumpInput = Input.GetButtonDown("Jump");
        attackInput = Input.GetButtonDown("Fire1");

        // ������ȴ
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
    }
    #endregion

    #region ״̬����
    private void UpdateState()
    {
        // ������
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // �޵�ʱ��
        if (invincibleTimer > 0) invincibleTimer -= Time.deltaTime;
    }
    #endregion

    #region �����ƶ�
    private void HandleMovement()
    {
        // ˮƽ�ƶ�
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // ��Ծ
        if (jumpInput && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // ��ת��ɫ����
        if (moveInput != 0)
        {
            transform.localScale = new Vector3(
                Mathf.Sign(moveInput) * Mathf.Abs(transform.localScale.x),
                transform.localScale.y,
                transform.localScale.z
            );
        }
    }
    #endregion

    #region ��������
    private void UpdateAnimation()
    {
        if (isDead) return;

        // ����״̬����
        if (isHurt)
        {
            animator.PlayAnimation(hurtClip.name);
            return;
        }

        // ����״̬
        if (isAttacking)
        {
            animator.PlayAnimation(attackClip.name);
            return;
        }

        // �ƶ�״̬
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            animator.PlayAnimation(walkClip.name);
        }
        // ��Ծ/����״̬
        else if (!isGrounded)
        {
            // ʹ�ÿ��ж�����Ϊ��Ծ/��������
            animator.PlayAnimation(idleClip.name);
        }
        // ����״̬
        else
        {
            animator.PlayAnimation(idleClip.name);
        }

        // ����������
        if (attackInput && attackTimer <= 0)
        {
            StartAttack();
        }
    }
    #endregion

    #region ս��ϵͳ
    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        animator.PlayAnimation(attackClip.name);

        // �������
        StartCoroutine(DetectAttackHit());

        // ��������
        Invoke("EndAttack", attackClip.length);
    }

    private IEnumerator DetectAttackHit()
    {
        // �ȴ������������﹥��֡
        yield return new WaitForSeconds(attackClip.length * 0.3f);

        // ���ǰ������
        Vector2 attackDirection = new Vector2(transform.localScale.x > 0 ? 1 : -1, 0);
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            attackDirection,
            attackRange,
            LayerMask.GetMask("Enemy")
        );

        if (hit.collider != null)
        {
            hit.collider.GetComponent<Enemy>().TakeDamage(attackDamage);
        }
    }

    private void EndAttack()
    {
        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || invincibleTimer > 0) return;

        health -= damage;
        invincibleTimer = invincibleTime;

        if (health <= 0)
        {
            Die();
        }
        else
        {
            // ����״̬
            StartCoroutine(HurtState());
        }
    }

    private IEnumerator HurtState()
    {
        isHurt = true;
        animator.PlayAnimation(hurtClip.name);

        // ����Ч��
        Vector2 knockbackDirection = new Vector2(
            -Mathf.Sign(transform.localScale.x),
            0.5f
        ).normalized;
        rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);

        // ����״̬����ʱ��
        yield return new WaitForSeconds(hurtClip.length);

        isHurt = false;
    }

    private void Die()
    {
        isDead = true;
        animator.PlayAnimation(deadClip.name);

        // ������ײ������
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;

        // ��ѡ�����ٽ�ɫ
        // Destroy(gameObject, deadClip.length);
    }
    #endregion

    // ���Ը���
    private void OnDrawGizmosSelected()
    {
        // �����ⷶΧ
        Gizmos.color = Color.green;
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // ������Χ
        Gizmos.color = Color.red;
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, attackDirection * attackRange);
    }
}