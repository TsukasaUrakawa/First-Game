using UnityEngine;

public class BookButton : MonoBehaviour
{
    [SerializeField] Sprite _bookSprite;
    [SerializeField] BookPopUpController _bookPopUpController;

    public void SendSprite()
    {
        _bookPopUpController.ShowBookPopUp(_bookSprite);
    }
}
