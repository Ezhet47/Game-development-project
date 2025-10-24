using UnityEngine;

public class InteractionDetect : MonoBehaviour
{
    public GameObject buttonSprite;
    public Transform playerTrans;
    public bool canpress;
    private Collectable current;

    private void Update()
    {
        if (buttonSprite) buttonSprite.SetActive(canpress);

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
        var tp = other.GetComponentInParent<Teleport>();     

        bool available = false;
        if (col != null) available |= col.IsInteractable;
        if (dlg != null) available |= dlg.IsInteractable;
        if (tp != null) available |= tp.IsInteractable;      

        canpress = available;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Interactable")) return;

        if (current && other.GetComponentInParent<Collectable>() == current)
            current = null;

        canpress = false;
    }

    public void ForceHidePrompt()
    {
        canpress = false;
    }
}

