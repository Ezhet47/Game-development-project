using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Popup : MonoBehaviour
{
    [Header("Refs")]
    public Canvas targetCanvas;                 
    public RectTransform root;                  
    public CanvasGroup canvasGroup;            
    public Image icon;
    public TextMeshProUGUI label;

    [Header("Anim")]
    public float appearScale = 1f;
    public float appearTime = 0.15f;
    public float holdTime = 0.35f;
    public float floatUpDistance = 80f;
    public float fadeOutTime = 0.35f;

    [Header("Offset")]
    public Vector3 worldOffset = new Vector3(0, 1.5f, 0); 

    void Reset()
    {
        root = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    public void ShowAtWorld(Vector3 worldPos, Sprite sprite, string text, Camera cam = null)
    {
        if (!targetCanvas)
        {
            var qte = UI_QTE.Instance;
            if (qte && qte.panel)
                targetCanvas = qte.panel.GetComponentInParent<Canvas>();
            if (!targetCanvas)
                targetCanvas = FindFirstObjectByType<Canvas>();
        }

        if (!root) root = GetComponent<RectTransform>();
        if (!canvasGroup) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (icon) icon.sprite = sprite;
        if (label) label.text = text;
        if (cam == null) cam = Camera.main;
        
        var worldOffset = new Vector3(0, 1.2f, 0);
        worldPos += worldOffset;

        var canvasRect = targetCanvas.transform as RectTransform;
        
        if (targetCanvas.renderMode == RenderMode.WorldSpace)
        {
            root.SetParent(canvasRect, worldPositionStays: false);
            root.position = worldPos;
            root.rotation = Quaternion.identity;
            root.localScale = Vector3.one * 0.001f; 
            root.anchoredPosition3D = root.localPosition; 
        }
        else
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, worldPos);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
                out Vector2 localPoint
            );

            root.SetParent(canvasRect, worldPositionStays: false);
            root.anchoredPosition = localPoint;
            root.localScale = Vector3.one;
            root.rotation = Quaternion.identity;
        }
        if (icon != null)
        {
            if (sprite == null)
            {
                icon.enabled = false;
            }
            else
            {
                icon.enabled = true;
                icon.sprite = sprite;
            }
        }

        if (label != null) label.text = text ?? string.Empty;
        StartCoroutine(PlayAnim());
    }


    private IEnumerator PlayAnim()
    {
        canvasGroup.alpha = 0f;
        
        float t = 0f;
        while (t < appearTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / appearTime);
            root.localScale = Vector3.one * Mathf.Lerp(0f, appearScale, k);
            canvasGroup.alpha = k;
            yield return null;
        }
        root.localScale = Vector3.one * appearScale;
        canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(holdTime);
        
        t = 0f;
        Vector2 startPos = root.anchoredPosition;
        Vector2 endPos = startPos + new Vector2(0f, floatUpDistance);
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / fadeOutTime);
            root.anchoredPosition = Vector2.Lerp(startPos, endPos, k);
            canvasGroup.alpha = 1f - k;
            yield return null;
        }

        Destroy(gameObject);
    }
}
