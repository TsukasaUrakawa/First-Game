using UnityEngine;
using UnityEngine.EventSystems;

public class PlacedBookClick : MonoBehaviour
{
    private BookSlot _currentSlot;

    public void SetCurrentSlot(BookSlot slot)
    {
        _currentSlot = slot;
    }

    private void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (_currentSlot == null ||
            _currentSlot.IsLocked ||
            HandBookPlacementController.Instance == null)
        {
            return;
        }

        HandBookPlacementController.Instance.TryReturnBookToHand(_currentSlot);
    }
}
