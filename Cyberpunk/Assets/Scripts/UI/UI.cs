using UnityEngine;
using UnityEngine.InputSystem;

public class UI : MonoBehaviour
{
    public static UI instance { get; private set; }
    
    public GameObject bg;
    public GameObject chipPanel;
    public GameObject victoryPanel;
    
    public UI_Dialogue dialogueUI { get; private set; }
    public UI_InGame inGameUI { get; private set; }
    public UI_FadeScreen fadeScreenUI { get; private set; }
    public UI_Options optionsUI { get; private set; }
    
    [SerializeField] private GameObject[] uiElements;
    private PlayerInputSet input;
    
    private void Awake()
    {
        instance = this;
        
        inGameUI = GetComponentInChildren<UI_InGame>(true);
        dialogueUI = GetComponentInChildren<UI_Dialogue>(true);
        fadeScreenUI = GetComponentInChildren<UI_FadeScreen>(true);
        optionsUI = GetComponentInChildren<UI_Options>(true);
        
        input = new PlayerInputSet();
        input.UI.Enable();

        input.UI.DialogueInteraction.performed += OnDialogueInteraction;
        input.UI.DialogueNavigation.performed += OnDialogueNavigation;
        
        input.UI.OptionsUI.performed += OnOptionsToggle;
    }
    

    private void Start()
    {
        if (bg == null || chipPanel == null || victoryPanel == null) return;
        
        if (GameManager.Instance.PuzzleCompleted)
        {
            bg.SetActive(false);
            chipPanel.SetActive(false);
            victoryPanel.SetActive(true);

            GameManager.Instance.PuzzleCompleted = false;
        }
        else if (GameManager.Instance.HasPlayedPuzzle)
        {
            bg.SetActive(false);
            chipPanel.SetActive(true);
            victoryPanel.SetActive(false);
        }
        else
        {
            bg.SetActive(true);
            chipPanel.SetActive(false);
            victoryPanel.SetActive(false);
        }

        GameManager.Instance.HasPlayedPuzzle = true;
    }
    
    public void GoToMain()
    {
        GameManager.Instance.PuzzleCompleted = false;
        GameManager.Instance.HasPlayedPuzzle = false;
        GameManager.Instance.GoToMainSceneBefore();
    }

    public void ReturnToPanel()
    {
        GameManager.Instance.GoToMainSceneBefore();
    }

    public void GoToPuzzle()
    {
        GameManager.Instance.GoToPuzzleScene();
    }

    public void GoToCollect()
    {
        GameManager.Instance.GoToCollection();
    }

    public void OpenChipPanel()
    {
        chipPanel.SetActive(true);
        bg.SetActive(false);
    }

    public void CloseChipPanel()
    {
        chipPanel.SetActive(false);
        bg.SetActive(true);
    }
    
    public void OpenDialogueUI(DialogueLineSO firstLine)
    {
        dialogueUI.gameObject.SetActive(true);
        dialogueUI.PlayDialogueLine(firstLine);
        
    }
    
    private void SwitchTo(GameObject objectToSwitchOn)
    {
        foreach (var element in uiElements)
            element.gameObject.SetActive(false);

        objectToSwitchOn.SetActive(true);
    }
    
    public void SwitchToInGameUI()
    {
        SwitchTo(inGameUI.gameObject);
    }

    private void OnDialogueInteraction(InputAction.CallbackContext ctx)
    {
        if (dialogueUI != null && dialogueUI.gameObject.activeInHierarchy)
            dialogueUI.DialogueInteraction();
    }

    private void OnDialogueNavigation(InputAction.CallbackContext ctx)
    {
        if (dialogueUI != null && dialogueUI.gameObject.activeInHierarchy)
        {
            int direction = Mathf.RoundToInt(ctx.ReadValue<float>());
            dialogueUI.NavigateChoice(direction);
        }
    }
    
    private void OnOptionsToggle(InputAction.CallbackContext ctx)
    {
        if(optionsUI == null) return;
        
        if (optionsUI.gameObject.activeInHierarchy)
        {
            SwitchToInGameUI();
        }
        else
        {
            SwitchTo(optionsUI.gameObject);
        }
    }
    
    private void OnDestroy()
    {
        if (input != null)
        {
            input.UI.DialogueInteraction.performed -= OnDialogueInteraction;
            input.UI.DialogueNavigation.performed -= OnDialogueNavigation;
            input.UI.OptionsUI.performed -= OnOptionsToggle;
            input.Dispose();
        }
    }
    
    private void OnEnable()
    {
        input?.Enable();
    }
    
    private void OnDisable()
    {
        input?.Disable();
    }
}
