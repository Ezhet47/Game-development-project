/*using UnityEngine;
using UnityEngine.EventSystems;

public class AnimatorBoolHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Animator animator;
    [SerializeField] private string boolParameter;

    private void Awake()
    {
        if (animator) 
            animator.SetBool(boolParameter, false); 
    }

    private void OnDisable()
    {
        if (animator) 
            animator.SetBool(boolParameter, false); 
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animator) 
            animator.SetBool(boolParameter, true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (animator) 
            animator.SetBool(boolParameter, false);
    }
}
*/
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimatorBoolHandler : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Animator animator;
    [SerializeField] private string boolName = "isTouch"; // Animator����ƶ�����Bool����

    [Header("��ѡ")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] barkClips;
    [SerializeField] private HeartFloat heartFloat;

    // ��¼�Ƿ��ڲ���
    public bool isTouch { get; private set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isTouch) return; // �����в��ظ�����
        isTouch = true;

        if (animator)
            animator.SetBool(boolName, true); // ��ʼ����

        if (audioSource && barkClips != null && barkClips.Length > 0)
            audioSource.PlayOneShot(barkClips[Random.Range(0, barkClips.Length)]);

        if (heartFloat)
            heartFloat.Play();
    }

    // ?? �ڶ���ĩβ�� Animation Event �����������
    public void OnAnimComplete()
    {
        isTouch = false; // ��λ�ű�״̬
        if (animator)
            animator.SetBool(boolName, false); // ? ��λ Animator ��Idle
    }
}






