using UnityEngine;
using UnityEngine.UI;

public class BookPopUpController : MonoBehaviour
{
    [SerializeField] GameObject _bookPopUpUI;
    [SerializeField] Image _bookImage;

    //BookPopUpUIを表示する
    public void ShowBookPopUp(Sprite selectedBookSprite)
    {
        //Imageコンポーネントに画像をセット
        _bookImage.sprite = selectedBookSprite;
        _bookPopUpUI.SetActive(true);
    }

    public void CloseBookPopUp()
    {
        _bookPopUpUI.SetActive(false);
    }
}
