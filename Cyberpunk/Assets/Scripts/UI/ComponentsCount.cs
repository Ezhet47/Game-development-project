using UnityEngine;
using TMPro;

public class componentCount : MonoBehaviour
{
    public int totalComponents;
    public TextMeshProUGUI componentText;
    public static componentCount Instance;
    void Start()
    {
        Instance = this;
        UpdateTotalScore();
    }

    public void UpdateTotalScore()
    {
        this.componentText.text = totalComponents.ToString("00");
    }
}
