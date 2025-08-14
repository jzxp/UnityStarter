using UnityEngine;

public class PlayerTester : MonoBehaviour
{
    public PlayerController player;

    void Update()
    {
        // 模拟敌人攻击（按H键造成10点伤害）
        if (Input.GetKeyDown(KeyCode.H))
        {
            player.TakeDamage(10);
        }

        // 重置角色（按R键）
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.ResetPlayer();
        }
    }
}