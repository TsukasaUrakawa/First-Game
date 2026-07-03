using System.Collections.Generic;
using UnityEngine;

public class HandItemController : MonoBehaviour
{
    private const int MaxCapacity = 5;

    [SerializeField] private HandItemSlotUI[] _slots = new HandItemSlotUI[MaxCapacity];

    private readonly List<HandBookData> _books = new List<HandBookData>();
    private int _selectedIndex = -1;

    public int Capacity => Mathf.Min(MaxCapacity, _slots.Length);
    public int BookCount => _books.Count;
    public int AvailableSpace => Mathf.Max(0, Capacity - BookCount);
    public bool IsFull => BookCount >= Capacity;
    public bool HasSelectedBook => _selectedIndex >= 0 && _selectedIndex < BookCount;

    private void Awake()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] != null)
            {
                _slots[i].Initialize(this, i);
            }
        }

        RefreshUI();
    }

    public bool TryAddBook(Sprite sprite, int correctSlotIndex)
    {
        if (sprite == null || IsFull)
        {
            return false;
        }

        _books.Add(new HandBookData(sprite, correctSlotIndex));
        RefreshUI();
        return true;
    }

    public void SelectBook(int index)
    {
        if (index < 0 || index >= BookCount)
        {
            return;
        }

        _selectedIndex = _selectedIndex == index ? -1 : index;
        RefreshUI();
    }

    public bool TryGetSelectedBook(out HandBookData book)
    {
        if (!HasSelectedBook)
        {
            book = null;
            return false;
        }

        book = _books[_selectedIndex];
        return true;
    }

    public bool RemoveSelectedBook()
    {
        if (!HasSelectedBook)
        {
            return false;
        }

        _books.RemoveAt(_selectedIndex);
        _selectedIndex = -1;
        RefreshUI();
        return true;
    }

    private void RefreshUI()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i] == null)
            {
                continue;
            }

            Sprite sprite = i < BookCount ? _books[i].Sprite : null;
            _slots[i].SetView(sprite, i == _selectedIndex);
        }
    }
}
