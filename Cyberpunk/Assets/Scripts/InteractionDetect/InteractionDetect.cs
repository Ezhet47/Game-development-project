using UnityEngine;

public class InteractionDetect : MonoBehaviour
{
    public GameObject buttonSprite;
    public Transform playerTrans;
    public bool canpress;

    private void Update()
    {
        buttonSprite.SetActive(canpress);

        if (canpress)
        {

            
            buttonSprite.transform.localScale = -playerTrans.localScale;

        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Interactable"))
        {
            canpress = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        canpress = false;
    }
}