using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour
{
    [TextArea(2, 5)]
    public string[] pages;               
    public Transform bubbleAnchor;       
    public KeyCode interactKey = KeyCode.E;

    [Header("hint")]
    public GameObject hintObject;        

    private bool playerIn;
    private Player cachedPlayer;

    public bool IsInteractable
    {
        get
        {
            var ui = UI_CollectionDialogue.Instance;
            bool dialogueBusy = (ui != null && ui.IsShowing);
            return playerIn && !dialogueBusy;  
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
            if (ui.IsShowing) return;

            Transform follow = bubbleAnchor ? bubbleAnchor : (cachedPlayer ? cachedPlayer.transform : transform);
            ui.StartDialogue(pages, follow, cachedPlayer);

            if (hintObject != null)
            {
                Destroy(hintObject);
                hintObject = null; 
            }
        }
    }
}
