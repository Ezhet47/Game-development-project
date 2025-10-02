using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    private void Start()
    {
        var fadeScreen = FindFadeScreenUI();
        if (fadeScreen != null) fadeScreen.DoFadeIn();
    }

    private const string MainScene = "MainScene";
    private const string PuzzleScene = "PuzzleScene";
    private const string Collection = "Collection";

    public bool PuzzleCompleted = false;
    public bool HasPlayedPuzzle = false;

    public void GoToPuzzleScene() => StartCoroutine(LoadSceneWithFade(PuzzleScene));
    public void GoToMainScene()   => StartCoroutine(LoadSceneWithFade(MainScene));
    public void GoToCollection()  => StartCoroutine(LoadSceneWithFade(Collection));

    private UI_FadeScreen FindFadeScreenUI()
    {
        return FindFirstObjectByType<UI_FadeScreen>();
    }

    private IEnumerator LoadSceneWithFade(string sceneName)
    {
        var fadeScreen = FindFadeScreenUI();
        if (fadeScreen != null)
        {
            fadeScreen.DoFadeOut();
            yield return fadeScreen.fadeEffectCo;
        }

        SceneManager.LoadScene(sceneName);
        yield return null;

        fadeScreen = FindFadeScreenUI();
        if (fadeScreen != null)
        {
            fadeScreen.DoFadeIn();
        }
    }
}