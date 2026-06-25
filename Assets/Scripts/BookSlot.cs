using UnityEngine;

public class BookSlot : MonoBehaviour
{
    public bool IsFilled { get; private set; }

    [SerializeField] private int _slotIndex;
    public int SlotIndex => _slotIndex;

    private BookObject _placedBook;

    private void OnValidate()
    {
        //オブジェクトの名前にする
        string objectName = gameObject.name;

        if (objectName.StartsWith("Slot"))
        {
            string numberPart = objectName.Substring(4);
            //int型に変換できるか確認→numberに代入
            if (int.TryParse(numberPart, out int number))
            {
                //スロットの内部番号に変換
                _slotIndex = number - 1;
            }
        }
    }
    public void PlaceBook(Transform bookTransform)
    {
        //本の位置をスロットの位置にピッタリ吸着
        bookTransform.position = transform.position;
        IsFilled = true;
        //配置された本のBookObjectを保存
        _placedBook = bookTransform.GetComponent<BookObject>();
    }

    public void ClearSlot()
    {
        IsFilled = false;
        _placedBook = null;
    }

    public bool IsCorrect()
    {
        if (_placedBook == null)
        {
            return false;
        }
        //本が持っている正解のスロット番号と配置されたスロット番号が同じとき返す
        return _placedBook.CorrectSlotIndex == _slotIndex;
    }
}