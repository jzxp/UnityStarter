using UnityEngine;
using System.Collections;

public class CharacterController : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("战斗设置")]
    public float attackRange = 1f;
    public int attackDamage = 10;
    public float attackCooldown = 0.5f;
    public float invincibleTime = 1f;

    [Header("动画剪辑")]
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip attackClip;
    public AnimationClip hurtClip;
    public AnimationClip deadClip;

    // 组件引用
    private Rigidbody2D rb;
    private SpriteAnimator animator;
    private Transform groundCheck;

    // 状态变量
    private bool isGrounded;
    private bool isAttacking;
    private bool isHurt;
    private bool isDead;
    private float attackTimer;
    private float invincibleTimer;
    private int health = 100;

    // 输入变量
    private float moveInput;
    private bool jumpInput;
    private bool attackInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<SpriteAnimator>();

        // 创建地面检测点
        groundCheck = new GameObject("GroundCheck").transform;
        groundCheck.SetParent(transform);
        groundCheck.localPosition = new Vector3(0, -0.5f, 0);
    }

    void Update()
    {
        if (isDead) return;

        // 获取玩家输入
        GetPlayerInput();

        // 更新状态
        UpdateState();

        // 更新动画
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        if (isDead || isHurt) return;

        // 物理移动
        HandleMovement();
    }

    #region 输入处理
    private void GetPlayerInput()
    {
        moveInput = Input.GetAxis("Horizontal");
        jumpInput = Input.GetButtonDown("Jump");
        attackInput = Input.GetButtonDown("Fire1");

        // 攻击冷却
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
    }
    #endregion

    #region 状态管理
    private void UpdateState()
    {
        // 检测地面
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // 无敌时间
        if (invincibleTimer > 0) invincibleTimer -= Time.deltaTime;
    }
    #endregion

    #region 物理移动
    private void HandleMovement()
    {
        // 水平移动
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // 跳跃
        if (jumpInput && isGrounded)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // 翻转角色朝向
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

    #region 动画管理
    private void UpdateAnimation()
    {
        if (isDead) return;

        // 受伤状态优先
        if (isHurt)
        {
            animator.PlayAnimation(hurtClip.name);
            return;
        }

        // 攻击状态
        if (isAttacking)
        {
            animator.PlayAnimation(attackClip.name);
            return;
        }

        // 移动状态
        if (Mathf.Abs(moveInput) > 0.1f)
        {
            animator.PlayAnimation(walkClip.name);
        }
        // 跳跃/下落状态
        else if (!isGrounded)
        {
            // 使用空闲动画作为跳跃/下落的替代
            animator.PlayAnimation(idleClip.name);
        }
        // 空闲状态
        else
        {
            animator.PlayAnimation(idleClip.name);
        }

        // 处理攻击输入
        if (attackInput && attackTimer <= 0)
        {
            StartAttack();
        }
    }
    #endregion

    #region 战斗系统
    private void StartAttack()
    {
        isAttacking = true;
        attackTimer = attackCooldown;
        animator.PlayAnimation(attackClip.name);

        // 攻击检测
        StartCoroutine(DetectAttackHit());

        // 结束攻击
        Invoke("EndAttack", attackClip.length);
    }

    private IEnumerator DetectAttackHit()
    {
        // 等待攻击动画到达攻击帧
        yield return new WaitForSeconds(attackClip.length * 0.3f);

        // 检测前方敌人
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
            // 受伤状态
            StartCoroutine(HurtState());
        }
    }

    private IEnumerator HurtState()
    {
        isHurt = true;
        animator.PlayAnimation(hurtClip.name);

        // 击退效果
        Vector2 knockbackDirection = new Vector2(
            -Mathf.Sign(transform.localScale.x),
            0.5f
        ).normalized;
        rb.AddForce(knockbackDirection * 5f, ForceMode2D.Impulse);

        // 受伤状态持续时间
        yield return new WaitForSeconds(hurtClip.length);

        isHurt = false;
    }

    private void Die()
    {
        isDead = true;
        animator.PlayAnimation(deadClip.name);

        // 禁用碰撞和物理
        GetComponent<Collider2D>().enabled = false;
        rb.simulated = false;

        // 可选：销毁角色
        // Destroy(gameObject, deadClip.length);
    }
    #endregion

    // 调试辅助
    private void OnDrawGizmosSelected()
    {
        // 地面检测范围
        Gizmos.color = Color.green;
        if (groundCheck != null)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        // 攻击范围
        Gizmos.color = Color.red;
        Vector2 attackDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        Gizmos.DrawRay(transform.position, attackDirection * attackRange);
    }
}