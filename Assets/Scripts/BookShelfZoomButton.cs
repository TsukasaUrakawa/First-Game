using UnityEngine;

public class BookShelfZoomButton : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private GameObject _bookShelf;
    [SerializeField] private float _zoomSize = 3f;

    public void ZoomShelf()
    {
        if (_cameraController == null || _bookShelf == null)
        {
            Debug.LogWarning("カメラまたは本棚が設定されていません");
            return;
        }

        _cameraController.ZoomTo(
            _bookShelf.transform.position,
            _zoomSize
        );
    }
}