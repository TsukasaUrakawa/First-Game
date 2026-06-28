using UnityEngine;
using UnityEngine.EventSystems;

public class ShelfAreaClick : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private float _zoomSize = 3f;

    private void OnMouseOver()
    {
        if (_cameraController.IsZoomed)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            _cameraController.ZoomTo(transform.position, _zoomSize);
        }
    }
}