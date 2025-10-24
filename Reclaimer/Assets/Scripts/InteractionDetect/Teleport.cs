using UnityEngine;
using TMPro;
using System.Collections;

public class Teleport : MonoBehaviour
{
    [Header("Interaction")]
    public InteractionDetect otherScript;
    private bool collide = true;
    private bool playerInRange = false;

    [Header("Requirement")]
    public int requiredTotal = 7;            
    [Tooltip("ComponentCount.instance.totalComponents>=0 ")]
    public int currentCountOverride = -1;         

    [Header("UI Hint")]
    public TextMeshProUGUI notEnoughText;      
    public CanvasGroup notEnoughGroup;          
    public string notEnoughMessage = "0";
    public float hintFade = 0.15f;              
    public float hintShowTime = 1.25f;            


    public bool IsInteractable
    {
        get
        {
            bool dialogueBusy = UI_CollectionDialogue.Instance != null && UI_CollectionDialogue.Instance.IsShowing;
            return playerInRange && collide && !dialogueBusy;
        }
    }

    private void Awake()
    {
        if (notEnoughGroup == null && notEnoughText != null)
            notEnoughGroup = notEnoughText.GetComponent<CanvasGroup>();

        if (notEnoughGroup != null)
        {
            notEnoughGroup.alpha = 0f;
            notEnoughGroup.blocksRaycasts = false;
            notEnoughGroup.interactable = false;
        }
        if (notEnoughText != null && !string.IsNullOrEmpty(notEnoughMessage))
            notEnoughText.text = notEnoughMessage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerInRange = false;
    }

    private void Update()
    {
        if (!(playerInRange && collide)) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            int cur = GetCurrentCollected();

            if (cur >= requiredTotal)
            {
                collide = false;
                if (otherScript) otherScript.canpress = false; 

                GameManager.Instance.HasPlayedPuzzle = false;
                GameManager.Instance.GoToMainSceneBefore();
            }
            else
            {
                ShowNotEnoughHint();
            }
        }
    }

    private int GetCurrentCollected()
    {
        if (currentCountOverride >= 0) return currentCountOverride;

        if (ComponentCount.instance != null)
            return ComponentCount.instance.totalComponents;
        return 0;
    }

    private void ShowNotEnoughHint()
    {
        if (notEnoughText == null && notEnoughGroup == null) return; 

        if (notEnoughText != null && !string.IsNullOrEmpty(notEnoughMessage))
            notEnoughText.text = notEnoughMessage;

        StopAllCoroutines();
        StartCoroutine(CoFlashHint());
    }

    private IEnumerator CoFlashHint()
    {
        if (notEnoughGroup != null)
        {
            notEnoughGroup.blocksRaycasts = true;
            notEnoughGroup.interactable = true;

            float t = 0f;
            while (t < hintFade)
            {
                t += Time.deltaTime;
                notEnoughGroup.alpha = Mathf.Lerp(0f, 1f, t / hintFade);
                yield return null;
            }
            notEnoughGroup.alpha = 1f;

            yield return new WaitForSeconds(hintShowTime);
            t = 0f;
            while (t < hintFade)
            {
                t += Time.deltaTime;
                notEnoughGroup.alpha = Mathf.Lerp(1f, 0f, t / hintFade);
                yield return null;
            }
            notEnoughGroup.alpha = 0f;
            notEnoughGroup.blocksRaycasts = false;
            notEnoughGroup.interactable = false;
        }
        else if (notEnoughText != null)
        {
            notEnoughText.gameObject.SetActive(true);
            yield return new WaitForSeconds(hintShowTime);
            notEnoughText.gameObject.SetActive(false);
        }
    }
}
