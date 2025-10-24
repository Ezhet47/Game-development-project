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
    [SerializeField] private string boolName = "isTouch"; 

    [Header("¿ÉÑ¡")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] barkClips;
    [SerializeField] private HeartFloat heartFloat;


    public bool isTouch { get; private set; }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isTouch) return;
        isTouch = true;

        if (animator)
            animator.SetBool(boolName, true); 

        if (audioSource && barkClips != null && barkClips.Length > 0)
            audioSource.PlayOneShot(barkClips[Random.Range(0, barkClips.Length)]);

        if (heartFloat)
            heartFloat.Play();
    }


    public void OnAnimComplete()
    {
        isTouch = false; 
        if (animator)
            animator.SetBool(boolName, false); 
    }
}






