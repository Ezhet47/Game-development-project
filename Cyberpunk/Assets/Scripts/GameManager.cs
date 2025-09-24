using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private const string MainScene = "MainScene";
    private const string PuzzleScene = "PuzzleScene";
    private const string Collection = "Collection";

    public bool PuzzleCompleted = false;
    public bool HasPlayedPuzzle = false;

    public void GoToPuzzleScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(PuzzleScene);
    }

    public void GoToMainScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(MainScene);
    }

    public void GoToCollection()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(Collection);
    }
}
