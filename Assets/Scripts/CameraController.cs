using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _camera;

    private Vector3 _defaultPosition;
    private float _defaultSize;

    private bool _isZoomed = false;

    private void Awake()
    {
        _camera = GetComponent<Camera>();

        _defaultPosition = transform.position;
        _defaultSize = _camera.orthographicSize;
    }

    private void Update()
    {
        if (_isZoomed && Input.GetMouseButtonDown(1))
        {
            ResetCamera();
        }
    }

    public void ZoomTo(Vector3 targetPosition, float zoomSize)
    {
        transform.position = new Vector3(
            targetPosition.x,
            targetPosition.y,
            transform.position.z
        );

        _camera.orthographicSize = zoomSize;
        _isZoomed = true;
    }

    public void ResetCamera()
    {
        transform.position = _defaultPosition;
        _camera.orthographicSize = _defaultSize;
        _isZoomed = false;
    }
}