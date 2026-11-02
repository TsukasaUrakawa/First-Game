using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class BookPopUpController : MonoBehaviour
{
    [SerializeField] private GameObject _bookPopUpUI;
    [SerializeField] private Image[] _bookImages = new Image[6];
    [SerializeField] private GameObject[] _bookCards = new GameObject[6];
    [SerializeField] private HandItemController _handItemController;
    [SerializeField] private TMP_Text _selectionCountLabel;
    [SerializeField] private TMP_Text _capacityLabel;
    [SerializeField] private Button _takeButton;
    [SerializeField] private RectTransform _dialogCard;
    [SerializeField] private CanvasGroup _dialogCanvasGroup;
    [SerializeField] private AudioClip _clickSound;
    [SerializeField] private AudioClip _bookSelectSound;
    [SerializeField] private AudioClip _bookDeselectSound;
    [SerializeField] private AudioClip _bookLimitSound;

    [SerializeField]private AudioSource _audioSource;

    public bool IsOpen => _bookPopUpUI != null && _bookPopUpUI.activeSelf;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    public void ShowSelectedBooks(IReadOnlyList<BookButton> selectedBooks)
    {
        if (selectedBooks.Count == 0) return;
        for (int i = 0; i < _bookImages.Length; i++)
        {
            if (_bookImages[i] == null)
            {
                continue;
            }

            bool hasBook = i < selectedBooks.Count;
            if (i < _bookCards.Length && _bookCards[i] != null) _bookCards[i].SetActive(hasBook);
            _bookImages[i].gameObject.SetActive(hasBook);
            _bookImages[i].sprite = hasBook ? selectedBooks[i].BookSprite : null;
        }

        _bookPopUpUI.SetActive(true);
        if (_selectionCountLabel != null) _selectionCountLabel.text = $"選択： {selectedBooks.Count} 冊";
        if (_capacityLabel != null)
            _capacityLabel.text = $"受け取り後の手持ち {_handItemController.BookCount + selectedBooks.Count} / {_handItemController.Capacity} 冊";
        if (_takeButton != null) _takeButton.interactable = selectedBooks.Count <= _handItemController.AvailableSpace;
        DOTween.Kill(this);
        if (_dialogCard != null && _dialogCanvasGroup != null)
        {
            _dialogCanvasGroup.alpha = 0;
            _dialogCard.localScale = Vector3.one * .97f;
            DOTween.Sequence().SetId(this).SetUpdate(true)
                .Join(_dialogCanvasGroup.DOFade(1, .16f))
                .Join(_dialogCard.DOScale(1, .16f).SetEase(Ease.OutCubic));
        }
    }

    public void CloseBookPopUp()
    {
        DOTween.Kill(this);
        if (_dialogCard != null) _dialogCard.localScale = Vector3.one;
        if (_dialogCanvasGroup != null) _dialogCanvasGroup.alpha = 1;
        _bookPopUpUI.SetActive(false);
    }

    private void OnDestroy() => DOTween.Kill(this);

    public void PlayClickSE()
    {
        PlaySound(_clickSound);
    }

    public void PlayBookSelectSE()
    {
        PlaySound(_bookSelectSound);
    }

    public void PlayBookDeselectSE()
    {
        PlaySound(_bookDeselectSound);
    }

    public void PlayBookLimitSE()
    {
        PlaySound(_bookLimitSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (_audioSource != null && clip != null)
        {
            _audioSource.PlayOneShot(clip);
        }
    }
}
