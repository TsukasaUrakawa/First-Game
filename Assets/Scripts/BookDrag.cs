using UnityEngine;

public class BookDrag : MonoBehaviour
{
    private bool _isDragging;
    private Vector3 _offset;
    private void OnMouseDown()
    {
        _isDragging = true;
        _offset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        if (_isDragging)
        {
            Vector3 targetPosition = GetMouseWorldPosition() + _offset;

            targetPosition.x = Mathf.Clamp(targetPosition.x, -9f, 16f);
            targetPosition.y = Mathf.Clamp(targetPosition.y, -6f, 8.5f);

            transform.position = targetPosition;
        }
    }

    private void OnMouseUp()
    {
        _isDragging = false;
    }
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        return mousePosition;
    }
}
