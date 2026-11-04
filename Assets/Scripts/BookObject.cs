using UnityEngine;

/// <summary>Book identity is independent of its 3D presentation and the current shelf.</summary>
public class BookObject : MonoBehaviour
{
    [SerializeField] private BookVisual3D _visual;
    public int CorrectSlotIndex { get; private set; }
    public Sprite Sprite => _visual.Sprite;
    public BookVisual3D Visual => _visual;
    public void SetSprite(Sprite sprite) => _visual.SetSprite(sprite);
    public void SetCorrectSlotIndex(int index) => CorrectSlotIndex = index;
}
