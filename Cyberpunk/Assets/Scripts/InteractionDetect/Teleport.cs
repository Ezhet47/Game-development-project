using UnityEngine;

public class Teleport : MonoBehaviour
{
    [Header("Interaction")]
    public InteractionDetect otherScript;
    private bool collide = true;
    private bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = true;
            otherScript.canpress = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            otherScript.canpress = false;
        }
    }

    private void Update()
    {
        if (playerInRange && collide && Input.GetKeyDown(KeyCode.E))
        {
            collide = false;
            otherScript.canpress = false;
            
            GameManager.Instance.PuzzleCompleted = false;
            GameManager.Instance.HasPlayedPuzzle = false;
            
            GameManager.Instance.GoToMainSceneBefore();
        }
    }
}