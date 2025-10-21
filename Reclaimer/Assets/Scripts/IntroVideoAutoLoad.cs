using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroVideoAutoLoad : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "MainMenu";
    private VideoPlayer vp;

    private void Awake()
    {
        vp = GetComponent<VideoPlayer>();
        
        vp.loopPointReached += OnVideoFinished;
        
        if (!vp.playOnAwake)
            vp.Play();
    }

    private void OnDestroy()
    {
        if (vp != null) vp.loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer source)
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
