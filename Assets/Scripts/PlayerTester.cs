using UnityEngine;

public class PlayerTester : MonoBehaviour
{
    public PlayerController player;

    void Update()
    {
        // ģ����˹�������H�����10���˺���
        if (Input.GetKeyDown(KeyCode.H))
        {
            player.TakeDamage(10);
        }

        // ���ý�ɫ����R����
        if (Input.GetKeyDown(KeyCode.R))
        {
            player.ResetPlayer();
        }
    }
}