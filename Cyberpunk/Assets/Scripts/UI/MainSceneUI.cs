using UnityEngine;

public class MainSceneUI : MonoBehaviour
{
    public GameObject bg;
    public GameObject chipPanel;
    public GameObject victoryPanel;

    private void Start()
    {
        if (GameManager.Instance.PuzzleCompleted)
        {
            bg.SetActive(false);
            chipPanel.SetActive(false);
            victoryPanel.SetActive(true);

            GameManager.Instance.PuzzleCompleted = false;
        }
        else if (GameManager.Instance.HasPlayedPuzzle)
        {
            bg.SetActive(false);
            chipPanel.SetActive(true);
            victoryPanel.SetActive(false);
        }
        else
        {
            bg.SetActive(true);
            chipPanel.SetActive(false);
            victoryPanel.SetActive(false);
        }

        GameManager.Instance.HasPlayedPuzzle = true;
    }

    public void OpenChipPanel()
    {
        chipPanel.SetActive(true);
        bg.SetActive(false);
    }

    public void CloseChipPanel()
    {
        chipPanel.SetActive(false);
        bg.SetActive(true);
    }
}
