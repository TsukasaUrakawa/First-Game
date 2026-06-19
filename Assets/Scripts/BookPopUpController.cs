using UnityEngine;
using UnityEngine.UI;

public class BookPopUpController : MonoBehaviour
{
    [SerializeField] GameObject _bookPopUpUI;
    [SerializeField] Image _bookImage;
    public void ShowBookPopUp(Sprite selectedBookSprite)
    {
        _bookImage.sprite = selectedBookSprite;
        _bookPopUpUI.SetActive(true);
    }

    public void CloseBookPopUp()
    {
        _bookPopUpUI.SetActive(false);
    }
}
