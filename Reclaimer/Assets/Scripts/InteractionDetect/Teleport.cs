using UnityEngine;
using TMPro;
using System.Collections;

public class Teleport : MonoBehaviour
{
    [Header("Interaction")]
    public InteractionDetect otherScript;
    private bool collide = true;
    private bool playerInRange = false;

    [Header("Requirement")]
    public int requiredTotal = 7;                 // ��Ҫ�������
    [Tooltip("����ʹ�� ComponentCount.instance.totalComponents������㲻�������������������ֶ����ǵ�ǰ������>=0 ��Ч����")]
    public int currentCountOverride = -1;         // ��ѡ���ǣ�-1 ��ʾ���ã�

    [Header("UI Hint")]
    public TextMeshProUGUI notEnoughText;         // ����Ҫ��7������� �ı�����ѡ��
    public CanvasGroup notEnoughGroup;            // ��ѡ���������뵭��
    public string notEnoughMessage = "��Ҫ��7�����";
    public float hintFade = 0.15f;                // ����/����ʱ��
    public float hintShowTime = 1.25f;            // ͣ��ʱ��

    // ������ϵͳ�ļ��ݣ�����Ի�ʱ���ý�����
    public bool IsInteractable
    {
        get
        {
            bool dialogueBusy = UI_CollectionDialogue.Instance != null && UI_CollectionDialogue.Instance.IsShowing;
            return playerInRange && collide && !dialogueBusy;
        }
    }

    private void Awake()
    {
        // ������ʾ UI����ʼ��Ϊ����
        if (notEnoughGroup == null && notEnoughText != null)
            notEnoughGroup = notEnoughText.GetComponent<CanvasGroup>();

        if (notEnoughGroup != null)
        {
            notEnoughGroup.alpha = 0f;
            notEnoughGroup.blocksRaycasts = false;
            notEnoughGroup.interactable = false;
        }
        if (notEnoughText != null && !string.IsNullOrEmpty(notEnoughMessage))
            notEnoughText.text = notEnoughMessage;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerInRange = true;
        // �Ƿ���� E ������ʾ����� InteractionDetect ���ƣ����ﲻǿ�Ƹ�Ԥ
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        playerInRange = false;
    }

    private void Update()
    {
        if (!(playerInRange && collide)) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            int cur = GetCurrentCollected();

            if (cur >= requiredTotal)
            {
                // ? ��꣺ִ�д���
                collide = false;
                if (otherScript) otherScript.canpress = false; // �ص� E ��ʾ����ѡ��

                GameManager.Instance.HasPlayedPuzzle = false;
                GameManager.Instance.GoToMainSceneBefore();
            }
            else
            {
                // ? δ��꣺��ʾ����Ҫ��7�������
                ShowNotEnoughHint();
            }
        }
    }

    private int GetCurrentCollected()
    {
        // 1) ���ֶ�������Ч��������
        if (currentCountOverride >= 0) return currentCountOverride;

        // 2) Ĭ�ϴ� ComponentCount ��ȡ������Ŀ��������
        if (ComponentCount.instance != null)
            return ComponentCount.instance.totalComponents;

        // 3) �Ҳ�����Դʱ������ 0�����ⱨ����
        return 0;
    }

    private void ShowNotEnoughHint()
    {
        if (notEnoughText == null && notEnoughGroup == null) return; // û�� UI �;�Ĭ����

        // ȷ���ı�����
        if (notEnoughText != null && !string.IsNullOrEmpty(notEnoughMessage))
            notEnoughText.text = notEnoughMessage;

        StopAllCoroutines();
        StartCoroutine(CoFlashHint());
    }

    private IEnumerator CoFlashHint()
    {
        if (notEnoughGroup != null)
        {
            notEnoughGroup.blocksRaycasts = true;
            notEnoughGroup.interactable = true;

            // ����
            float t = 0f;
            while (t < hintFade)
            {
                t += Time.deltaTime;
                notEnoughGroup.alpha = Mathf.Lerp(0f, 1f, t / hintFade);
                yield return null;
            }
            notEnoughGroup.alpha = 1f;

            // ͣ��
            yield return new WaitForSeconds(hintShowTime);

            // ����
            t = 0f;
            while (t < hintFade)
            {
                t += Time.deltaTime;
                notEnoughGroup.alpha = Mathf.Lerp(1f, 0f, t / hintFade);
                yield return null;
            }
            notEnoughGroup.alpha = 0f;
            notEnoughGroup.blocksRaycasts = false;
            notEnoughGroup.interactable = false;
        }
        else if (notEnoughText != null)
        {
            // û�� CanvasGroup ��ֱ������һС���
            notEnoughText.gameObject.SetActive(true);
            yield return new WaitForSeconds(hintShowTime);
            notEnoughText.gameObject.SetActive(false);
        }
    }
}
