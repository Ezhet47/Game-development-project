using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialPopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float displayTime = 5f;
    public CanvasGroup canvasGroup;

    void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void Show(string msg)
    {
        StopAllCoroutines();
        text.text = msg;
        gameObject.SetActive(true);
        StartCoroutine(ShowAndHide());
    }

    private IEnumerator ShowAndHide()
    {
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
            yield return null;
        }
        canvasGroup.alpha = 1f;

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1));

        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
