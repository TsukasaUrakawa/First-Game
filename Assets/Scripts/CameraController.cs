using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Camera _camera;

    private Vector3 _defaultPosition;
    private float _defaultSize;

    public bool IsZoomed { get; private set; }

    private bool _canResetByRightClick = true;

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

        // 拡大に使った右クリックで、即リセットされないようにする
        _canResetByRightClick = false;
    }

    public void ResetCamera()
    {
        transform.position = _defaultPosition;
        _camera.orthographicSize = _defaultSize;
        IsZoomed = false;
    }
}