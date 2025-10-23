using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ComponentsPanel : MonoBehaviour
{
    [Header("Toggle")]
    public KeyCode toggleKey = KeyCode.Space;   // ?????
    public float fadeTime = 0.15f;              // ??ç–???????

    [Header("UI Refs")]
    public CanvasGroup canvasGroup;             // ?????????? CanvasGroup
    public TextMeshProUGUI counterText;         // ??X / 7?? ???
    public Image progressFill;                  // ?????????????Image type = Filled??

    [Header("Goal")]
    public int targetTotal = 7;                 // ????????????

    [Header("Animator (???)")]
    public Animator animator;                   // ???ß‘???/??????????????
    public string showTrigger = "Show";         // ????????????? Trigger ??
    public string hideTrigger = "Hide";         // ???????????? Trigger ??

    private bool isOpen = false;                // ???????
    private bool contentVisible = false;        // UI ????????????????
    private Player cachedPlayer;                // ????????/??????

    public AudioSource audioSource;
    public AudioClip open;

    void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // ?????????????ÂÔ?????
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
        isOpen = false;
        contentVisible = false;

        // ??????????? canMove ?????
        cachedPlayer = FindObjectOfType<Player>();

        // ???????? Animator???????? UnscaledTime ????? ???? ???????????????
        if (animator)
            animator.updateMode = AnimatorUpdateMode.Normal; // ??????????∞È??
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();

    
        if (isOpen && contentVisible)
            RefreshTextAndBar();
    }

    public void Toggle()
    {
        isOpen = !isOpen;

        if (animator && (showTrigger.Length > 0 || hideTrigger.Length > 0))
        {
            if (isOpen && showTrigger.Length > 0) audioSource.PlayOneShot(open);animator.SetTrigger(showTrigger);
                if (!isOpen && hideTrigger.Length > 0) animator.SetTrigger(hideTrigger);
        }

        StopAllCoroutines();
        StartCoroutine(Fade(isOpen));

        // ? ???????????/??????????
        if (cachedPlayer != null)
            cachedPlayer.canMove = !isOpen;

        // ??????????????????????????????
        if (counterText) counterText.alpha = 0f;
        contentVisible = false;
    }

    private IEnumerator Fade(bool show)
    {
        float start = canvasGroup.alpha;
        float end = show ? 1f : 0f;
        canvasGroup.blocksRaycasts = show;
        canvasGroup.interactable = show;

        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime; // ????????????? Time.deltaTime
            canvasGroup.alpha = Mathf.Lerp(start, end, t / fadeTime);
            yield return null;
        }
        canvasGroup.alpha = end;

        // ??????????????????????????
        if (show)
        {
            yield return new WaitForSeconds(0.05f);
            if (counterText)
                StartCoroutine(FadeInText());
        }
        else
        {
            if (counterText) counterText.alpha = 0f;
            contentVisible = false;
        }
    }

    private IEnumerator FadeInText()
    {
        contentVisible = true;

        if (counterText)
        {
            float t = 0f, dur = 0.2f;
            while (t < dur)
            {
                t += Time.deltaTime;
                counterText.alpha = Mathf.Lerp(0f, 1f, t / dur);
                yield return null;
            }
            counterText.alpha = 1f;
        }
        // ?????¶≤??????????????????
        RefreshTextAndBar();
    }

    private void RefreshTextAndBar()
    {
        int cur = 0;
        if (ComponentCount.instance != null)
            cur = ComponentCount.instance.totalComponents;

        if (counterText)
            counterText.text = $"{cur} / {targetTotal}";

        if (progressFill)
            progressFill.fillAmount = Mathf.Clamp01(targetTotal > 0 ? (float)cur / targetTotal : 0f);
    }

    private void OnDisable()
    {
        // ??å„????????????????
        if (cachedPlayer != null)
            cachedPlayer.canMove = true;

        isOpen = false;
        contentVisible = false;

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}
