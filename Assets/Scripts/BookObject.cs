using UnityEngine;

public class BookObject : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;
    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetSprite(Sprite bookObjectSprite)
    {
        _spriteRenderer.sprite = bookObjectSprite;
    }
}
