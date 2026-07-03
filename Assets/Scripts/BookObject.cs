using UnityEngine;

public class BookObject : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    private float _targetWorldWidth = 0.58f;

    public int CorrectSlotIndex { get; private set; }

    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _boxCollider;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _boxCollider = GetComponent<BoxCollider2D>();
    }

    public void SetSprite(Sprite bookObjectSprite)
    {
        _spriteRenderer.sprite = bookObjectSprite;
        NormalizeBookWidth(bookObjectSprite);
    }

    public void SetCorrectSlotIndex(int index)
    {
        CorrectSlotIndex = index;
    }

    private void NormalizeBookWidth(Sprite sprite)
    {
        float spriteWidth = sprite.bounds.size.x;

        if (spriteWidth <= 0f)
        {
            return;
        }

        float parentScaleX = transform.parent != null
            ? Mathf.Abs(transform.parent.lossyScale.x)
            : 1f;

        Vector3 scale = transform.localScale;

        // Spriteの表示幅がtargetWorldWidthになるよう補正
        scale.x = _targetWorldWidth /
                  (spriteWidth * parentScaleX);

        transform.localScale = scale;

        // Colliderも表示中の本と同じ幅にする
        if (_boxCollider != null)
        {
            Vector2 colliderSize = _boxCollider.size;
            colliderSize.x = spriteWidth;
            _boxCollider.size = colliderSize;
        }
    }
}