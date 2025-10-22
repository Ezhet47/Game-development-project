using UnityEngine;

public class Object_Clara : Object_NPC
{
    [Header("Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;
    [SerializeField] private DialogueLineSO secondDialogueLine;

    private bool switched;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Interact()
    {
        base.Interact();
        
        bool canSwitch = (secondDialogueLine != null);
        
        var startLine = (switched && canSwitch) ? secondDialogueLine : firstDialogueLine;
        
        if (!switched && canSwitch && ui != null && ui.dialogueUI != null)
        {
            ui.dialogueUI.DialogueClosed -= OnDialogueClosedFirstTime;
            ui.dialogueUI.DialogueClosed += OnDialogueClosedFirstTime;
        }

        ui.OpenDialogueUI(startLine);
    }

    private void OnDialogueClosedFirstTime()
    {
        if (secondDialogueLine != null)
        {
            switched = true;
        }
        
        if (ui != null && ui.dialogueUI != null)
        {
            ui.dialogueUI.DialogueClosed -= OnDialogueClosedFirstTime;
        }
    }
}