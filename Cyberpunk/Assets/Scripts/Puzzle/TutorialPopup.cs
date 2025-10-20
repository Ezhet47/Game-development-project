using UnityEngine;
using TMPro;
using System.Collections;

public class TutorialPopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float displayTime = 5f; // 显示秒数
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
        canvasGroup.alpha = 1;
        StartCoroutine(HideAfterDelay());
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);

        // 淡出效果
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }
        gameObject.SetActive(false);
    }
}
