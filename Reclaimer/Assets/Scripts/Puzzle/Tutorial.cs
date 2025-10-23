using UnityEngine;
using TMPro;
using System.Collections;

public class Tutorial : MonoBehaviour
{
    public TextMeshProUGUI text;            // ���� Inspector ָ������Ϊ�գ��ű����Զ�������������� TMP
    public float displayTime = 5f;
    public CanvasGroup canvasGroup;         // ���� Inspector ָ������Ϊ�ջ��Զ���ȡ�򴴽�

    [Header("Auto Start")]
    [TextArea] public string startMessage = "Welcome!";
    public bool showOnStart = true;

    private Coroutine currentRoutine;
    private bool isShowing = false;

    // ����������ã��������� canMove
    private Player cachedPlayer;

    void Awake()
    {
        // �Զ����� TextMeshProUGUI�����������ϣ�
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>(true);

        // �Զ���ȡ�򴴽� CanvasGroup��ͨ����ͬһ�����ϣ�
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        // ��ʼ����
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void Start()
    {
        // ���Ի��� Player���� Start ʱ Player ��δ Awake��Ҳ���� Show ʱ�ٴβ��ң�
        cachedPlayer = FindObjectOfType<Player>();

        if (showOnStart)
            Show(startMessage);
    }

    public void Show(string msg)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        // ��� text ���� null���ٴγ��Բ��ң��Է���������ʱ�Ŵ����Ӷ���
        if (text == null)
            text = GetComponentInChildren<TextMeshProUGUI>(true);

        StopAllCoroutines();
        if (text != null) text.text = msg;
        currentRoutine = StartCoroutine(ShowAndHide());
    }

    private IEnumerator ShowAndHide()
    {
        isShowing = true;

        // ��ֹ����ƶ������ҵ� player��
        if (cachedPlayer == null)
            cachedPlayer = FindObjectOfType<Player>();
        if (cachedPlayer != null)
            cachedPlayer.canMove = false;

        // ���ý����赲����ֹ��ҵ��������
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        // ����
        float t = 0f;
        float fadeIn = 0.25f;
        while (t < fadeIn)
        {
            t += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeIn);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        // ? ��ʾ��ʱ��ֻ�ܰ��� E �� �Źر�
        float timer = 0f;
        while (timer < displayTime && isShowing)
        {
            if (Input.GetMouseButtonDown(1)||Input.GetMouseButtonDown(0)) // �� �޸Ĵ�
                break;
            timer += Time.deltaTime;
            yield return null;
        }

        // ����
        t = 0f;
        float fadeOut = 0.25f;
        while (t < fadeOut)
        {
            t += Time.deltaTime;
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOut);
            yield return null;
        }

        if (canvasGroup != null) canvasGroup.alpha = 0f;
        // ���� UI
        gameObject.SetActive(false);
        isShowing = false;

        // �ָ�����ƶ�
        if (cachedPlayer == null)
            cachedPlayer = FindObjectOfType<Player>();
        if (cachedPlayer != null)
            cachedPlayer.canMove = true;

        // �ָ������赲
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }

    /// <summary>
    /// �ⲿ���Ե���ȡ����ǰ��ʾ���ָ���ҿ���
    /// </summary>
    public void AbortAndHide()
    {
        StopAllCoroutines();
        isShowing = false;
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        gameObject.SetActive(false);

        if (cachedPlayer == null)
            cachedPlayer = FindObjectOfType<Player>();
        if (cachedPlayer != null)
            cachedPlayer.canMove = true;
    }
}
