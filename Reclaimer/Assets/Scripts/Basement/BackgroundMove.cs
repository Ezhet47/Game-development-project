using UnityEngine;

public class BackgroundMove : MonoBehaviour
{
    [SerializeField] private float parallaxFactor = 0.5f; // �ƶ��ٶ������������ı���
    private Transform cam;
    private Vector3 lastCamPos;

    private void Start()
    {
        cam = Camera.main.transform;
        lastCamPos = cam.position;
    }

    private void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;
        // X�����ƶ���Z���򱣳ֲ���
        transform.position += new Vector3(delta.x * parallaxFactor, delta.y * parallaxFactor, 0);
        lastCamPos = cam.position;
    }
}