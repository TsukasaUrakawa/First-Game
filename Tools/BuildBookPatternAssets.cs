// Edit mode: unity command script eval_file --file Tools/BuildBookPatternAssets.cs --json
// Cuts approved artwork into sprites and builds each answer from those SAME pieces.
if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Stop Play first.");
string[] colors = { "Green", "Blue", "Beige", "Red", "Purple", "Brown", "White", "Black" };
int[] counts = { 18, 17, 18, 17, 18, 17, 18, 17 };
const int sourceSpines = 18; // Source sheets contain 18; 17-book shelves use the first 17.
const int width = 144, height = 576, padding = 4;
var factory = new UnityEditor.U2D.Sprites.SpriteDataProviderFactories(); factory.Init();
var results = new System.Collections.Generic.List<object>();
var backup = ".utmp/book-pattern-backup"; System.IO.Directory.CreateDirectory(backup);
for (int colorIndex = 0; colorIndex < colors.Length; colorIndex++)
{
    string color = colors[colorIndex]; int count = counts[colorIndex];
    string sourcePath = $"Art/BookPatterns/Source/{color}.png";
    string bookPath = $"Assets/Resources/Books/Book{colorIndex + 1}.png";
    string answerPath = $"Assets/Resources/Books/Books{colorIndex + 1}.png";
    foreach (string path in new[] { bookPath, answerPath, bookPath + ".meta", answerPath + ".meta" })
    {
        string saved = System.IO.Path.Combine(backup, System.IO.Path.GetFileName(path));
        if (!System.IO.File.Exists(saved)) System.IO.File.Copy(path, saved);
    }
    var importer = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(bookPath);
    var provider = factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
    var rects = provider.GetSpriteRects().OrderBy(r => r.name).ToArray();
    if (rects.Length != count || !rects.Select(r => r.name).SequenceEqual(Enumerable.Range(1, count).Select(i => color + i.ToString("00"))))
        throw new System.Exception("Unexpected sprite names/count: " + bookPath);
    var references = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(bookPath).OfType<UnityEngine.Sprite>().ToDictionary(s => s.name, s =>
    {
        UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(s, out string guid, out long id); return guid + ":" + id;
    });
    var source = new UnityEngine.Texture2D(2, 2, UnityEngine.TextureFormat.RGBA32, false);
    var atlas = new UnityEngine.Texture2D(count * (width + padding) + padding, height + padding * 2, UnityEngine.TextureFormat.RGBA32, false);
    var answer = new UnityEngine.Texture2D(count * width, height, UnityEngine.TextureFormat.RGBA32, false);
    try
    {
        if (!UnityEngine.ImageConversion.LoadImage(source, System.IO.File.ReadAllBytes(sourcePath))) throw new System.Exception("Cannot load " + sourcePath);
        var pixels = source.GetPixels32();
        // Trim plain outer margins, retaining the complete height of the bindings.
        var occupiedRows = Enumerable.Range(0, source.height).Where(y =>
            Enumerable.Range(0, source.width).Count(x =>
            {
                var c = pixels[y * source.width + x];
                return UnityEngine.Mathf.Max(c.r, c.g, c.b) > 16 && UnityEngine.Mathf.Min(c.r, c.g, c.b) < 235;
            }) > source.width * .7f).ToArray();
        int bottom = occupiedRows.First() + 4, top = occupiedRows.Last() - 4;
        // The dark, full-height binding seams are much darker than crossing gold lines.
        float ColumnLight(int x)
        {
            float total = 0;
            for (int y = source.height / 5; y < source.height * 4 / 5; y++)
            {
                var c = pixels[y * source.width + x]; total += (c.r + c.g + c.b) / 3f;
            }
            return total;
        }
        var cuts = new int[sourceSpines + 1]; cuts[0] = 2; cuts[sourceSpines] = source.width - 2;
        for (int i = 1; i < sourceSpines; i++)
        {
            float expected = i * source.width / (float)sourceSpines;
            cuts[i] = Enumerable.Range(0, source.width)
                .Where(x => UnityEngine.Mathf.Abs(x - expected) < source.width / (float)sourceSpines * .18f)
                .OrderBy(ColumnLight).First();
        }
        atlas.SetPixels32(new UnityEngine.Color32[atlas.width * atlas.height]);
        for (int i = 0; i < count; i++)
        {
            int left = cuts[i] + 2, right = cuts[i + 1] - 2;
            if (right - left < source.width / sourceSpines * .7f) throw new System.Exception("Narrow book crop: " + color + i);
            var piece = new UnityEngine.Color32[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    piece[y * width + x] = source.GetPixelBilinear(
                        (UnityEngine.Mathf.Lerp(left, right - 1, x / (float)(width - 1)) + .5f) / source.width,
                        (UnityEngine.Mathf.Lerp(bottom, top, y / (float)(height - 1)) + .5f) / source.height);
            int atlasX = padding + i * (width + padding);
            atlas.SetPixels32(atlasX, padding, width, height, piece);
            answer.SetPixels32(i * width, 0, width, height, piece);
            rects[i].rect = new UnityEngine.Rect(atlasX, padding, width, height);
        }
        atlas.Apply(); answer.Apply();
        System.IO.File.WriteAllBytes(bookPath, UnityEngine.ImageConversion.EncodeToPNG(atlas));
        System.IO.File.WriteAllBytes(answerPath, UnityEngine.ImageConversion.EncodeToPNG(answer));
        provider.SetSpriteRects(rects); provider.Apply();
        importer.npotScale = UnityEditor.TextureImporterNPOTScale.None; importer.maxTextureSize = 4096;
        importer.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        UnityEditor.AssetDatabase.ImportAsset(answerPath, UnityEditor.ImportAssetOptions.ForceUpdate);
        var answerImporter = (UnityEditor.TextureImporter)UnityEditor.AssetImporter.GetAtPath(answerPath);
        answerImporter.npotScale = UnityEditor.TextureImporterNPOTScale.None; answerImporter.maxTextureSize = 4096;
        answerImporter.textureCompression = UnityEditor.TextureImporterCompression.Uncompressed;
        answerImporter.SaveAndReimport();
        var after = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(bookPath).OfType<UnityEngine.Sprite>().ToArray();
        foreach (var sprite in after)
        {
            UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(sprite, out string guid, out long id);
            if (references[sprite.name] != guid + ":" + id) throw new System.Exception("Sprite reference changed: " + sprite.name);
        }
        results.Add(new { color, count, preservedReferences = after.Length, sourceBounds = new { bottom, top, cuts } });
    }
    finally
    {
        UnityEngine.Object.DestroyImmediate(source); UnityEngine.Object.DestroyImmediate(atlas); UnityEngine.Object.DestroyImmediate(answer);
    }
}
UnityEditor.AssetDatabase.SaveAssets();
return results;
