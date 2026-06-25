using UnityEngine;

public class BookDrag : MonoBehaviour
{
    private bool _isDragging;
    private Vector3 _offset;
    [SerializeField] private float _snapDistance = 1f;
    private BookSlot _currentSlot;

    private void OnMouseDown()
    {
        //本がすでにスロットに入っていたら空にする
        if (_currentSlot != null)
        {
            _currentSlot.ClearSlot();
            _currentSlot = null;
        }

        _isDragging = true;
        //マウスの位置と本の中心とのズレを保存
        _offset = transform.position - GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        if (_isDragging)
        {
            //本の移動先を作る
            Vector3 targetPosition = GetMouseWorldPosition() + _offset;

            //範囲制限
            targetPosition.x = Mathf.Clamp(targetPosition.x, -9f, 16f);
            targetPosition.y = Mathf.Clamp(targetPosition.y, -6f, 8.5f);

            transform.position = targetPosition;
        }
    }

    private void OnMouseUp()
    {
        _isDragging = false;

        BookSlot nearestSlot = FindNearestSlot();

        if (nearestSlot != null && !nearestSlot.IsFilled)
        {
            nearestSlot.PlaceBook(transform);
            _currentSlot = nearestSlot;
        }
    }
    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0;
        return mousePosition;
    }
    private BookSlot FindNearestSlot()
    {
        //BookSlotコンポーネントをもつオブジェクトを全部探して配列にする
        BookSlot[] slots = FindObjectsByType<BookSlot>(FindObjectsSortMode.None);

        BookSlot nearestSlot = null;
        float nearestDistance = _snapDistance;

        foreach (BookSlot slot in slots)
        {
            //本とスロットの距離を測る
            float distance = Vector3.Distance(transform.position, slot.transform.position);

            if (distance < nearestDistance)
            {
                //スロットとの最短距離の更新
                nearestDistance = distance;
                //一番近いスロットの更新
                nearestSlot = slot;
            }
        }
        //一番近かったスロットを返す
        return nearestSlot;
    }
}
