using UnityEngine;

public class Teleport : MonoBehaviour
{
    [Header("Interaction")]
    public InteractionDetect otherScript;  
    private bool collide = true;
    private bool playerInRange = false;


    public bool IsInteractable
    {
        get
        {
          
            bool dialogueBusy = DialogueUI.Instance != null && DialogueUI.Instance.IsShowing;
            return playerInRange && collide && !dialogueBusy;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
       
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
          
        }
    }

    private void Update()
    {
        if (playerInRange && collide && Input.GetKeyDown(KeyCode.E))
        {
            collide = false;

            if (otherScript) otherScript.canpress = false;

            GameManager.Instance.HasPlayedPuzzle = false;
            GameManager.Instance.GoToMainSceneBefore();
        }
    }
}