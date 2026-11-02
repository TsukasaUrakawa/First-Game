# Book list Japanese font

`BookListJapanese.asset` is a static TextMesh Pro atlas for the text in
`Characters.txt` and printable ASCII. Add new Japanese text to the corpus and run
`unity command script eval_file --file Tools/BuildBookListFont.cs` in Edit mode
to rebuild without changing the font asset GUID.

Source: Zen Maru Gothic Medium, Copyright 2021 The Zen Maru Gothic Project Authors,
from https://github.com/google/fonts/tree/main/ofl/zenmarugothic.
SHA-256: `3CFDB98A13571EDE17FCC769F5093A97C38B80A7B9B2AB754A26B4D822092B3B`.

The original TTF is kept in `Editor/`. The static atlas does not depend on a
runtime OS font, runtime glyph generation, or loading the full source TTF.
The SIL Open Font License and copyright notice ship with the game in
`Assets/StreamingAssets/ThirdParty/ZenMaruGothic-OFL.txt`.
