using UnityEngine;

public class Collectable : MonoBehaviour
{
    private int score = 1;                 // 如果不需要加分可改为 0 或删掉相关两行
    private bool collide = true;
    private bool playerInRange = false;

    public InteractionDetect otherScript;  // 玩家身上的 InteractionDetect（Inspector 拖入）
    public Transform focusPoint;           // QTE镜头与UI对焦点，空则用物体自身

    private Player cachedPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            cachedPlayer = collision.GetComponentInParent<Player>();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (collision.GetComponentInParent<Player>() == cachedPlayer)
                cachedPlayer = null;
        }
    }

    private void Update()
    {
        if (playerInRange && collide && Input.GetKeyDown(KeyCode.E))
        {
            // 防抖：进入QTE前关提示+锁一次触发
            collide = false;
            if (otherScript) otherScript.canpress = false;

            // 冻结玩家移动
            if (cachedPlayer) cachedPlayer.canMove = false;

            // 设定对焦点
            Transform focus = focusPoint ? focusPoint : transform;

            // 启动QTE
            UI_QTE.Instance.StartSkillCheck(
                focus,
                success: () =>
                {
                    // 恢复移动
                    if (cachedPlayer) cachedPlayer.canMove = true;

                    // 可选：加分（你的工程里是 ComponentCount.instance）
                    if (ComponentCount.instance != null && score != 0)
                    {
                        ComponentCount.instance.totalComponents += score;
                        ComponentCount.instance.UpdateTotalScore();
                        
                    }
                    GameManager.Instance.HasCollected = true;

                    // 成功：销毁
                    Destroy(gameObject);
                },
                fail: () =>
                {
                    // 失败：恢复可重试
                    if (cachedPlayer) cachedPlayer.canMove = true;
                    collide = true;
                    if (otherScript) otherScript.canpress = true;
                }
            );
        }
    }
}
