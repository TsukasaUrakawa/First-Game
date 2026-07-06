using UnityEngine;

/// <summary>
/// 全ての本画像
/// </summary>
public class BookManager : MonoBehaviour
{
    public Sprite[] _allBookSprites;

    private void Awake()
    {
        _allBookSprites = Resources.LoadAll<Sprite>("Books");
    }
}
