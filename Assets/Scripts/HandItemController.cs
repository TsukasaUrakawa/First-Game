using System.Collections.Generic;
using UnityEngine;

public class HandItemController : MonoBehaviour
{
    private const int MaxCapacity = 5;

    [SerializeField] private HandItemSlotUI[] _slots = new HandItemSlotUI[MaxCapacity];

    [Header("Sound")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _itemClickSound;
    [SerializeField] private AudioClip _itemCancelSound;
    [SerializeField, Range(0f, 1f)] private float _itemClickVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float _itemCancelVolume = 1f;

    private readonly List<HandBookData> _books = new List<HandBookData>();
    private int _selectedIndex = -1;

    public int Capacity => Mathf.Min(MaxCapacity, _slots.Length);
    public int BookCount => _books.Count;
    public int AvailableSpace => Mathf.Max(0, Capacity - BookCount);
    public bool IsFull => BookCount >= Capacity;
    public bool HasSelectedBook => _selectedIndex >= 0 && _selectedIndex < BookCount;

    private void Awake()
    {
        if (_audioSource == null)
        {
            _audioSource = GetComponent<AudioSource>();
        }

        if (_audioSource == null && (_itemClickSound != null || _itemCancelSound != null))
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }

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

        bool isDeselecting = _selectedIndex == index;

        _selectedIndex = isDeselecting ? -1 : index;
        RefreshUI();

        if (isDeselecting)
        {
            PlayItemDeselectSound();
        }
        else
        {
            PlayItemClickSound();
        }
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

    private void PlayItemClickSound()
    {
        if (_audioSource != null && _itemClickSound != null)
        {
            _audioSource.PlayOneShot(_itemClickSound, _itemClickVolume);
        }
    }

    private void PlayItemDeselectSound()
    {
        if (_audioSource != null && _itemCancelSound != null)
        {
            _audioSource.PlayOneShot(_itemCancelSound, _itemCancelVolume);
        }
    }
}
