// Edit mode. Verifies all playable pieces agree with their displayed completion picture.
if (UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Stop Play first.");
string[] colors = { "Green", "Blue", "Beige", "Red", "Purple", "Brown", "White", "Black" };
int checks = 0, total = 0;
void Check(bool ok, string label) { if (!ok) throw new System.Exception("FAIL: " + label); checks++; }
for (int c = 0; c < colors.Length; c++)
{
    int count = c % 2 == 0 ? 18 : 17;
    string bookPath = $"Assets/Resources/Books/Book{c + 1}.png";
    var sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(bookPath).OfType<UnityEngine.Sprite>().OrderBy(s => s.name).ToArray();
    Check(sprites.Select(s => s.name).SequenceEqual(Enumerable.Range(1, count).Select(i => colors[c] + i.ToString("00"))), colors[c] + " identities and count");
    var atlas = new UnityEngine.Texture2D(2, 2); var answer = new UnityEngine.Texture2D(2, 2);
    try
    {
        UnityEngine.ImageConversion.LoadImage(atlas, System.IO.File.ReadAllBytes(bookPath));
        UnityEngine.ImageConversion.LoadImage(answer, System.IO.File.ReadAllBytes($"Assets/Resources/Books/Books{c + 1}.png"));
        Check(answer.width == count * 144 && answer.height == 576, colors[c] + " full completion picture dimensions");
        foreach (var sprite in sprites)
        {
            int order = int.Parse(sprite.name.Substring(colors[c].Length)) - 1;
            var rect = sprite.rect;
            Check(rect.width == 144 && rect.height == 576, sprite.name + " readable spine proportions");
            foreach (int x in new[] { 0, 17, 47, 71, 103, 143 })
                foreach (int y in new[] { 0, 43, 117, 239, 383, 511, 575 })
                    Check(atlas.GetPixel((int)rect.x + x, (int)rect.y + y) == answer.GetPixel(order * 144 + x, y), sprite.name + " matches completion picture");
            Check(atlas.GetPixel((int)rect.x - 1, (int)rect.y + 100).a == 0, sprite.name + " isolated atlas gutter");
            total++;
        }
    }
    finally { UnityEngine.Object.DestroyImmediate(atlas); UnityEngine.Object.DestroyImmediate(answer); }
}
Check(total == 140, "All 140 books replaced");
return new { passed = checks, books = total, colors = colors.Length };
