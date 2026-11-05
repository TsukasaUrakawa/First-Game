using UnityEngine;

/// <summary>Orthographic views show real 3D depth while keeping spine patterns easy to compare.</summary>
public class CameraController : MonoBehaviour
{
    [SerializeField] private GameplayUIController _gameplayUI;
    [SerializeField] private Transform _roomTarget;
    [SerializeField] private Vector2 _roomSize = new Vector2(26.8f, 16f);
    [SerializeField] private BoxOpenAndClose _roomBox;
    [SerializeField] private Renderer _boxBody;
    [SerializeField] private Renderer _boxContactShadow;
    [SerializeField] private Collider _boxCollider;
    [SerializeField] private float _floorHeight = -5.15f;
    private Camera _camera;
    private float _zoomSize, _lastAspect;
    private Vector3 _zoomTarget, _boxAnchor, _closedBoxExtents;
    private float _boxCenterHeight;
    private Renderer[] _boxRenderers;
    private bool[] _boxRendererStates;
    public bool IsZoomed { get; private set; }
    private const float ReferenceAspect = 16f / 9f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        // The click collider is slightly taller than the visible cardboard.
        // Ground the closed body, keeping its actual bottom on the floor.
        _boxAnchor = _roomBox.transform.InverseTransformPoint(_boxBody.bounds.center);
        _closedBoxExtents = _boxBody.bounds.extents;
        _boxCenterHeight = _floorHeight + _closedBoxExtents.y;
        var boxRenderers = _roomBox.GetComponentsInChildren<Renderer>();
        _boxRenderers = new Renderer[boxRenderers.Length + 1];
        System.Array.Copy(boxRenderers, _boxRenderers, boxRenderers.Length);
        _boxRenderers[boxRenderers.Length] = _boxContactShadow;
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
        var rotation = Quaternion.Euler(13, -5, 0);
        var boxViewport = new Rect(.015f, .020f, .25f, .235f);
        var extents = _closedBoxExtents;
        size = Mathf.Max(size, ProjectedDiameter(rotation * Vector3.right, extents) / (2 * _lastAspect * boxViewport.width),
            ProjectedDiameter(rotation * Vector3.up, extents) / (2 * boxViewport.height));
        // The low floor anchor can lie behind a close orthographic camera at
        // narrow aspect ratios. Leave depth for the whole box, not just its center.
        Pose(_roomTarget.position, size, rotation, viewport.center, 80);
        // Anchor the real box below the shelf selector, including after a window resize.
        var ray = _camera.ViewportPointToRay(boxViewport.center);
        if (new Plane(Vector3.up, new Vector3(0, _boxCenterHeight, 0)).Raycast(ray, out float distance))
            _roomBox.transform.position += ray.GetPoint(distance) - _roomBox.transform.TransformPoint(_boxAnchor);
        // The floor shadow has its own world transform: authored box rotation
        // and nonuniform scale must never tilt it or stretch it a second time.
        var center = _roomBox.transform.TransformPoint(_boxAnchor);
        _boxContactShadow.transform.SetPositionAndRotation(new Vector3(center.x, _floorHeight + .006f, center.z), Quaternion.identity);
        _boxContactShadow.transform.localScale = new Vector3(extents.x * 2.36f, 1, extents.z * 2.36f);
    }

    private static float ProjectedDiameter(Vector3 axis, Vector3 extents) => 2 *
        (Mathf.Abs(axis.x) * extents.x + Mathf.Abs(axis.y) * extents.y + Mathf.Abs(axis.z) * extents.z);

    private void FitZoomToAspect()
    {
        _lastAspect = _camera.aspect;
        float size = _zoomSize * Mathf.Max(1, ReferenceAspect / Mathf.Max(.1f, _lastAspect));
        Pose(_zoomTarget, size, Quaternion.Euler(8, -6, 0), new Vector2(.5f, .47f));
    }

    private void Pose(Vector3 target, float size, Quaternion rotation, Vector2 screenCenter, float distance = 40)
    {
        _camera.orthographicSize = size;
        transform.rotation = rotation;
        transform.position = target - transform.forward * distance
            - transform.right * ((screenCenter.x - .5f) * 2 * size * _camera.aspect)
            - transform.up * ((screenCenter.y - .5f) * 2 * size);
    }

    private void SetBoxVisible(bool visible)
    {
        for (int i = 0; i < _boxRenderers.Length; i++) _boxRenderers[i].enabled = visible && _boxRendererStates[i];
        _boxCollider.enabled = visible;
    }
}
