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

        // ��֧�� Collectable��Ҳ֧�� DialogueTrigger
        var col = other.GetComponentInParent<Collectable>();
        var dlg = other.GetComponentInParent<DialogueTrigger>();

        bool available = false;
        if (col != null) available |= col.IsInteractable;        // ���� Collectable ���Ѿ��ӹ� IsInteractable
        if (dlg != null) available |= dlg.IsInteractable;        // ��1������������

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
