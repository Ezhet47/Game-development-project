using UnityEngine;

public class DoorSFX : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] AudioClip openClip;   
    [SerializeField] AudioClip closeClip;  

    void Reset() { if (!source) source = GetComponent<AudioSource>(); }
    
    public void PlayOpenSFX()  { if (source && openClip)  source.PlayOneShot(openClip); }
    public void PlayCloseSFX() { if (source && closeClip) source.PlayOneShot(closeClip); }
}