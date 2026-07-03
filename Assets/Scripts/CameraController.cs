using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    private Camera _camera;
    private Vector3 _defaultPosition;
    private float _defaultSize;
    private bool _canResetByRightClick = true;

    public bool IsZoomed { get; private set; }

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _defaultPosition = transform.position;
        _defaultSize = _camera.orthographicSize;
    }

    private void Update()
    {
        if (!Input.GetMouseButton(1))
        {
            _canResetByRightClick = true;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (IsZoomed && _canResetByRightClick && Input.GetMouseButtonDown(1))
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
        IsZoomed = true;
        _canResetByRightClick = false;
    }

    public void ResetCamera()
    {
        transform.position = _defaultPosition;
        _camera.orthographicSize = _defaultSize;
        IsZoomed = false;
    }
}
