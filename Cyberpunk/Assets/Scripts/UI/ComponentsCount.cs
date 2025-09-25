using UnityEngine;
using TMPro;

public class ComponentsCount : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public int totalComponents;
    public TextMeshProUGUI ComponentsText;
    public static ComponentsCount Instance;
    void Start()
    {
        Instance = this;
    }

    public void UpdateTotalScore()
    {
        this.ComponentsText.text = totalComponents.ToString();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
