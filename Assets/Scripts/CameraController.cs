using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameplayUIController _gameplayUI;

    private Camera _camera;
    private Vector3 _defaultPosition;
    private float _defaultSize;
    private float _zoomSize;
    private float _lastAspect;
    private const float ReferenceAspect = 16f / 9f;

    public bool IsZoomed { get; private set; }

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _defaultPosition = transform.position;
        _defaultSize = _camera.orthographicSize;
    }

    public void ZoomTo(Vector3 targetPosition, float zoomSize)
    {
        transform.position = new Vector3(
            targetPosition.x,
            targetPosition.y,
            transform.position.z
        );

        _zoomSize = zoomSize;
        FitZoomToAspect();
        IsZoomed = true;
        _gameplayUI.RefreshVisibility();
    }

    public void ResetCamera()
    {
        transform.position = _defaultPosition;
        _camera.orthographicSize = _defaultSize;
        IsZoomed = false;

        _gameplayUI.RefreshVisibility();
    }

    private void LateUpdate()
    {
        if (IsZoomed && !Mathf.Approximately(_lastAspect, _camera.aspect)) FitZoomToAspect();
    }

    private void FitZoomToAspect()
    {
        _lastAspect = _camera.aspect;
        // Keep the authored shelf width visible when the viewport is narrower than 16:9.
        _camera.orthographicSize = _zoomSize * Mathf.Max(1, ReferenceAspect / Mathf.Max(.1f, _lastAspect));
    }
}
