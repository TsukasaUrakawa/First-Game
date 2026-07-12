using UnityEngine;

public class BookObject : MonoBehaviour
{
    [Header("Display Size")]
    [SerializeField, Min(0.01f)]
    private float _targetWorldWidth = 0.58f;

    [SerializeField, Min(0.01f)]
    private float _targetWorldHeight = 2.3f;

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
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (_boxCollider == null)
        {
            _boxCollider = GetComponent<BoxCollider2D>();
        }

        if (_spriteRenderer == null || bookObjectSprite == null)
        {
            return;
        }

        _spriteRenderer.sprite = bookObjectSprite;

        NormalizeBookSize(bookObjectSprite);
        AdjustCollider(bookObjectSprite);
    }

    public void SetCorrectSlotIndex(int index)
    {
        CorrectSlotIndex = index;
    }

    private void NormalizeBookSize(Sprite sprite)
    {
        float spriteWidth = sprite.bounds.size.x;
        float spriteHeight = sprite.bounds.size.y;

        if (spriteWidth <= 0f || spriteHeight <= 0f)
        {
            return;
        }

        float parentScaleX = transform.parent != null
            ? Mathf.Abs(transform.parent.lossyScale.x)
            : 1f;

        float parentScaleY = transform.parent != null
            ? Mathf.Abs(transform.parent.lossyScale.y)
            : 1f;

        Vector3 scale = transform.localScale;

        scale.x = _targetWorldWidth / (spriteWidth * parentScaleX);
        scale.y = _targetWorldHeight / (spriteHeight * parentScaleY);

        transform.localScale = scale;
    }

    private void AdjustCollider(Sprite sprite)
    {
        if (_boxCollider == null || sprite == null)
        {
            return;
        }

        _boxCollider.size = sprite.bounds.size;
        _boxCollider.offset = sprite.bounds.center;
    }
}