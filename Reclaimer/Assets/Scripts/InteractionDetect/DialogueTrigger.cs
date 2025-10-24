using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] pages;               // 多页文本
    public Transform bubbleAnchor;       // 气泡跟随的点（不填就跟随玩家）
    public KeyCode interactKey = KeyCode.E;

    // 👇 新增：提示物体引用
    [Header("提示物体（可选）")]
    public GameObject hintObject;        // 挂你在触发器下的提示（比如“按E”）

    private bool playerIn;
    private Player cachedPlayer;

    public bool IsInteractable
    {
        get
        {
            // 玩家在碰撞框内 且 当前没有对话在进行 才算可交互
            var ui = UI_CollectionDialogue.Instance;
            bool dialogueBusy = (ui != null && ui.IsShowing);
            return playerIn && !dialogueBusy;   // playerIn 是你脚本里已有的布尔值
        }
    }

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerIn = true;
        cachedPlayer = other.GetComponentInParent<Player>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerIn = false;
        cachedPlayer = null;
    }

    private void Update()
    {
        if (!playerIn) return;

        if (Input.GetKeyDown(interactKey))
        {
            var ui = UI_CollectionDialogue.Instance;
            if (ui == null) { Debug.LogWarning("No DialogueUI in scene."); return; }

            // 已经在对话中则交由 UI 自己处理（E 键会补完/下一页）
            if (ui.IsShowing) return;

            // 开始对话
            Transform follow = bubbleAnchor ? bubbleAnchor : (cachedPlayer ? cachedPlayer.transform : transform);
            ui.StartDialogue(pages, follow, cachedPlayer);

            // 👇 新增：开始对话后销毁提示物体
            if (hintObject != null)
            {
                Destroy(hintObject);
                hintObject = null; // 防止重复引用
            }
        }
    }
}
