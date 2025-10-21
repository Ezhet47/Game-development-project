using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("Canvas & UI")]
    public Canvas targetCanvas;              // ��� UI Canvas��Screen Space - Camera / Overlay / World Space ��֧�֣�
    public RectTransform bubbleRoot;         // ���������ڵ㣨RectTransform��
    public TextMeshProUGUI textLabel;        // �ı�
    public CanvasGroup canvasGroup;          // ���뵭�������ص������ѡ��

    [Header("Typing")]
    [Range(1, 120)] public float charsPerSecond = 25f;  // ���ֻ��ٶ�
    public bool clickToCompleteLine = true;             // �����а�E�Ƿ����̲���
    public Vector3 worldOffset = new Vector3(0f, 1.5f, 0f); // ͷ��ƫ�ƣ����絥λ��

    [Header("Input Guard")]
    [SerializeField] private float inputIgnoreDuration = 0.1f; // ��ʼ����������ʱ�����룩
    private float inputAllowedAt = 0f;

    [Header("E-Hint���� InteractionDetect һ�����ƣ�")]
    public GameObject eHintRoot;             // Сͼ�� + �İ� ������������ bubbleRoot �£�
    public Image eHintIcon;                  // ��İ���������E ��ͼ��
    public TextMeshProUGUI eHintLabel;       // ��ʾ�İ������� E ���� / ��һҳ / ������
    public bool canPress = false;            // ������������ InteractionDetect.canpress��
    public bool followPlayerFlipX = false;   // ����������ҷ�ת����ѡ��

    // ����ʱ
    private List<string> lines = new List<string>();
    private int index = -1;
    private Coroutine typingRoutine;
    private bool isShowing = false;
    private bool isTyping = false;

    private Transform followTarget;          // ���ݸ���˭����һ�NPC��
    private Transform playerTransForFlip;    // �������ҷ�ת
    private Player cachedPlayer;             // ��/�����ƶ�
    private Camera cam;

    // ���������㴥�������ã�
    public static DialogueUI Instance { get; private set; }
    public bool IsShowing => isShowing;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        if (!bubbleRoot) bubbleRoot = GetComponent<RectTransform>();
        cam = Camera.main;

        HideImmediate();
    }

    void LateUpdate()
    {
        if (!isShowing || followTarget == null || targetCanvas == null || bubbleRoot == null)
        {
            ApplyEHintActive();
            return;
        }

        // ���� �ѡ�����λ��(ͷ��ƫ��)��ת���� Canvas ���겢��λ���� ����
        var worldPos = followTarget.position + worldOffset;

        if (targetCanvas.renderMode == RenderMode.WorldSpace)
        {
            bubbleRoot.position = worldPos;
            bubbleRoot.rotation = Quaternion.identity;
        }
        else
        {
            // ScreenSpace-Overlay / ScreenSpace-Camera
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(cam ? cam : Camera.main, worldPos);
            RectTransform canvasRect = targetCanvas.transform as RectTransform;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screen,
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
                out Vector2 local
            );

            // �踸������
            if (bubbleRoot.parent != canvasRect)
                bubbleRoot.SetParent(canvasRect, worldPositionStays: false);

            bubbleRoot.anchoredPosition = local;
            bubbleRoot.localRotation = Quaternion.identity;
        }

        // ���� ���루E���������� �� ��ҳ ����
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (Time.time >= inputAllowedAt && Input.GetKeyDown(KeyCode.E))
            {
                if (isTyping && clickToCompleteLine) { CompleteTypingInstant(); }
                else { Next(); }
            }
        }

        // ���� E����ʾ�������뷭ת��ģ�� InteractionDetect ��� ����
        ApplyEHintActive();

        if (followPlayerFlipX && playerTransForFlip && eHintRoot)
        {
            var rt = eHintRoot.transform as RectTransform;
            if (rt)
            {
                Vector3 ls = rt.localScale;
                ls.x = Mathf.Sign(-playerTransForFlip.localScale.x) * Mathf.Abs(ls.x);
                rt.localScale = ls;
            }
        }
    }

    // ���� ��ʼ�Ի� ����
    public void StartDialogue(IEnumerable<string> content, Transform follow, Player player)
    {
        lines.Clear();
        lines.AddRange(content);
        followTarget = follow;
        cachedPlayer = player ? player : FindObjectOfType<Player>();
        index = -1;

        // ���ƶ�
        if (cachedPlayer) cachedPlayer.canMove = false;
        if (cachedPlayer) playerTransForFlip = cachedPlayer.transform;

        // ��ʾ���
        isShowing = true;
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        bubbleRoot.gameObject.SetActive(true);

        // E ����ʾ��������ʾ�����ڵ�һ�п�ʼǰ��ʾ����E���ꡱ
        canPress = true;
        UpdateEHintTypingState(true);
        ApplyEHintActive();

        Next(); // ���ŵ�һ��
        inputAllowedAt = Time.time + inputIgnoreDuration;
    }

    // ���� ��һҳ or ���� ����
    public void Next()
    {
        if (!isShowing) return;

        index++;
        if (index >= lines.Count)
        {
            EndDialogue();
            return;
        }

        if (typingRoutine != null) StopCoroutine(typingRoutine);
        typingRoutine = StartCoroutine(TypeLine(lines[index]));
    }

    // ���� ���ֻ� ����
    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        UpdateEHintTypingState(true);      // ���ڴ��֣���ʾ����E���ꡱ

        textLabel.text = "";
        float delay = 1f / Mathf.Max(1f, charsPerSecond);

        foreach (char c in line)
        {
            textLabel.text += c;
            yield return new WaitForSeconds(delay);
        }

        isTyping = false;
        typingRoutine = null;

        UpdateEHintTypingState(false);     // ���꣺��ʾ����E��һҳ/������
    }

    // ���� ���̲��굱ǰҳ ����
    private void CompleteTypingInstant()
    {
        if (!isTyping) return;
        if (typingRoutine != null) StopCoroutine(typingRoutine);

        textLabel.text = lines[index];
        isTyping = false;
        typingRoutine = null;

        UpdateEHintTypingState(false);     // �������ʾ����E��һҳ/������
    }

    // ���� �����Ի��������ƶ��� ����
    public void EndDialogue()
    {
        isShowing = false;

        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        bubbleRoot.gameObject.SetActive(false);

        if (cachedPlayer) cachedPlayer.canMove = true;

        // ��β
        lines.Clear();
        index = -1;
        followTarget = null;
        playerTransForFlip = null;
        cachedPlayer = null;

        // ���� E ��ʾ
        canPress = false;
        ApplyEHintActive();
    }

    private void HideImmediate()
    {
        if (bubbleRoot) bubbleRoot.gameObject.SetActive(false);
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        canPress = false;
        ApplyEHintActive();
    }

    // ���� E ��ʾ���� & �İ� ���� 
    private void ApplyEHintActive()
    {
        if (eHintRoot) eHintRoot.SetActive(canPress);
    }

    private void UpdateEHintTypingState(bool typing)
    {
        if (!eHintLabel) return;

        if (typing)
        {
            eHintLabel.text = "�� E ����";
        }
        else
        {
            bool isLast = (index >= 0 && index == lines.Count - 1);
            eHintLabel.text = isLast ? "�� E ����" : "�� E ��һҳ";
        }
    }
}
