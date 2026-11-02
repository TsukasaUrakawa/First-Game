using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BookShelfSlotAligner)), CanEditMultipleObjects]
public class BookShelfSlotAlignerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
        {
            bool changed = DrawDefaultInspector();
            var shelves = targets.Cast<BookShelfSlotAligner>().ToArray();
            EditorGUILayout.LabelField("配置する本の数", shelves.Sum(s => s.SlotCount) + " 冊");
            EditorGUILayout.HelpBox(
                "開始位置は棚のローカル座標です。直下のBookSlotを番号順に等間隔で並べます。",
                MessageType.Info);

            if (GUILayout.Button("本の位置を整列") || changed)
            {
                AlignShelves(shelves);
            }
        }
    }

    [MenuItem("CONTEXT/BookShelfSlotAligner/Align Child Slots")]
    private static void AlignFromContextMenu(MenuCommand command)
    {
        if (!EditorApplication.isPlaying)
        {
            AlignShelves(new[] { (BookShelfSlotAligner)command.context });
        }
    }

    private static void AlignShelves(BookShelfSlotAligner[] shelves)
    {
        var children = shelves.SelectMany(s => s.transform.Cast<Transform>()).ToArray();
        Undo.RecordObjects(children, "Align Book Slots");
        foreach (BookShelfSlotAligner shelf in shelves)
        {
            shelf.AlignChildSlots();
        }

        foreach (Transform child in children)
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(child);
        }
    }
}
