using UnityEngine;
using UnityEngine.UI;

public class BookButton : MonoBehaviour
{
    private Sprite _bookSprite;
    private BookPopUpController _bookPopUpController;
    private Image _bookImage;

    private void Awake()
    {
        _bookImage = GetComponent<Image>();
    }
    public void SetUp(Sprite BookSprite,BookPopUpController Controller)
    {
        _bookSprite = BookSprite;
        _bookPopUpController = Controller;
        _bookImage.sprite = _bookSprite;
    }

    //本の一覧に表示されている画像をUIManagerに渡す
    public void SendSprite()
    {
        _bookPopUpController.ShowBookPopUp(_bookSprite);
    }
}
