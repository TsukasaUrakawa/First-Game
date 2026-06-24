using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    //ボタンに画像を配置
    public void SetUp(Sprite BookSprite,BookPopUpController Controller)
    {
        _bookSprite = BookSprite;
        _bookPopUpController = Controller;
        _bookImage.sprite = _bookSprite;
    }

    //本の一覧に表示されている画像をUIManagerに渡す
    public void SendSprite()
    {
        _bookPopUpController.PlayBookSelectSE();
        _bookPopUpController.ShowBookPopUp(_bookSprite,this.gameObject);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _frameImage.SetActive(true);
        _bookImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _frameImage.SetActive(false);
        _bookImage.color = Color.white;
    }
}
