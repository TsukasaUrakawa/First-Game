// Edit mode. Bake the title's serif letters; keep the source TTF editor-only.
if(UnityEditor.EditorApplication.isPlaying) throw new System.Exception("Stop Play first.");
const string path="Assets/Fonts/Presentation/PresentationSerif.asset";
if(UnityEditor.AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(path)!=null) throw new System.Exception("Presentation font already exists.");
var source=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Font>("Assets/Fonts/Presentation/Editor/EBGaramond.ttf");
if(source==null) throw new System.Exception("Import the EB Garamond source first.");
var font=TMPro.TMP_FontAsset.CreateFontAsset(source,96,8,UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,1024,1024,TMPro.AtlasPopulationMode.Dynamic,false);
if(!font.TryAddCharacters(new string(Enumerable.Range(32,95).Select(c=>(char)c).ToArray()),out string missing))throw new System.Exception("Missing glyphs: "+missing);
font.name="PresentationSerif";font.atlasPopulationMode=TMPro.AtlasPopulationMode.Static;
var settings=new UnityEditor.SerializedObject(font);settings.FindProperty("m_ClearDynamicDataOnBuild").boolValue=false;settings.ApplyModifiedPropertiesWithoutUndo();
font.atlasTextures[0].name="PresentationSerif Atlas";font.material.name="PresentationSerif Material";
TMPro.ShaderUtilities.UpdateShaderRatios(font.material);
UnityEditor.AssetDatabase.CreateAsset(font,path);
UnityEditor.AssetDatabase.AddObjectToAsset(font.atlasTextures[0],font);UnityEditor.AssetDatabase.AddObjectToAsset(font.material,font);
font.ReadFontAssetDefinition();UnityEditor.EditorUtility.SetDirty(font);UnityEditor.AssetDatabase.SaveAssets();
return new{path,characters=font.characterTable.Count,mode=font.atlasPopulationMode.ToString(),runtimeSource=font.sourceFontFile!=null};
