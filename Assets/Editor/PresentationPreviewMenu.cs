using UnityEditor;
using UnityEngine;

public static class PresentationPreviewMenu
{
    private const string ClearMenu = "Tools/Shelf Care/Preview Clear Screen";

    [MenuItem(ClearMenu)]
    private static void PreviewClear()
    {
        Object.FindFirstObjectByType<GameClearEffectController>().PlayClearEffect();
    }

    [MenuItem(ClearMenu, true)]
    private static bool CanPreviewClear() => EditorApplication.isPlaying &&
        GameplayUIController.Instance != null && !GameplayUIController.Instance.IsGameCleared &&
        Object.FindFirstObjectByType<GameClearEffectController>() != null;
}
