using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] pages;               // ��ҳ�ı�
    public Transform bubbleAnchor;       // ���ݸ���ĵ㣨����͸�����ң�
    public KeyCode interactKey = KeyCode.E;

    private bool playerIn;
    private Player cachedPlayer;
    public bool IsInteractable
    {
        get
        {
            // �������ײ���� �� ��ǰû�жԻ��ڽ��� ����ɽ���
            var ui = DialogueUI.Instance;
            bool dialogueBusy = (ui != null && ui.IsShowing);
            return playerIn && !dialogueBusy;   // playerIn ����ű������еĲ���ֵ
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
            var ui = DialogueUI.Instance;
            if (ui == null) { Debug.LogWarning("No DialogueUI in scene."); return; }

            // �Ѿ��ڶԻ������� UI �Լ�����E ���Ჹ��/��һҳ��
            if (ui.IsShowing) return;

            // ��ʼ�Ի�
            Transform follow = bubbleAnchor ? bubbleAnchor : (cachedPlayer ? cachedPlayer.transform : transform);
            ui.StartDialogue(pages, follow, cachedPlayer);
        }
    }
}
