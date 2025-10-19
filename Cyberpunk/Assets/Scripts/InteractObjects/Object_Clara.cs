using UnityEngine;

public class Object_Clara : Object_NPC
{
    [Header("Dialogue")]
    [SerializeField] private DialogueLineSO firstDialogueLine;
    
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
        
        ui.OpenDialogueUI(firstDialogueLine);
    }
}
