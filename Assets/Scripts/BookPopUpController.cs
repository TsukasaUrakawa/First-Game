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
        //選ばれた本の画像を保存
        _selectedBookSprite = selectedBookSprite;
        //選ばれた本のボタンを保存
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
        //本の色に対応したプレハブを生成
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
        //生成した本に選ばれた本の画像をセット
        bookObject.SetSprite(_selectedBookSprite);

        int slotIndex = GetSlotIndexFromSpriteName(_selectedBookSprite.name);
        bookObject.SetCorrectSlotIndex(slotIndex);

        _bookPopUpUI.SetActive(false);
        Destroy(_selectedBookButton);
    }
    private int GetSlotIndexFromSpriteName(string spriteName)
    {
        //数字の部分だけ取り出す
        string numberPart = spriteName.Substring(spriteName.Length - 2);
        int number = int.Parse(numberPart);

        int offset = 0;

        if (spriteName.Contains("Green"))
        {
            offset = 0;
        }
        else if (spriteName.Contains("Blue"))
        {
            offset = 18;
        }
        else if (spriteName.Contains("Beige"))
        {
            offset = 35;
        }
        else if (spriteName.Contains("Red"))
        {
            offset = 53;
        }
        else if (spriteName.Contains("Purple"))
        {
            offset = 70;
        }
        else if (spriteName.Contains("Brown"))
        {
            offset = 88;
        }
        else if (spriteName.Contains("White"))
        {
            offset = 105;
        }
        else if (spriteName.Contains("Black"))
        {
            offset = 123;
        }

        return offset + number - 1;
    }
}
