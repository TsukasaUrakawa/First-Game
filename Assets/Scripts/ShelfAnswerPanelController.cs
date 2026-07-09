using UnityEngine;
using UnityEngine.UI;

public class ShelfAnswerPanelController : MonoBehaviour
{
    [SerializeField] private GameObject _answerPanel;
    [SerializeField] private Image _answerImage;
    [SerializeField] private bool _hideOnStart = true;

    public static ShelfAnswerPanelController Instance { get; private set; }

    private void Awake()
    {
        Instance = this;

        if (_answerPanel != null && _hideOnStart)
        {
            _answerPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Open(Sprite answerSprite)
    {
        if (_answerPanel == null || _answerImage == null)
        {
            Debug.LogWarning("完成図パネルまたは完成図Imageが設定されていません");
            return;
        }

        if (answerSprite == null)
        {
            Debug.LogWarning("この本棚の完成図Spriteが設定されていません");
            return;
        }

        _answerImage.sprite = answerSprite;
        _answerImage.preserveAspect = true;
        _answerPanel.SetActive(true);
    }

    public void Close()
    {
        if (_answerPanel != null)
        {
            _answerPanel.SetActive(false);
        }
    }
}
