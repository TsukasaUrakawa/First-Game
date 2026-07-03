using System.Collections.Generic;
using UnityEngine;

public class HandItemController : MonoBehaviour
{
    [SerializeField] private HandItemSlotUI[] _slots;

    private readonly List<HandBookData> _books = new();
    private int _selectedIndex = -1;

    public bool IsFull => _books.Count >= _slots.Length;
    public bool HasSelectedBook =>
        _selectedIndex >= 0 && _selectedIndex < _books.Count;

    private void Awake()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            _slots[i].Initialize(this, i);
        }

        RefreshUI();
    }

    public bool TryAddBook(Sprite sprite, int correctSlotIndex)
    {
        if (IsFull)
        {
            return false;
        }

        _books.Add(new HandBookData(sprite, correctSlotIndex));
        RefreshUI();
        return true;
    }

    public void SelectBook(int index)
    {
        if (index < 0 || index >= _books.Count)
        {
            return;
        }

        // 同じ本を再クリックすると選択解除
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
            Sprite sprite = i < _books.Count
                ? _books[i].Sprite
                : null;

            _slots[i].SetView(sprite, i == _selectedIndex);
        }
    }
}