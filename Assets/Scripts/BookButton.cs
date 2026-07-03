using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 本一覧にあるそれぞれの本につく
/// </summary>
public class BookButton : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
{
    [SerializeField] GameObject _frameImage;
    private Sprite _bookSprite;
    private BookPopUpController _bookPopUpController;
    private Image _bookImage;

    private void Awake()
    {
        _bookImage = GetComponent<Image>();
    }
    /// <summary>
    /// ボタンに画像を配置する
    /// </summary>
    /// <param name="BookSprite">本それぞれの画像</param>
    /// <param name="Controller">ポップアップを表示するためのオブジェクト</param>
    public void SetUp(Sprite BookSprite,BookPopUpController Controller)
    {
        _bookSprite = BookSprite;
        _bookPopUpController = Controller;
        _bookImage.sprite = _bookSprite;
    }

    /// <summary>
    /// 本一覧に表示されている画像をUIManagerに渡す
    /// </summary>
    public void SendSprite()
    {
        _bookPopUpController.PlayBookSelectSE();
        _bookPopUpController.ShowBookPopUp(_bookSprite,this.gameObject);
    }

    /// <summary>
    /// マウスカーソルが本の画像に乗ったときに起動
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        _frameImage.SetActive(true);
        _bookImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
    }
    /// <summary>
    /// マウスカーソルが本の画像から外れた時に軌道
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        _frameImage.SetActive(false);
        _bookImage.color = Color.white;
    }
}
