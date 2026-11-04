// Compare two calls separated by rendered frames in TitleScene or on Clear.
// Back up settings/progress first. This records actual glyph/decoration vertices.
if (!UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Run in Play mode.");
bool title = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "TitleScene";
GameObject Named(string name) => UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include, UnityEngine.FindObjectsSortMode.None).Single(t => t.name == name).gameObject;
var background = Named(title ? "Background" : "ClearImage").GetComponent<RectTransform>();
var brand = Named(title ? "TitleBrand" : "ClearBrand").GetComponent<RectTransform>();
var headline = Named(title ? "TitleHeadline" : "ClearHeadline").GetComponent<TMPro.TMP_Text>();
if (!headline.gameObject.activeInHierarchy) throw new System.Exception("Show the presentation and let a frame render first.");
var character = headline.textInfo.characterInfo.First(c => c.isVisible);
var vertex = headline.textInfo.meshInfo[character.materialReferenceIndex].vertices[character.vertexIndex];
var ambient = UnityEngine.Object.FindFirstObjectByType<AmbientGeometryGraphic>();
var mote = ambient.canvasRenderer.GetMesh().vertices[0];
var music = Named(title ? "TitleBGM" : "GameBGM").GetComponent<AudioSource>();
return new { reduced = GamePreferences.ReducedMotion, bx = background.anchoredPosition.x,
    by = background.anchoredPosition.y, brandY = brand.anchoredPosition.y, glyphY = vertex.y,
    moteX = mote.x, moteY = mote.y, musicSamples = music.timeSamples,
    musicPlaying = music.isPlaying, dirty = GamePreferences.IsDirty };
