using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Teleport : MonoBehaviour
{
    [Header("Blackout")]
    public Image blackScreen;             
    public float fadeDuration = 1f;      

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
            StartCoroutine(FadeOutAndReturnToPanel());
        }
    }

    private IEnumerator FadeOutAndReturnToPanel()
    {
        float t = 0f;
        Color c = blackScreen.color;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Clamp01(t / fadeDuration);
            blackScreen.color = c;
            yield return null;
        }
        GameManager.Instance.PuzzleCompleted = false;
        GameManager.Instance.HasPlayedPuzzle = false;
        GameManager.Instance.GoToMainScene();
    }
}