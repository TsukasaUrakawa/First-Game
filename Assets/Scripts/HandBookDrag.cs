using UnityEngine;
using UnityEngine.EventSystems;

public class HandBookDrag : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private GameObject[] _bookPrefabs;

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("手持ち本クリック");

        if (!_handItemController.HasBook)
        {
            return;
        }

        GameObject prefab = GetPrefabFromSpriteName(_handItemController.CurrentBookSprite.name);

        if (prefab == null)
        {
            Debug.LogWarning("対応する本Prefabが見つかりません");
            return;
        }

        Vector3 spawnPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        spawnPosition.z = 0;

        GameObject book = Instantiate(prefab, spawnPosition, Quaternion.identity);

        BookObject bookObject = book.GetComponent<BookObject>();
        bookObject.SetSprite(_handItemController.CurrentBookSprite);
        bookObject.SetCorrectSlotIndex(_handItemController.CurrentCorrectSlotIndex);

        _handItemController.ClearHandBook();
    }

    private GameObject GetPrefabFromSpriteName(string spriteName)
    {
        if (spriteName.Contains("Green")) return _bookPrefabs[0];
        if (spriteName.Contains("Blue")) return _bookPrefabs[1];
        if (spriteName.Contains("Beige")) return _bookPrefabs[2];
        if (spriteName.Contains("Red")) return _bookPrefabs[3];
        if (spriteName.Contains("Purple")) return _bookPrefabs[4];
        if (spriteName.Contains("Brown")) return _bookPrefabs[5];
        if (spriteName.Contains("White")) return _bookPrefabs[6];
        if (spriteName.Contains("Black")) return _bookPrefabs[7];

        return null;
    }
}