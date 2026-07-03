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
    [SerializeField] private GameObject[] _bookPrefabs;
    [SerializeField] AudioClip _takeBookSound;
    [SerializeField] private HandItemController _handItemController;


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
        if (_handItemController.HasBook)
        {
            Debug.Log("すでに本を持っています");
            return;
        }

        int slotIndex = GetSlotIndexFromSpriteName(_selectedBookSprite.name);

        _handItemController.SetHandBook(_selectedBookSprite, slotIndex);

        if (_takeBookSound != null)
        {
            _audioSource2.PlayOneShot(_takeBookSound);
        }
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
