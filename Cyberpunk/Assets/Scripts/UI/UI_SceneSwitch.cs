using UnityEngine;

public class UI_SceneSwitch : MonoBehaviour
{
    [SerializeField] private AudioClip buttonClickSound;
    private AudioSource audioSource;
    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void GoToMainMenu()
    {
        PlayClickSound();
        GameManager.Instance.GoToMainMenu();
    }
    public void GoToMainBefore()
    {
        PlayClickSound();
        GameManager.Instance.HasPlayedPuzzle = false;
        GameManager.Instance.GoToMainSceneBefore();
    }
    
    public void GoToMainAfter()
    {
        PlayClickSound();
        GameManager.Instance.GoToMainSceneAfter();
    }

    public void GoToPuzzle()
    {
        PlayClickSound();
        GameManager.Instance.GoToPuzzleScene();
    }

    public void GoToCollect()
    {
        PlayClickSound();
        GameManager.Instance.GoToCollection();
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
    private void PlayClickSound()
    {
        if (buttonClickSound != null)
            audioSource.PlayOneShot(buttonClickSound);
    }

}
