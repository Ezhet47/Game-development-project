using UnityEngine;
using TMPro;

public class ComponentCount : MonoBehaviour
{
    public int totalComponents;
    public TextMeshProUGUI componentText;
    public static ComponentCount instance;
    void Start()
    {
        instance = this;
        UpdateTotalScore();
    }

    public void UpdateTotalScore()
    {
        this.componentText.text = totalComponents.ToString("00");
    }
}
