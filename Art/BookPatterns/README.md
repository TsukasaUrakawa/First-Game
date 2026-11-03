# 本の幾何学模様

本を正しく並べると、一続きの幾何学模様が完成するゲームです。
位置を示す数字やラベルは絵に入れません。左右で異なる図形、曲線の高さ、
交点や塗りつぶした図形を目印にして、本の位置を判断できるようにします。

`Source/`はbuilt-in image_genで制作した8色の原画です。
承認された最初の太い線の緑の案を基準に、色ごとに異なる模様を用意しました。
制作時のプロンプトは`prompts.json`に保存しています。

原画は18冊の連続した模様です。17冊の棚には先頭17冊分を使います。
原画の余白と背表紙の境界を除き、Unityで同じ比率のスプライトに切り分けます。
ゲームで使う140冊の画像と完成図は、同じ切り分け済み画像から生成します。
既存のスプライト名・GUID・内部IDを保つため、選択、配置、正解判定の参照が
そのまま対応します。原画はAssets外にあり、ビルドには含まれません。

再生成はUnity EditorのEditモードで実行します。

```powershell
unity command script eval_file --file Tools/BuildBookPatternAssets.cs --json
unity command script eval_file --file Tests/BookPatternAssetSmoke.cs --json
```

生成先は`Assets/Resources/Books/Book1.png`～`Book8.png`と、
`Books1.png`～`Books8.png`です。背表紙を個別に編集した場合も完成図を再生成してください。
