using UnityEngine;
using UnityEngine.UI;

public class BookPopUpController : MonoBehaviour
{
    private AudioSource _audioSource2;
    private Sprite _selectedBookSprite;
    private GameObject _selectedBookButton;
    [SerializeField] GameObject _bookPopUpUI;
    [SerializeField] Image _bookImage;
    [SerializeField] AudioClip _clickSound;
    [SerializeField] AudioClip _bookSelectSound;
    [SerializeField] GameObject _bookObjectPrefab;
    [SerializeField] Transform _spawnPoint;


    private void Awake()
    {
        _audioSource2 = GetComponent<AudioSource>();
    }
    //BookPopUpUIを表示する
    public void ShowBookPopUp(Sprite selectedBookSprite,GameObject selectedBookButton)
    {
        //Imageコンポーネントに画像をセット
        _bookImage.sprite = selectedBookSprite;
        _bookPopUpUI.SetActive(true);
        _selectedBookSprite = selectedBookSprite;
        _selectedBookButton = selectedBookButton;
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

    public void TakeBook()
    {
        GameObject book = Instantiate(_bookObjectPrefab,_spawnPoint.position,Quaternion.identity);
        BookObject bookObject = book.GetComponent<BookObject>();
        bookObject.SetSprite(_selectedBookSprite);
        _bookPopUpUI.SetActive(false);
        Destroy(_selectedBookButton);
    }
}
