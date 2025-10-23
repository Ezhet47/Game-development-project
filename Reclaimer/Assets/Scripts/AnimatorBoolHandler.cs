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
    [SerializeField] private string boolName = "isTouch"; // Animator里控制动画的Bool参数

    [Header("可选")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] barkClips;
    [SerializeField] private HeartFloat heartFloat;

    // 记录是否在播放
    public bool isTouch { get; private set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isTouch) return; // 播放中不重复触发
        isTouch = true;

        if (animator)
            animator.SetBool(boolName, true); // 开始播放

        if (audioSource && barkClips != null && barkClips.Length > 0)
            audioSource.PlayOneShot(barkClips[Random.Range(0, barkClips.Length)]);

        if (heartFloat)
            heartFloat.Play();
    }

    // ?? 在动画末尾加 Animation Event 调用这个函数
    public void OnAnimComplete()
    {
        isTouch = false; // 复位脚本状态
        if (animator)
            animator.SetBool(boolName, false); // ? 复位 Animator 回Idle
    }
}






