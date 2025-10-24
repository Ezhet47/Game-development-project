using UnityEngine;
using TMPro;
using System.Collections;

public class Tutorial : MonoBehaviour
{
    public TextMeshProUGUI text;           
    public float displayTime = 5f;
    public CanvasGroup canvasGroup;         

    [Header("Auto Start")]
    [TextArea] public string startMessage = "Welcome!";
    public bool showOnStart = true;

    private Coroutine currentRoutine;
    private bool isShowing = false;
    
    private Player cachedPlayer;

    void Awake()
    {
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>(true);
        
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        cachedPlayer = FindFirstObjectByType<Player>();

        if (showOnStart)
            Show(startMessage);
    }

    public void Show(string msg)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);
        
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>(true);

        StopAllCoroutines();
        if (text != null) text.text = msg;
        currentRoutine = StartCoroutine(ShowAndHide());
    }

    private IEnumerator ShowAndHide()
    {
        isShowing = true;
        
        if (cachedPlayer == null)
            cachedPlayer = FindFirstObjectByType<Player>();
        if (cachedPlayer != null)
            cachedPlayer.canMove = false;
        
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        
        float t = 0f;
        float fadeIn = 0.25f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeIn);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;
        
        float timer = 0f;
        while (timer < displayTime && isShowing)
        {
            if (Input.GetMouseButtonDown(1)||Input.GetMouseButtonDown(0)) 
                break;
            timer += Time.deltaTime;
            yield return null;
        }
        
        t = 0f;
        float fadeOut = 0.25f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOut);
            yield return null;
        }

        if (canvasGroup != null) 
            canvasGroup.alpha = 0f;

        gameObject.SetActive(false);
        isShowing = false;

  
        if (cachedPlayer == null)
            cachedPlayer = FindFirstObjectByType<Player>();
        if (cachedPlayer != null)
            cachedPlayer.canMove = true;
        
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
    
    public void AbortAndHide()
    {
        StopAllCoroutines();
        isShowing = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        gameObject.SetActive(false);

        if (cachedPlayer == null)
            cachedPlayer = FindFirstObjectByType<Player>();
        if (cachedPlayer != null)
            cachedPlayer.canMove = true;
    }
}
