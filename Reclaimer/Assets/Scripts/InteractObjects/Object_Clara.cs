using UnityEngine;

public class Object_Clara : Object_NPC
{
    [Header("Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;
    [SerializeField] private DialogueLineSO secondDialogueLine;
    
    private static bool switchedThisSession;
    private bool subscribed;

    protected override void Awake()
    {
        base.Awake();
        
        if (switchedThisSession && secondDialogueLine != null)
        {
            firstDialogueLine = secondDialogueLine;
        }
    }

    public override void Interact()
    {
        base.Interact();

        bool canSwitch = (firstDialogueLine != null && secondDialogueLine != null);
        
        var startLine = (switchedThisSession && canSwitch) ? secondDialogueLine : firstDialogueLine;
        
        bool showingFirstNow = (!switchedThisSession && canSwitch && startLine == firstDialogueLine);

        if (showingFirstNow && ui != null && ui.dialogueUI != null && !subscribed)
        {
            ui.dialogueUI.DialogueClosed -= OnDialogueClosedFirstTime;
            ui.dialogueUI.DialogueClosed += OnDialogueClosedFirstTime;
            subscribed = true;
        }

        ui.OpenDialogueUI(startLine);
    }

    private void OnDialogueClosedFirstTime()
    {
        if (firstDialogueLine != null && secondDialogueLine != null)
        {
            switchedThisSession = true;             
            firstDialogueLine = secondDialogueLine;     
        }
        
        if (ui != null && ui.dialogueUI != null)
        {
            ui.dialogueUI.DialogueClosed -= OnDialogueClosedFirstTime;
        }
        subscribed = false;
    }
    
    private void OnDisable()
    {
        if (ui != null && ui.dialogueUI != null)
        {
            ui.dialogueUI.DialogueClosed -= OnDialogueClosedFirstTime;
        }
        subscribed = false;
    }
}
