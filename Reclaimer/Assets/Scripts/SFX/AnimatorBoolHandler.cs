using UnityEngine;
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
    
    public void Set(bool value)
    {
        if (animator)
            animator.SetBool(boolParameter, value);
    }

    public void ResetFalse() => Set(false);
}
