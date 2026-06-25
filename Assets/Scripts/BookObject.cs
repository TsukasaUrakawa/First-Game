using UnityEngine;

public class BookObject : MonoBehaviour
{
    public int CorrectSlotIndex { get; private set; }
    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite bookObjectSprite)
    {
        _spriteRenderer.sprite = bookObjectSprite;
    }
    public void SetCorrectSlotIndex(int index)
    {
        //受け取った番号を正解として保存
        CorrectSlotIndex = index;
    }
}
