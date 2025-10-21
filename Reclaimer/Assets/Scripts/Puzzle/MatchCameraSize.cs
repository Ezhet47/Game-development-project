using UnityEngine;

[ExecuteAlways]
public class MatchCameraSize : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;

        float height = cam.orthographicSize * 2f;
        float width = height * cam.aspect;
        var sprite = GetComponent<SpriteRenderer>().sprite;
        if (sprite == null) return;

        Vector2 size = sprite.bounds.size;
        transform.localScale = new Vector3(width / size.x, height / size.y, 1);
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
    }
}
