using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // 公开可配置参数
    public float moveSpeed = 5f;
    public int maxHealth = 100;

    // 组件引用
    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    // 状态变量
    private int currentHealth;
    private bool isDead = false;
    private bool isFacingRight = true;

    void Start()
    {
        // 获取组件引用
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 初始化状态
        currentHealth = maxHealth;
        isFacingRight = true;

        // 优化 Animator 性能
        anim.updateMode = AnimatorUpdateMode.UnscaledTime; // 不受 Time.timeScale 影响
        anim.cullingMode = AnimatorCullingMode.AlwaysAnimate; // 始终渲染动画
    }

    void Update()
    {
        if (isDead) return; // 死亡后停止所有操作

        HandleMovement();
        HandleAttack();
    }

    void HandleMovement()
    {
        // 获取水平输入 (A/D 或 左右箭头)
        float moveX = Input.GetAxisRaw("Horizontal");

        // 移动角色
        rb.velocity = new Vector2(moveX * moveSpeed, rb.velocity.y);

        // 更新动画参数
        anim.SetFloat("Speed", Mathf.Abs(moveX));

        // 角色朝向翻转
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
        // 切换朝向并翻转精灵
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    void HandleAttack()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            // 确保不在其他动作中
            if (!IsPlayingState("Attack") && !IsPlayingState("Hurt"))
            {
                anim.SetTrigger("Attack");
            }
        }
    }

    // 受伤处理（可由敌人调用）
    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        anim.SetTrigger("Hurt");

        // 显示当前生命值（可选）
        Debug.Log($"受到伤害: {damage}, 剩余生命: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        isDead = true;
        anim.SetTrigger("Dead");

        // 禁用物理和碰撞
        rb.velocity = Vector2.zero;
        rb.simulated = false;
        GetComponent<Collider2D>().enabled = false;

        Debug.Log("角色已死亡");
    }

    // 动画状态检测辅助方法
    bool IsPlayingState(string stateName)
    {
        return anim.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    // 在攻击动画最后一帧调用（通过动画事件）
    public void OnAttackEnd()
    {
        anim.ResetTrigger("Attack");
    }

    // 在受伤动画最后一帧调用（通过动画事件）
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