using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private GameplayUIController _gameplayUI;

    private Camera _camera;
    private Vector3 _defaultPosition;
    private float _defaultSize;

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

        _camera.orthographicSize = zoomSize;
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
}
