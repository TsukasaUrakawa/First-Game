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
    [SerializeField] Transform _spawnPoint;
    [SerializeField] private GameObject[] _bookPrefabs;
    [SerializeField] private GameObject _effectPrefab;
    [SerializeField] AudioClip _takeBookSound;


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
        GameObject prefab = null;
        if (_selectedBookSprite.name.Contains("Green"))
        {
            prefab = _bookPrefabs[0];
        }
        else if (_selectedBookSprite.name.Contains("Blue"))
        {
            prefab = _bookPrefabs[1];
        }
        else if (_selectedBookSprite.name.Contains("Beige"))
        {
            prefab = _bookPrefabs[2];
        }
        else if (_selectedBookSprite.name.Contains("Red"))
        {
            prefab = _bookPrefabs[3];
        }
        else if (_selectedBookSprite.name.Contains("Purple"))
        {
            prefab = _bookPrefabs[4];
        }
        else if (_selectedBookSprite.name.Contains("Brown"))
        {
            prefab = _bookPrefabs[5];
        }
        else if (_selectedBookSprite.name.Contains("White"))
        {
            prefab = _bookPrefabs[6];
        }
        else if (_selectedBookSprite.name.Contains("Black"))
        {
            prefab = _bookPrefabs[7];
        }
        GameObject book = Instantiate(prefab,_spawnPoint.position,Quaternion.identity);
        //本取り出し音再生
        if (_takeBookSound != null)
        {
            _audioSource2.PlayOneShot(_takeBookSound);
        }
        //エフェクト生成
        if (_effectPrefab != null)
        {
            Instantiate(_effectPrefab, _spawnPoint.position, Quaternion.identity);
        }
        BookObject bookObject = book.GetComponent<BookObject>();
        bookObject.SetSprite(_selectedBookSprite);
        _bookPopUpUI.SetActive(false);
        Destroy(_selectedBookButton);
    }
}
