using UnityEngine;

/// <summary>Orthographic views show real 3D depth while keeping spine patterns easy to compare.</summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private GameplayUIController _gameplayUI;
    [SerializeField] private Transform _roomTarget;
    [SerializeField] private Vector2 _roomSize = new Vector2(28.5f, 16f);
    [SerializeField] private BoxOpenAndClose _roomBox;
    [SerializeField] private Collider _boxCollider;
    [SerializeField] private float _floorHeight = -5.15f;
    private Camera _camera;
    private float _zoomSize, _lastAspect;
    private Vector3 _zoomTarget, _boxAnchor;
    private float _boxCenterHeight;
    private Renderer[] _boxRenderers;
    private bool[] _boxRendererStates;
    public bool IsZoomed { get; private set; }
    private const float ReferenceAspect = 16f / 9f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _boxAnchor = _roomBox.transform.InverseTransformPoint(_boxCollider.bounds.center);
        _boxCenterHeight = _floorHeight + _boxCollider.bounds.extents.y;
        _boxRenderers = _roomBox.GetComponentsInChildren<Renderer>();
        _boxRendererStates = new bool[_boxRenderers.Length];
        for (int i = 0; i < _boxRenderers.Length; i++) _boxRendererStates[i] = _boxRenderers[i].enabled;
        FrameRoom();
    }

    public void ZoomTo(Vector3 targetPosition, float zoomSize)
    {
        _roomBox.CloseLid();
        _zoomTarget = targetPosition;
        _zoomSize = zoomSize;
        IsZoomed = true;
        FitZoomToAspect();
        SetBoxVisible(false);
        _gameplayUI.RefreshVisibility();
    }

    public void ResetCamera()
    {
        IsZoomed = false;
        FrameRoom();
        SetBoxVisible(true);
        _gameplayUI.RefreshVisibility();
    }

    private void LateUpdate()
    {
        if (Mathf.Approximately(_lastAspect, _camera.aspect)) return;
        if (IsZoomed) FitZoomToAspect(); else FrameRoom();
    }

    private void FrameRoom()
    {
        _lastAspect = _camera.aspect;
        var viewport = new Rect(.23f, .29f, .74f, .64f);
        float size = Mathf.Max(_roomSize.y / (2 * viewport.height), _roomSize.x / (2 * _lastAspect * viewport.width));
        Pose(_roomTarget.position, size, Quaternion.Euler(13, -5, 0), viewport.center);
        // Anchor the real box below the shelf selector, including after a window resize.
        var ray = _camera.ViewportPointToRay(new Vector3(.105f, .14f));
        if (new Plane(Vector3.up, new Vector3(0, _boxCenterHeight, 0)).Raycast(ray, out float distance))
            _roomBox.transform.position += ray.GetPoint(distance) - _roomBox.transform.TransformPoint(_boxAnchor);
    }

    private void FitZoomToAspect()
    {
        _lastAspect = _camera.aspect;
        float size = _zoomSize * Mathf.Max(1, ReferenceAspect / Mathf.Max(.1f, _lastAspect));
        Pose(_zoomTarget, size, Quaternion.Euler(2, 0, 0), new Vector2(.5f, .47f));
    }

    private void Pose(Vector3 target, float size, Quaternion rotation, Vector2 screenCenter)
    {
        _camera.orthographicSize = size;
        transform.rotation = rotation;
        transform.position = target - transform.forward * 40
            - transform.right * ((screenCenter.x - .5f) * 2 * size * _camera.aspect)
            - transform.up * ((screenCenter.y - .5f) * 2 * size);
    }

    private void SetBoxVisible(bool visible)
    {
        for (int i = 0; i < _boxRenderers.Length; i++) _boxRenderers[i].enabled = visible && _boxRendererStates[i];
        _boxCollider.enabled = visible;
    }
}
