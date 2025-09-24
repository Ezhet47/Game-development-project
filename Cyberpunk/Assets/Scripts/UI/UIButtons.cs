using UnityEngine;

public class UIButtons : MonoBehaviour
{
    public void GoToMain()
    {
        GameManager.Instance.PuzzleCompleted = false;
        GameManager.Instance.HasPlayedPuzzle = false;
        GameManager.Instance.GoToMainScene();
    }

    public void ReturnToPanel()
    {
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
}
