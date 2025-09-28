using UnityEngine;
using UnityEngine.SceneManagement;


public class UI_MainMenu : MonoBehaviour
{
    public void PlayButton()
    {
        GameManager.Instance.GoToMainScene();
    }
    
    public void QuitButton()
    {
        Application.Quit();
    }
}
