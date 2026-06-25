using UnityEngine;

public class BookJudgeManager : MonoBehaviour
{
    [SerializeField] private BookShelfJudge[] _shelves;

    public void CheckAllShelves()
    {
        bool allCorrect = true;

        foreach (BookShelfJudge shelf in _shelves)
        {
            bool shelfCorrect = shelf.CheckShelf();

            if (!shelfCorrect)
            {
                allCorrect = false;
            }
        }

        if (allCorrect)
        {
            Debug.Log("全部の棚が完成！");
        }
        else
        {
            Debug.Log("まだ間違っている棚があります");
        }
    }
}