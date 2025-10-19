using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private Camera mainCamera;
    private float lastCameraPositionX;
    private float cameraHalfWidth;

    [SerializeField] private ParallaxLayer[] backgroundLayers;

    private void Awake()
    {
        mainCamera = Camera.main;
        UpdateCameraWidth();    
        InitializeLayers();
    }

    private void FixedUpdate()
    {
     
        UpdateCameraWidth();

        float currentCameraPositionX = mainCamera.transform.position.x;
        float distanceToMove = currentCameraPositionX - lastCameraPositionX;
        lastCameraPositionX = currentCameraPositionX;

        float cameraLeftEdge = currentCameraPositionX - cameraHalfWidth;
        float cameraRightEdge = currentCameraPositionX + cameraHalfWidth;

        foreach (ParallaxLayer layer in backgroundLayers)
        {
            layer.Move(distanceToMove);
            layer.LoopBackground(cameraLeftEdge, cameraRightEdge);
        }
    }

    private void InitializeLayers()
    {
        foreach (ParallaxLayer layer in backgroundLayers)
            layer.CalculateImageWidth();
    }

    private void UpdateCameraWidth()
    {
        if (mainCamera == null) return;

        float currentHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;
        if (!Mathf.Approximately(currentHalfWidth, cameraHalfWidth))
        {
            cameraHalfWidth = currentHalfWidth;
        }
    }
}
