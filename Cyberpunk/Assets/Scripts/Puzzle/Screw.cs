using UnityEngine;
using UnityEngine.EventSystems;

public class Screw : MonoBehaviour
{
    private bool removed = false;
    private float rotationSpeed = 400f; 
    //private float currentAngle = 0f;
    private float descendDuration = 0.5f; 

    public void StartUnscrew()
    {
        if (removed) return;
        StartCoroutine(RemoveScrew());
    }


    private System.Collections.IEnumerator RemoveScrew()
    {
        removed = true;
        //Debug.Log($"{name} Starts being removed");

        float duration = 0.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float deltaAngle = rotationSpeed * Time.deltaTime;
            transform.Rotate(0, 0, -deltaAngle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Vector3 startPos = transform.localPosition;
        Vector3 endPos = startPos + new Vector3(0, -25f, 0);
        elapsed = 0f;

        while (elapsed < descendDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / descendDuration);
            transform.localPosition = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        gameObject.SetActive(false);

        PipeManager.Instance.OnScrewRemoved();
    }
}
