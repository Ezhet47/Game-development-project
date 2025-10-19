using UnityEngine;

public class UI_SceneSwitch : MonoBehaviour
{
    public void GoToMainMenu()
    {
        GameManager.Instance.GoToMainMenu();
    }
    public void GoToMain()
    {
        GameManager.Instance.PuzzleCompleted = false;
        GameManager.Instance.HasPlayedPuzzle = false;
        GameManager.Instance.GoToMainScene();
    }

    public void GoToPuzzle()
    {
        GameManager.Instance.GoToPuzzleScene();
    }

    public void GoToCollect()
    {
        GameManager.Instance.GoToCollection();
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
