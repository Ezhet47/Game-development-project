using UnityEngine;
using UnityEngine.UI;

public class GuideController : MonoBehaviour
{
    [SerializeField] private Button guideButton;
    [SerializeField] private GameObject guideImage;

    private bool isVisible = false;

    private void Start()
    {
        if (guideImage != null)
            guideImage.SetActive(false);

        if (guideButton != null)
            guideButton.onClick.AddListener(ToggleGuide);
    }

    private void Update()
    {
        if (isVisible && Input.GetMouseButtonDown(0))
        {
            //if (!RectTransformUtility.RectangleContainsScreenPoint(
            //    guideImage.GetComponent<RectTransform>(),
            //    Input.mousePosition))
            if (isVisible && Input.GetMouseButtonDown(0))
            {
                HideGuide();
            }
        }
    }

    public void ToggleGuide()
    {
        isVisible = !isVisible;
        guideImage.SetActive(isVisible);
    }

    private void HideGuide()
    {
        isVisible = false;
        guideImage.SetActive(false);
    }
}
