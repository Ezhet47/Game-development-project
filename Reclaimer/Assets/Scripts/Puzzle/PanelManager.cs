using UnityEngine;
using System.Collections;

public class PanelManager : MonoBehaviour
{
    public GameObject bg;
    public GameObject lockedBG;
    public GameObject panel;       
    public GameObject panelTest;

    [SerializeField] private GameObject tutorialImage;
    [SerializeField] private float tutorialDuration = 5f;
    private bool tutorialShown = false;

    private void Awake()
    {
        SetAllInactive();           
    }

    private void Start()
    {
        UpdateSceneState();
    }

    public void UpdateSceneState()
    {
        SetAllInactive();         

        bool hasCollected = GameManager.Instance.HasCollected;
        bool hasPlayed = GameManager.Instance.HasPlayedPuzzle;

        var mgr = PipeManager.Instance;
        bool level1Done = mgr && mgr.IsLevel1Completed();
        bool level2Done = mgr && mgr.IsLevel2Completed();

        //Debug.Log($"[PanelManager] hasCollected={hasCollected}, hasPlayed={hasPlayed}, L1={level1Done}, L2={level2Done}");

        if (!hasCollected) { ShowLocked(); return; }
        if (!hasPlayed) { ShowPanel(); return; }
        if (!level1Done || !level2Done) { ShowBG(); return; }

        //Debug.Log("[PanelManager] ¡ú ShowPanelTest()");
        ShowPanelTest();
    }

    private void SetAllInactive()
    {
        if (bg) bg.SetActive(false);
        if (lockedBG) lockedBG.SetActive(false);
        if (panel) panel.SetActive(false);
        if (panelTest) panelTest.SetActive(false);
    }

    public void ShowLocked()
    {
        lockedBG.SetActive(true);
        bg.SetActive(false);
        panel.SetActive(false);
        panelTest.SetActive(false);
        //Debug.Log("Display Locked Puzzle BG");
        var popup = FindFirstObjectByType<TutorialPopup>();
        if (popup != null)
        {
            //Debug.Log("Popup triggered");
            popup.Show(
                "Not yet collected.\n" +
                "Please collect the repair items first."
            );
        }
    }

    public void ShowBG()
    {
        lockedBG.SetActive(false);
        bg.SetActive(true);
        panel.SetActive(false);
        panelTest.SetActive(false);
        //Debug.Log("Display BG");
        if (!tutorialShown && tutorialImage != null)
        {
            tutorialShown = true;
            StartCoroutine(ShowTutorial());
        }
    }

    public void ShowPanel()
    {
        lockedBG.SetActive(false);
        bg.SetActive(false);
        panel.SetActive(true);
        panelTest.SetActive(false);
        //Debug.Log("Display Panel (Diagnosis)");
        var popup = FindFirstObjectByType<TutorialPopup>();
        if (popup != null)
        {
            //Debug.Log("Popup triggered");
            popup.Show(
                "Right-click and drag the scanner.\n" +
                "Scan the cyber arm to locate the faulty part.\n" +
                "Right-click and drag the screwdriver.\n" +
                "Align its tip with a screw to remove it."
            );
        }
    }

    public void ShowPanelTest()
    {
        lockedBG.SetActive(false);
        bg.SetActive(false);
        panel.SetActive(false);
        panelTest.SetActive(true);
        //Debug.Log("Display Panel_Test (Verification)");

        StartCoroutine(ReturnToMainAfterDelay(5f));
    }

    public void ReturnToMain()
    {
        GameManager.Instance.HasPlayedPuzzle = false;
        GameManager.Instance.GoToMainSceneBefore();
    }

    private IEnumerator ReturnToMainAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        //GameManager.Instance.HasPlayedPuzzle = false;
        GameManager.Instance.GoToMainSceneAfter();
    }

    private IEnumerator ShowTutorial()
    {
        tutorialImage.SetActive(true);

        CanvasGroup cg = tutorialImage.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = tutorialImage.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / 0.3f);
            yield return null;
        }
        cg.alpha = 1f;

        yield return new WaitUntil(() => Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1));

        t = 0f;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / 0.3f);
            yield return null;
        }
        cg.alpha = 0f;

        tutorialImage.SetActive(false);
    }


}
