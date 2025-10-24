using UnityEngine;

public class DogSFX : MonoBehaviour
{
    [SerializeField] private AnimatorBoolHandler boolHandler; 
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] barkClips;           
    [SerializeField] private HeartFloat heartFloat;
    
    public void PlayBarkAndHeart()
    {
        if (audioSource != null && barkClips != null && barkClips.Length > 0)
        {
            var clip = barkClips[Random.Range(0, barkClips.Length)];
            audioSource.PlayOneShot(clip);
        }

        if (heartFloat != null)
            heartFloat.Play();
    }
    
    public void OnDogAnimComplete()
    {
        if (boolHandler != null)
            boolHandler.ResetFalse();
    }
}