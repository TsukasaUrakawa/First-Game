using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BookPopUpController : MonoBehaviour
{
    [SerializeField] private GameObject _bookPopUpUI;
    [SerializeField] private Image[] _bookImages = new Image[5];
    [SerializeField] private AudioClip _clickSound;
    [SerializeField] private AudioClip _bookSelectSound;
    [SerializeField] private AudioClip _takeBookSound;

    private AudioSource _audioSource;

    public bool IsOpen => _bookPopUpUI != null && _bookPopUpUI.activeSelf;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void ShowSelectedBooks(IReadOnlyList<BookButton> selectedBooks)
    {
        for (int i = 0; i < _bookImages.Length; i++)
        {
            if (_bookImages[i] == null)
            {
                continue;
            }

            bool hasBook = i < selectedBooks.Count;
            _bookImages[i].gameObject.SetActive(hasBook);
            _bookImages[i].sprite = hasBook ? selectedBooks[i].BookSprite : null;
        }

        _bookPopUpUI.SetActive(true);
    }

    public void CloseBookPopUp()
    {
        _bookPopUpUI.SetActive(false);
    }

    public void PlayClickSE()
    {
        PlaySound(_clickSound);
    }

    public void PlayBookSelectSE()
    {
        PlaySound(_bookSelectSound);
    }

    public void PlayTakeBookSE()
    {
        PlaySound(_takeBookSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}
