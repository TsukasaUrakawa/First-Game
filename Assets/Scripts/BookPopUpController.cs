using UnityEngine;
using UnityEngine.UI;

public class BookPopUpController : MonoBehaviour
{
    private AudioSource _audioSource2;
    [SerializeField] GameObject _bookPopUpUI;
    [SerializeField] Image _bookImage;
    [SerializeField] AudioClip _clickSound;
    [SerializeField] AudioClip _bookSelectSound;


    private void Awake()
    {
        _audioSource2 = GetComponent<AudioSource>();
    }
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

    public void PlayClickSE()
    {
        _audioSource2.PlayOneShot(_clickSound);
    }

    public void PlayBookSelectSE()
    {
        _audioSource2.PlayOneShot(_bookSelectSound);
    }
}
