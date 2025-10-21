using UnityEngine;

public class InteractionDetect : MonoBehaviour
{
    public GameObject buttonSprite;
    public Transform playerTrans;
    public bool canpress;

    // 新增：当前重叠的可交互对象（可选，仅用于健壮性）
    private Collectable current;

    private void Update()
    {
        // 显示/隐藏 E
        if (buttonSprite) buttonSprite.SetActive(canpress);

        // 朝向翻转
        if (canpress && playerTrans && buttonSprite)
        {
            buttonSprite.transform.localScale = -playerTrans.localScale;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Interactable")) return;

        // 既支持 Collectable，也支持 DialogueTrigger
        var col = other.GetComponentInParent<Collectable>();
        var dlg = other.GetComponentInParent<DialogueTrigger>();

        bool available = false;
        if (col != null) available |= col.IsInteractable;        // 你在 Collectable 里已经加过 IsInteractable
        if (dlg != null) available |= dlg.IsInteractable;        // 第1步新增的属性

        canpress = available;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Interactable")) return;

        // 离开触发范围，关掉 E
        if (current && other.GetComponentInParent<Collectable>() == current)
            current = null;

        canpress = false;
    }

    // 可选：给外部强制关提示（你已在 Collectable 里有类似用法）
    public void ForceHidePrompt()
    {
        canpress = false;
    }
}
