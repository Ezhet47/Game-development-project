using UnityEngine;

public class InteractionDetect : MonoBehaviour
{
    public GameObject buttonSprite;
    public Transform playerTrans;
    public bool canpress;

    // ��������ǰ�ص��Ŀɽ������󣨿�ѡ�������ڽ�׳�ԣ�
    private Collectable current;

    private void Update()
    {
        // ��ʾ/���� E
        if (buttonSprite) buttonSprite.SetActive(canpress);

        // ����ת
        if (canpress && playerTrans && buttonSprite)
        {
            buttonSprite.transform.localScale = -playerTrans.localScale;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Interactable")) return;

        var col = other.GetComponentInParent<Collectable>();
        var dlg = other.GetComponentInParent<DialogueTrigger>();
        var tp = other.GetComponentInParent<Teleport>();     // ? ����

        bool available = false;
        if (col != null) available |= col.IsInteractable;
        if (dlg != null) available |= dlg.IsInteractable;
        if (tp != null) available |= tp.IsInteractable;      // ? ����

        canpress = available;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Interactable")) return;

        // �뿪������Χ���ص� E
        if (current && other.GetComponentInParent<Collectable>() == current)
            current = null;

        canpress = false;
    }

    // ��ѡ�����ⲿǿ�ƹ���ʾ�������� Collectable ���������÷���
    public void ForceHidePrompt()
    {
        canpress = false;
    }
}
