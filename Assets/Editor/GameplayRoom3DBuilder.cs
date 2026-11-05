using System;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/// <summary>Builds real 3D shelf geometry while preserving the 140 book identities and slot indices.</summary>
public static class GameplayRoom3DBuilder
{
    private const string Folder = "Assets/Presentation/Gameplay3D";
    private const string MeshPath = Folder + "/Meshes.asset";
    private static readonly string[] Colors = { "Green", "Blue", "Beige", "Red", "Purple", "Brown", "White", "Black" };
    private static readonly string[] ClothColors = { "#35402B", "#263643", "#B69B70", "#793226", "#57405D", "#775034", "#E4D5B6", "#282D30" };
    private static readonly float[] RowY = { 7.2f, 3.85f, .5f, -2.85f };
    private const float CaseCenter = 3.55f, PostWidth = .26f, BookWidth = .68f, EndClearance = .02f;
    private static float BayWidth(int column) => BookColorUtility.GetShelfSlotCount(column) * BookWidth + 2 * EndClearance;
    private static float CaseWidth => BayWidth(0) + BayWidth(1) + 3 * PostWidth;
    private static float PostX(int post) => CaseCenter - CaseWidth / 2 + PostWidth / 2
        + (post >= 1 ? BayWidth(0) + PostWidth : 0) + (post >= 2 ? BayWidth(1) + PostWidth : 0);
    private static float BayX(int column) => (PostX(column) + PostX(column + 1)) / 2;

    [MenuItem("Tools/Shelf Care/Rebuild 3D Gameplay Room")]
    public static void Rebuild()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (EditorApplication.isPlaying || scene.path != "Assets/Scenes/ShelfCare.unity" || scene.isDirty)
            throw new InvalidOperationException("Open the saved ShelfCare scene in Edit mode first.");
        System.IO.Directory.CreateDirectory("Assets/.utmp");
        EditorSceneManager.SaveScene(scene, "Assets/.utmp/before-3d-gameplay.unity", true);
        EnsureFolder("Assets", "Presentation"); EnsureFolder("Assets/Presentation", "Gameplay3D");
        // The same source images serve UI and real 3D spines; mipmaps keep distant
        // gold lines stable without making a second set of book textures.
        for (int i = 1; i <= 8; i++)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/Resources/Books/Book" + i + ".png");
            if (importer.mipmapEnabled && importer.filterMode == FilterMode.Trilinear && importer.anisoLevel == 4) continue;
            importer.mipmapEnabled = true; importer.filterMode = FilterMode.Trilinear; importer.anisoLevel = 4;
            importer.SaveAndReimport();
        }
        var sprites = new Sprite[BookColorUtility.TotalBooks];
        foreach (var sprite in Resources.LoadAll<Sprite>("Books"))
            if (BookColorUtility.TryGetBookIndex(sprite.name, out int index)) sprites[index] = sprite;
        if (sprites.Any(s => s == null)) throw new InvalidOperationException("Expected all 140 original spine sprites.");
        var library = BuildLibrary(sprites);
        var ghost = BuildGhost(library, sprites[0]);
        for (int color = 0; color < 8; color++) BuildBookPrefab(color, library, sprites.First(s => BookColorUtility.GetColorIndexFromSpriteName(s.name) == color));
        MigrateShelfPrefab(ghost);

        FitShelfRows();
        foreach (var slot in Object.FindObjectsByType<BookSlot>(FindObjectsSortMode.None)) ConfigureSlot(slot, ghost);
        foreach (var renderer in Object.FindObjectsByType<SpriteRenderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (renderer.name == "bookshelf" || renderer.name == "Square") renderer.enabled = false;

        var old = GameObject.Find("GameplayRoom3D"); if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("GameplayRoom3D");
        var grain = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Presentation/TitleBooks/ClothGrain.asset");
        var wood = Mat("Walnut", "#65452D", .04f, .36f, Grain("WoodGrain", false));
        var paint = Mat("PaintedTeal", "#15383C", .08f, .32f, grain);
        var copper = Mat("CopperTrim", "#BA8C53", .35f, .50f, grain);
        var darkBack = Mat("Backboard", "#422D22", .02f, .26f, Grain("WoodGrain", false));
        var wall = Mat("Wall", "#102B30", 0, .12f, grain);
        var floor = Mat("Floor", "#352E26", .04f, .28f, Grain("WoodGrain", false));
        BuildCabinet(root.transform, wood, paint, copper, darkBack);
        Board("Wall", root.transform, new Vector3(3.55f, 9, 4.2f), new Vector3(100, 60, .2f), wall, 0);
        Board("Floor", root.transform, new Vector3(0, -5.22f, -15), new Vector3(200, .14f, 200), floor, 0);
        var target = Child("RoomCameraTarget", root.transform); target.position = new Vector3(3.55f, 2.10f, .85f);

        var box = Object.FindFirstObjectByType<BoxOpenAndClose>();
        var boxMesh = box.GetComponentInChildren<SkinnedMeshRenderer>();
        // Keep the box's authored rotation and size when regenerating the room.
        BuildBoxContactShadow(boxMesh);
        var tutorial = Object.FindFirstObjectByType<BookPlacementTutorial>();
        var boxCollider = (Collider)new SerializedObject(tutorial).FindProperty("_boxCollider").objectReferenceValue;
        var camera = Camera.main; camera.orthographic = true; camera.nearClipPlane = .1f; camera.farClipPlane = 250;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = C("#10282D");
        var cameraData = camera.GetComponent<UniversalAdditionalCameraData>();
        cameraData.renderPostProcessing = true; cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        var control = camera.GetComponent<CameraController>();
        Ref(control, "_roomTarget", target); Ref(control, "_roomBox", box); Ref(control, "_boxCollider", boxCollider);
        Ref(control, "_boxBody", boxMesh);
        Ref(control, "_boxContactShadow", Named("FloorContactShadow").GetComponent<Renderer>());
        var framing = new SerializedObject(control); framing.FindProperty("_roomSize").vector2Value = new Vector2(26.8f, 16);
        framing.FindProperty("_floorHeight").floatValue = -5.15f; framing.ApplyModifiedPropertiesWithoutUndo();
        typeof(CameraController).GetMethod("Awake", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(control, null);
        foreach (var button in Object.FindObjectsByType<BookShelfZoomButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var so = new SerializedObject(button); so.FindProperty("_zoomSize").floatValue = 4.75f; so.ApplyModifiedPropertiesWithoutUndo();
        }
        var key = Named("Directional Light").GetComponent<Light>();
        key.transform.rotation = Quaternion.Euler(38, -28, 0); key.color = C("#FFE6BF"); key.intensity = 1.65f;
        key.shadows = LightShadows.Soft; key.shadowBias = .01f; key.shadowNormalBias = .03f;
        // URP otherwise ignores the light's small contact bias and uses the pipeline default.
        key.GetUniversalAdditionalLightData().usePipelineSettings = false;
        Point("WarmShelfLight", root.transform, new Vector3(-10, 13, -8), "#FFDCA9", 35, 48);
        Point("CoolShelfFill", root.transform, new Vector3(18, 7, -7), "#A1C5C8", 22, 45);
        RenderSettings.ambientMode = AmbientMode.Trilight; RenderSettings.ambientSkyColor = C("#7C9797");
        RenderSettings.ambientEquatorColor = C("#516263"); RenderSettings.ambientGroundColor = C("#263437");
        RenderSettings.fog = true; RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = C("#10282D"); RenderSettings.fogDensity = .005f;
        foreach (string pipelinePath in new[] { "Assets/Settings/PC_RPAsset.asset", "Assets/Settings/Mobile_RPAsset.asset" })
        {
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            pipeline.shadowDistance = Mathf.Max(100, pipeline.shadowDistance); EditorUtility.SetDirty(pipeline); AssetDatabase.SaveAssetIfDirty(pipeline);
        }
        ConfigureUI();
        foreach (var component in Object.FindObjectsByType<Component>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if ((component is Graphic || component is Transform || component is Collider || component is SpriteRenderer || component is BookShelfZoomButton)
                && PrefabUtility.IsPartOfPrefabInstance(component)) PrefabUtility.RecordPrefabInstancePropertyModifications(component);
        foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(MeshPath)) AssetDatabase.SaveAssetIfDirty(asset);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        Debug.Log("3D gameplay room rebuilt: 8 shelves, 140 original spines, 3D colliders, lazy 3D previews.");
    }

    private static void FitShelfRows()
    {
        var shelves = Object.FindObjectsByType<BookShelfJudge>(FindObjectsSortMode.None).OrderBy(s => s.name).ToArray();
        if (shelves.Length != 8) throw new InvalidOperationException("Expected eight shelves.");
        for (int color = 0; color < shelves.Length; color++)
        {
            var shelf = shelves[color]; var aligner = shelf.GetComponent<BookShelfSlotAligner>();
            var slots = shelf.GetComponentsInChildren<BookSlot>(true);
            if (slots.Length != BookColorUtility.GetShelfSlotCount(color)) throw new InvalidOperationException("Unexpected shelf capacity.");
            shelf.transform.position = new Vector3(BayX(color % 2), RowY[color / 2], 0);
            shelf.transform.rotation = Quaternion.identity; shelf.transform.localScale = Vector3.one;
            var so = new SerializedObject(aligner);
            so.FindProperty("_startLocalPosition").vector3Value = new Vector3(-(slots.Length - 1) * BookWidth / 2, 0, 0);
            so.FindProperty("_slotSpacing").floatValue = BookWidth; so.ApplyModifiedPropertiesWithoutUndo(); aligner.AlignChildSlots();
        }
    }

    private static void BuildCabinet(Transform room, Material wood, Material paint, Material copper, Material back)
    {
        var cabinet = Child("Bookcase", room);
        // Each bay follows its own book count, without stretching any book or its artwork.
        for (int post = 0; post < 3; post++)
            Board("Upright", cabinet, new Vector3(PostX(post), 2.10f, .85f), new Vector3(PostWidth, 14.05f, 2.05f), paint);
        for (int column = 0; column < 2; column++)
        {
            float x = BayX(column), width = BayWidth(column) + .03f;
            Board("Backboard", cabinet, new Vector3(x, 2.10f, 1.93f), new Vector3(width, 14.05f, .12f), back);
            foreach (float y in RowY.Select(v => v - 1.61f).Concat(new[] { 9.02f }))
            {
                Board("Shelf", cabinet, new Vector3(x, y, .85f), new Vector3(width, .18f, 2.05f), wood);
                Board("ShelfFront", cabinet, new Vector3(x, y - .015f, -.19f), new Vector3(width, .12f, .075f), paint);
                Board("ShelfCopper", cabinet, new Vector3(x, y + .071f, -.235f), new Vector3(width, .018f, .018f), copper, .004f);
            }
        }
        float total = CaseWidth + .42f;
        Board("Cornice", cabinet, new Vector3(CaseCenter, 9.18f, .85f), new Vector3(total, .24f, 2.32f), paint);
        Board("CorniceCopper", cabinet, new Vector3(CaseCenter, 9.30f, -.23f), new Vector3(total, .025f, .035f), copper, .006f);
        Board("Plinth", cabinet, new Vector3(CaseCenter, -4.825f, .85f), new Vector3(total, .65f, 2.24f), paint);
        Board("PlinthCopper", cabinet, new Vector3(CaseCenter, -4.47f, -.24f), new Vector3(total, .025f, .025f), copper, .006f);
    }

    private static BookVisualLibrary BuildLibrary(Sprite[] sprites)
    {
        var grain = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Presentation/TitleBooks/ClothGrain.asset");
        var body = new ShelfCareMeshGeometry(2);
        body.Box(new Vector3(-.3225f, 0, .72f), new Vector3(.035f, 3.04f, 1.46f), 0, .006f);
        body.Box(new Vector3(.3225f, 0, .72f), new Vector3(.035f, 3.04f, 1.46f), 0, .006f);
        body.Box(new Vector3(0, 0, .035f), new Vector3(.64f, 3.0f, .065f), 0, .006f);
        body.Box(new Vector3(0, 0, .735f), new Vector3(.610f, 2.93f, 1.39f), 1, .005f);
        var bodyMesh = SaveMesh(body.Mesh("Meshes"));
        var spineMeshes = new Mesh[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            var sourceUV = sprites[i].uv;
            float left = sourceUV.Min(v => v.x), right = sourceUV.Max(v => v.x), low = sourceUV.Min(v => v.y), high = sourceUV.Max(v => v.y);
            var mesh = new ShelfCareMeshGeometry(1);
            mesh.Quad(new Vector3(-.34f, -1.5f, -.018f), new Vector3(-.34f, 1.5f, -.018f),
                new Vector3(.34f, 1.5f, -.018f), new Vector3(.34f, -1.5f, -.018f), Vector3.back, 0,
                new[] { new Vector2(left, low), new Vector2(left, high), new Vector2(right, high), new Vector2(right, low) });
            spineMeshes[i] = SaveMesh(mesh.Mesh("Spine" + i.ToString("000")));
        }
        var covers = new Material[8]; var spines = new Material[8]; var ghosts = new Material[8];
        for (int i = 0; i < 8; i++)
        {
            covers[i] = Mat("Cloth" + Colors[i], ClothColors[i], .02f, .24f, grain);
            var texture = sprites.First(s => BookColorUtility.GetColorIndexFromSpriteName(s.name) == i).texture;
            spines[i] = Mat("Spine" + Colors[i], "#FFFFFF", .02f, .30f, texture, false, true);
            ghosts[i] = Mat("Preview" + Colors[i], "#B8EFC5", 0, .12f, texture, true, true, .48f);
        }
        var paper = Mat("Paper", "#F0DFC0", 0, .12f, Grain("PaperGrain", true));
        var ghostBody = Mat("PreviewBody", "#5DAF7B", 0, .12f, grain, true, false, .52f);
        var ghostPaper = Mat("PreviewPaper", "#C3D7BB", 0, .08f, Grain("PaperGrain", true), true, false, .62f);
        string path = Folder + "/BookVisualLibrary.asset";
        var library = AssetDatabase.LoadAssetAtPath<BookVisualLibrary>(path);
        if (library == null) { library = ScriptableObject.CreateInstance<BookVisualLibrary>(); AssetDatabase.CreateAsset(library, path); }
        var so = new SerializedObject(library); so.FindProperty("_bodyMesh").objectReferenceValue = bodyMesh;
        var entries = so.FindProperty("_spines"); entries.arraySize = sprites.Length;
        for (int i = 0; i < sprites.Length; i++)
        {
            var entry = entries.GetArrayElementAtIndex(i);
            entry.FindPropertyRelative("Sprite").objectReferenceValue = sprites[i];
            entry.FindPropertyRelative("Mesh").objectReferenceValue = spineMeshes[i];
            entry.FindPropertyRelative("ColorIndex").intValue = BookColorUtility.GetColorIndexFromSpriteName(sprites[i].name);
        }
        ObjectArray(so.FindProperty("_coverMaterials"), covers); ObjectArray(so.FindProperty("_spineMaterials"), spines);
        ObjectArray(so.FindProperty("_ghostSpineMaterials"), ghosts);
        so.FindProperty("_paperMaterial").objectReferenceValue = paper; so.FindProperty("_ghostBodyMaterial").objectReferenceValue = ghostBody;
        so.FindProperty("_ghostPaperMaterial").objectReferenceValue = ghostPaper;
        so.ApplyModifiedPropertiesWithoutUndo(); AssetDatabase.SaveAssetIfDirty(library); return library;
    }

    private static BookVisual3D BuildGhost(BookVisualLibrary library, Sprite sprite)
    {
        var root = new GameObject("GhostBook3D");
        try
        {
            BuildVisual(root, library, sprite, true);
            return PrefabUtility.SaveAsPrefabAsset(root, Folder + "/GhostBook3D.prefab").GetComponent<BookVisual3D>();
        }
        finally { Object.DestroyImmediate(root); }
    }

    private static void BuildBookPrefab(int color, BookVisualLibrary library, Sprite sprite)
    {
        string path = "Assets/Prefabs/BookPrefabs/" + Colors[color] + "BookObjectPrefab.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try
        {
            foreach (var renderer in root.GetComponents<SpriteRenderer>()) Object.DestroyImmediate(renderer);
            foreach (var collider in root.GetComponents<Collider2D>()) Object.DestroyImmediate(collider);
            var visual = BuildVisual(root, library, sprite, false);
            Ref(root.GetComponent<BookObject>(), "_visual", visual);
            var collider3D = root.GetComponent<BoxCollider>();
            if (collider3D == null) collider3D = root.AddComponent<BoxCollider>();
            collider3D.center = new Vector3(0, 0, .716f); collider3D.size = new Vector3(.68f, 3.04f, 1.47f);
            root.transform.localScale = Vector3.one;
            PrefabUtility.SaveAsPrefabAsset(root, path);
        }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static BookVisual3D BuildVisual(GameObject root, BookVisualLibrary library, Sprite sprite, bool ghost)
    {
        foreach (string name in new[] { "BindingAndPages", "Spine" })
        {
            var previous = root.transform.Find(name); if (previous != null) Object.DestroyImmediate(previous.gameObject);
        }
        var body = Child("BindingAndPages", root.transform).gameObject;
        var bodyMesh = body.AddComponent<MeshFilter>(); var bodyRenderer = body.AddComponent<MeshRenderer>();
        var front = Child("Spine", root.transform).gameObject;
        var frontMesh = front.AddComponent<MeshFilter>(); var frontRenderer = front.AddComponent<MeshRenderer>();
        bodyRenderer.shadowCastingMode = ghost ? ShadowCastingMode.Off : ShadowCastingMode.On;
        frontRenderer.shadowCastingMode = ShadowCastingMode.Off;
        bodyRenderer.receiveShadows = frontRenderer.receiveShadows = !ghost;
        var visual = root.GetComponent<BookVisual3D>();
        if (visual == null) visual = root.AddComponent<BookVisual3D>();
        Ref(visual, "_library", library); Ref(visual, "_bodyMesh", bodyMesh); Ref(visual, "_body", bodyRenderer);
        Ref(visual, "_spineMesh", frontMesh); Ref(visual, "_spine", frontRenderer);
        var so = new SerializedObject(visual); so.FindProperty("_ghost").boolValue = ghost; so.ApplyModifiedPropertiesWithoutUndo();
        visual.SetSprite(sprite); EditorUtility.SetDirty(visual); return visual;
    }

    private static void MigrateShelfPrefab(BookVisual3D ghost)
    {
        const string path = "Assets/Prefabs/BookShelfPrefab.prefab";
        var root = PrefabUtility.LoadPrefabContents(path);
        try { foreach (var slot in root.GetComponentsInChildren<BookSlot>(true)) ConfigureSlot(slot, ghost); PrefabUtility.SaveAsPrefabAsset(root, path); }
        finally { PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void ConfigureSlot(BookSlot slot, BookVisual3D ghost)
    {
        foreach (var collider in slot.GetComponents<Collider2D>()) Object.DestroyImmediate(collider);
        var box = slot.GetComponent<BoxCollider>();
        if (box == null) box = slot.gameObject.AddComponent<BoxCollider>();
        box.center = new Vector3(0, 0, .70f); box.size = new Vector3(.68f, 3.04f, 1.44f);
        Ref(slot, "_ghostPrefab", ghost);
    }

    private static void ConfigureUI()
    {
        foreach (string name in new[] { "ShelfButtonPanel", "HandItemPanel", "ShelfActionPanel", "TutorialPanel" })
        {
            var panel = Named(name); var surface = panel.GetComponent<Graphic>(); if (surface != null) surface.color = C("#142F34");
            foreach (var child in panel.GetComponentsInChildren<RoundedPanelGraphic>(true))
            {
                if (child.name == "Surface" || child.name.EndsWith("Surface")) child.color = C("#142F34");
                if (child.name == "ProgressTrack") child.color = C("#345053");
            }
            foreach (var text in panel.GetComponentsInChildren<TMP_Text>(true)) text.color = C("#EDE0C3");
        }
        foreach (var button in Object.FindObjectsByType<BookShelfZoomButton>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var surface = button.GetComponent<Graphic>(); if (surface != null) surface.color = C("#203D41");
            var so = new SerializedObject(button); var frame = so.FindProperty("_frameImage").objectReferenceValue as GameObject;
            if (frame != null && frame.GetComponent<RectTransform>() != null)
            {
                foreach (var graphic in frame.GetComponentsInChildren<Graphic>(true))
                    if (graphic is not TMP_Text) graphic.color = C(graphic.name == "Inset" ? "#203D41" : "#BA8C53");
            }
            else
            {
                var accent = button.transform.Find("SelectedAccent")?.gameObject;
                if (accent == null)
                {
                    accent = new GameObject("SelectedAccent", typeof(RectTransform), typeof(Image)); accent.transform.SetParent(button.transform, false);
                    var rect = (RectTransform)accent.transform; rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(0, 1);
                    rect.pivot = Vector2.up; rect.offsetMin = new Vector2(6, 8); rect.offsetMax = new Vector2(10, -8);
                    accent.GetComponent<Image>().color = C("#D9A66B"); accent.GetComponent<Image>().raycastTarget = false;
                }
                accent.SetActive(false); Ref(button, "_frameImage", accent);
            }
        }
        foreach (string name in new[] { "OpenPauseButton", "OpenSettingsButton", "TutorialHelpButton", "BackToRoomButton", "AnswerButton", "JudgeButton", "ReturnShelfBookButton", "CancelShelfMoveButton", "TutorialDismissButton", "TitleButton", "ReplayButton" })
        {
            var go = MaybeNamed(name); if (go == null) continue;
            bool primary = name == "JudgeButton" || name == "ReplayButton";
            var surface = go.GetComponent<Graphic>(); if (surface != null) surface.color = C(primary ? "#D4BB8B" : "#244249");
            foreach (var text in go.GetComponentsInChildren<TMP_Text>(true)) text.color = C(primary ? "#16313A" : "#F5E6C7");
        }
        foreach (var slot in Object.FindObjectsByType<HandItemSlotUI>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            var so = new SerializedObject(slot); var frame = so.FindProperty("_frameImage").objectReferenceValue as GameObject;
            if (frame != null) foreach (var g in frame.GetComponentsInChildren<Graphic>(true)) if (g is not TMP_Text) g.color = C(g.name == "Inset" ? "#244249" : "#CBA66C");
        }
        var saveStatus = (TMP_Text)new SerializedObject(Object.FindFirstObjectByType<GameProgressController>()).FindProperty("_saveStatus").objectReferenceValue;
        saveStatus.color = C("#C6B589");
        // The clear panel uses the live room beneath a quiet dark wash.
        var clearImage = Named("ClearImage").GetComponent<Image>(); clearImage.sprite = null; clearImage.color = new Color(.04f, .10f, .12f, .88f);
        Named("ClearPanel").GetComponent<Image>().color = Color.clear;
        var clearCard = Named("ClearCard"); clearCard.GetComponent<Graphic>().color = C("#142F34");
        foreach (var text in clearCard.GetComponentsInChildren<TMP_Text>(true)) text.color = C("#F5E6C7");
        Named("ReplayButton").GetComponentInChildren<TMP_Text>(true).color = C("#16313A");
    }

    private static Material Mat(string name, string hex, float metallic, float smooth, Texture texture = null, bool transparent = false, bool spine = false, float opacity = .30f)
    {
        string path = Folder + "/" + name + ".mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat == null) { mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); AssetDatabase.CreateAsset(mat, path); }
        var color = C(hex); if (transparent) color.a = opacity;
        mat.SetColor("_BaseColor", color); mat.SetFloat("_Metallic", metallic); mat.SetFloat("_Smoothness", smooth);
        mat.SetTexture("_BaseMap", texture); mat.SetTextureScale("_BaseMap", spine ? Vector2.one : new Vector2(3, 5));
        mat.SetFloat("_Surface", transparent ? 1 : 0); mat.SetFloat("_ZWrite", transparent ? 0 : 1);
        // URP Lit premultiplies the lit color in this mode. Applying SrcAlpha
        // again made the binding/pages almost invisible and flattened the preview.
        mat.SetFloat("_SrcBlend", (float)BlendMode.One);
        mat.SetFloat("_DstBlend", (float)(transparent ? BlendMode.OneMinusSrcAlpha : BlendMode.Zero));
        if (transparent)
        {
            mat.SetFloat("_Blend", 0); // Alpha, with the diffuse alpha multiply in the Lit shader.
            mat.SetFloat("_BlendModePreserveSpecular", 1);
            mat.SetFloat("_ReceiveShadows", 0); mat.EnableKeyword("_RECEIVE_SHADOWS_OFF");
            mat.SetOverrideTag("RenderType", "Transparent"); mat.renderQueue = (int)RenderQueue.Transparent;
            mat.SetShaderPassEnabled("ShadowCaster", false); mat.SetShaderPassEnabled("DepthOnly", false);
        }
        else { mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT"); mat.SetOverrideTag("RenderType", "Opaque"); mat.renderQueue = (int)RenderQueue.Geometry; }
        // Use the same keyword/blend-factor setup as the URP material inspector.
        BaseShaderGUI.SetupMaterialBlendMode(mat);
        mat.enableInstancing = true; EditorUtility.SetDirty(mat); AssetDatabase.SaveAssetIfDirty(mat); return mat;
    }

    private static Texture2D Grain(string name, bool paper)
    {
        string path = Folder + "/" + name + ".asset";
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null)
        {
            texture = new Texture2D(128, 128, TextureFormat.RGBA32, true) { name = name, wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Trilinear };
            AssetDatabase.CreateAsset(texture, path);
        }
        var pixels = new Color32[128 * 128];
        for (int y = 0; y < 128; y++) for (int x = 0; x < 128; x++)
        {
            float value = paper ? (x % 7 == 0 ? .76f : .96f) : .82f + .06f * Mathf.Sin(x * .82f + Mathf.Sin(y * .04f) * .9f) + .025f * Mathf.Sin(x * 3.1f + y * .13f);
            byte v = (byte)Mathf.Clamp(value * 255, 0, 255); pixels[y * 128 + x] = new Color32(v, v, v, 255);
        }
        texture.SetPixels32(pixels); texture.Apply(true, false); EditorUtility.SetDirty(texture); AssetDatabase.SaveAssetIfDirty(texture); return texture;
    }

    private static void BuildBoxContactShadow(Renderer body)
    {
        // A small, feathered floor shadow supplies the contact that a distant
        // directional shadow map cannot resolve. The moving/animated lid keeps
        // its real-time shadow; this mesh only follows the box's floor footprint.
        string texturePath = Folder + "/BoxContactShadow.asset";
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            texture = new Texture2D(64, 64, TextureFormat.RGBA32, false) { name = "BoxContactShadow", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            AssetDatabase.CreateAsset(texture, texturePath);
        }
        var pixels = new Color32[64 * 64];
        for (int y = 0; y < 64; y++) for (int x = 0; x < 64; x++)
        {
            float u = ((x + .5f) / 32 - 1) / .72f, v = ((y + .5f) / 32 - 1) / .72f;
            byte alpha = (byte)(220 * Mathf.Exp(-1.7f * (Mathf.Pow(u, 4) + Mathf.Pow(v, 4))));
            pixels[y * 64 + x] = new Color32(255, 255, 255, alpha);
        }
        texture.SetPixels32(pixels); texture.Apply(false, false); EditorUtility.SetDirty(texture); AssetDatabase.SaveAssetIfDirty(texture);
        string materialPath = Folder + "/BoxContactShadow.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null) { material = new Material(Shader.Find("Universal Render Pipeline/Unlit")); AssetDatabase.CreateAsset(material, materialPath); }
        material.SetTexture("_BaseMap", texture); material.SetColor("_BaseColor", new Color(.025f, .035f, .03f, .35f));
        material.SetFloat("_Surface", 1); material.SetFloat("_Blend", 0); material.SetFloat("_BlendModePreserveSpecular", 0);
        material.SetFloat("_QueueOffset", -10); BaseShaderGUI.SetupMaterialBlendMode(material);
        material.SetShaderPassEnabled("ShadowCaster", false); EditorUtility.SetDirty(material); AssetDatabase.SaveAssetIfDirty(material);
        var geometry = new ShelfCareMeshGeometry(1);
        geometry.Quad(new Vector3(-.5f, 0, -.5f), new Vector3(-.5f, 0, .5f), new Vector3(.5f, 0, .5f), new Vector3(.5f, 0, -.5f), Vector3.up, 0);
        var mesh = SaveMesh(geometry.Mesh("BoxContactShadow")); AssetDatabase.SaveAssetIfDirty(mesh);
        var existing = MaybeNamed("FloorContactShadow"); if (existing != null) Object.DestroyImmediate(existing);
        var shadow = new GameObject("FloorContactShadow");
        var bounds = body.bounds;
        float floor = Named("Floor").GetComponent<Renderer>().bounds.max.y;
        shadow.transform.position = new Vector3(bounds.center.x, floor + .006f, bounds.center.z);
        shadow.transform.localScale = new Vector3(bounds.size.x * 1.18f, 1, bounds.size.z * 1.18f);
        shadow.transform.SetParent(GameObject.Find("GameplayRoom3D").transform, true);
        shadow.AddComponent<MeshFilter>().sharedMesh = mesh;
        var renderer = shadow.AddComponent<MeshRenderer>(); renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off; renderer.receiveShadows = false;
    }

    private static void Board(string name, Transform parent, Vector3 position, Vector3 size, Material material, float bevel = .018f)
    {
        string key = "Box_" + string.Join("_", new[] { size.x, size.y, size.z, bevel }.Select(v => v.ToString("0.###", CultureInfo.InvariantCulture)));
        var mesh = AssetDatabase.LoadAllAssetsAtPath(MeshPath).OfType<Mesh>().SingleOrDefault(m => m.name == key);
        if (mesh == null) { var geometry = new ShelfCareMeshGeometry(1); geometry.Box(Vector3.zero, size, 0, bevel); mesh = SaveMesh(geometry.Mesh(key)); }
        var go = Child(name, parent).gameObject; go.transform.localPosition = position;
        go.AddComponent<MeshFilter>().sharedMesh = mesh; go.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static Mesh SaveMesh(Mesh mesh)
    {
        var existing = AssetDatabase.LoadAllAssetsAtPath(MeshPath).OfType<Mesh>().SingleOrDefault(m => m.name == mesh.name);
        if (existing == null)
        {
            if (AssetDatabase.LoadMainAssetAtPath(MeshPath) == null) AssetDatabase.CreateAsset(mesh, MeshPath); else AssetDatabase.AddObjectToAsset(mesh, MeshPath);
            return mesh;
        }
        existing.Clear(); existing.vertices = mesh.vertices; existing.normals = mesh.normals; existing.uv = mesh.uv;
        existing.subMeshCount = mesh.subMeshCount;
        for (int i = 0; i < mesh.subMeshCount; i++) existing.SetTriangles(mesh.GetTriangles(i), i);
        existing.RecalculateBounds(); existing.UploadMeshData(false); Object.DestroyImmediate(mesh); EditorUtility.SetDirty(existing); return existing;
    }
    private static void ObjectArray(SerializedProperty array, Object[] values) { array.arraySize = values.Length; for (int i = 0; i < values.Length; i++) array.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; }
    private static void Ref(Object target, string field, Object value) { var so = new SerializedObject(target); so.FindProperty(field).objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
    private static Color C(string hex) { ColorUtility.TryParseHtmlString(hex, out var color); return color; }
    private static Transform Child(string name, Transform parent) { var go = new GameObject(name); go.transform.SetParent(parent, false); return go.transform; }
    private static GameObject MaybeNamed(string name) => Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).SingleOrDefault(t => t.name == name)?.gameObject;
    private static GameObject Named(string name) => MaybeNamed(name) ?? throw new InvalidOperationException("Missing " + name);
    private static void EnsureFolder(string parent, string child) { if (!AssetDatabase.IsValidFolder(parent + "/" + child)) AssetDatabase.CreateFolder(parent, child); }
    private static void Point(string name, Transform parent, Vector3 position, string color, float intensity, float range)
    {
        var light = Child(name, parent).gameObject.AddComponent<Light>(); light.type = LightType.Point;
        light.transform.position = position; light.color = C(color); light.intensity = intensity; light.range = range; light.shadows = LightShadows.None;
    }
}
