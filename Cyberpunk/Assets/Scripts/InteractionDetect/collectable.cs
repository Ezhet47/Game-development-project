using UnityEngine;

public class collectable : MonoBehaviour
{
    private int score = 1;
    private bool collide = true;
    private bool playerInRange = false;
    public InteractionDetect otherScript;

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
            otherScript.canpress = false;
            componentCount.Instance.totalComponents += score;
            componentCount.Instance.UpdateTotalScore();
            Destroy(gameObject);
        }
    }
}