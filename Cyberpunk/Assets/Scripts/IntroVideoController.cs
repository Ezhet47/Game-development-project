// IntroVideoAutoLoader.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class IntroVideoController : MonoBehaviour
{
    [SerializeField] private string nextSceneName = "MainMenu"; // 主界面场景名
    [SerializeField] private VideoPlayer videoPlayer;           // 可不拖，脚本会自动寻找

    private bool loading;

    private void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = FindObjectOfType<VideoPlayer>();
        
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    private void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        if (loading) return;
        loading = true;
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}
