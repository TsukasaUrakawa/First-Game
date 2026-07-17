using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 本一覧に表示される1冊分のUIボタン
/// </summary>
public class BookButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject _frameImage;

    private Sprite _bookSprite;
    private BookSelectionController _selectionController;
    private Image _bookImage;

    public Sprite BookSprite => _bookSprite;
    public bool IsSelected { get; private set; }

    private void Awake()
    {
        _bookImage = GetComponent<Image>();
    }
    /// <summary>
    /// 本一覧の本一冊ずつに画像をセット
    /// </summary>
    /// <param name="bookSprite">本画像</param>
    /// <param name="selectionController"></param>
    public void SetUp(Sprite bookSprite, BookSelectionController selectionController)
    {
        _bookSprite = bookSprite;
        _selectionController = selectionController;
        //spriteコンポーネントに画像をセット
        _bookImage.sprite = bookSprite;
        SetSelected(false);
    }
    /// <summary>
    /// BookButtonのOnClickで呼ばれる
    /// </summary>
    public void SendSprite()
    {
        _selectionController.ToggleSelection(this); //押したボタンの情報をスクリプトに送る
    }

    /// <summary>
    /// 選択状態を変更する
    /// </summary>
    /// <param name="selected">選択されているか否か</param>
    public void SetSelected(bool selected)
    {
        IsSelected = selected;

        if (_frameImage != null)
        {
            _frameImage.SetActive(selected);
        }
    }
    /// <summary>
    /// カーソルが乗った時の処理
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_frameImage != null)
        {
            _frameImage.SetActive(true);
        }
        //本画像を暗くする
        _bookImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
    }
    /// <summary>
    /// カーソルが離れた時の処理
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_frameImage != null)
        {
            _frameImage.SetActive(IsSelected);
        }
        //明度を1にする
        _bookImage.color = Color.white;
    }
}
