using UnityEngine;
using UnityEngine.EventSystems;

public class ShelfAreaClick : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private float _zoomSize = 3f;

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        _cameraController.ZoomTo(transform.position, _zoomSize);
    }
}