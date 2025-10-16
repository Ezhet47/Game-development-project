using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public GameObject bg;
    public GameObject lockedBG;
    public GameObject panel;       
    public GameObject panelTest;  

    private void Start()
    {
        UpdateSceneState();
    }

    public void UpdateSceneState()
    {
        bool hasCollected = GameManager.Instance.HasCollected;
        bool hasPlayed = GameManager.Instance.HasPlayedPuzzle;
        bool completed = GameManager.Instance.PuzzleCompleted;

        if (!hasCollected) { ShowLocked(); return; }

        if (!hasPlayed)
        {
            ShowPanel();
        }
        else if (!completed)
        {
            ShowBG();
        }
        else
        {
            ShowPanelTest();
            GameManager.Instance.PuzzleCompleted = false;
        }
    }

    public void ShowLocked()
    {
        lockedBG.SetActive(true);
        bg.SetActive(false);
        panel.SetActive(false);
        panelTest.SetActive(false);
        //Debug.Log("Display Locked Puzzle BG");
    }

    public void ShowBG()
    {
        lockedBG.SetActive(false);
        bg.SetActive(true);
        panel.SetActive(false);
        panelTest.SetActive(false);
        //Debug.Log("Display BG");
    }

    public void ShowPanel()
    {
        lockedBG.SetActive(false);
        bg.SetActive(false);
        panel.SetActive(true);
        panelTest.SetActive(false);
        //Debug.Log("Display Panel (Diagnosis)");
    }

    public void ShowPanelTest()
    {
        lockedBG.SetActive(false);
        bg.SetActive(false);
        panel.SetActive(false);
        panelTest.SetActive(true);
        //Debug.Log("Display Panel_Test (Verification)");
    }

    public void ReturnToMain()
    {
        GameManager.Instance.PuzzleCompleted = false;
        GameManager.Instance.HasPlayedPuzzle = false;
        GameManager.Instance.GoToMainScene();
    }
}
