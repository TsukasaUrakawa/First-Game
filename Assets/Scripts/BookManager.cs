using UnityEngine;

public class BookManager : MonoBehaviour
{
    public Sprite[] _allBookSprites;

    private void Awake()
    {
        _allBookSprites = Resources.LoadAll<Sprite>("Books");
    }
}
