# タイトル・クリア・本選択画面

木の本棚、生成りの紙、緑のボタンに表示を揃えています。
文字を含まない背景画像と、Unityで描画する文字・装飾を分離しました。

## 動く演出

- `PresentationMotion`が背景の小さな揺れとマウス位置による視差、見出し全体の
  浮遊、英字一文字ごとの微小な上下移動、表示時のフェードを行います。
- `AmbientGeometryGraphic`は金色の光の粒とゆっくり回る幾何学模様を、
  UIの頂点で描きます。追加の動画やパーティクル素材は使いません。
- 演出はunscaled timeで動き、Time.timeScaleが0でも動作します。
  非表示のクリア画面では更新しません。装飾はクリックを遮りません。
- ボタンは浮遊する文字とは別の階層にあり、クリック位置が動きません。
- 演出を止める場合は、`PresentationMotion`と`AmbientGeometryGraphic`を
  無効にします。前者を無効にすると文字と背景は基準位置に戻ります。

## シーン内の編集場所

タイトルでPlayし、ゲームを開始した後、Unityの
`Tools > Shelf Care > Preview Clear Screen`からクリア演出を確認できます。
このメニューはEditor専用で、ゲーム中だけ有効になります。

- `TitleScene`: `Background`、`TitlePresentation/TitleBrand`、
  `StartButton`、`QuitButton`。英字見出しは`TitleHeadline`です。
- `ShelfCare`: 既存の`ClearPanel`がクリア画面です。`ClearCard/ClearBrand`、
  `CompletedPatternGallery`、`ReplayButton`、`TitleButton`を編集します。
  8種類の完成図にはゲーム中と同じ画像を使用しています。
- クリア時の入力ロック・効果音・フェードは既存の
  `GameClearEffectController`で維持します。以前の単発パーティクルは外し、
  金色のUI装飾で穏やかな演出に置き換えました。
- WebGLでは機能しない終了操作を表示しないため、タイトルの終了ボタンは
  WebGLプレイヤーでは非表示にします。Editorでの終了操作はフェード後に
  入力可能な状態へ戻します。

## 本選択画面

`BookListPanel`に紙の背景と日本語の見出し・閉じるボタンを配置しています。
本は`BookButtonPrefab`の紙のカードに表示します。`BookCover`が画像、
`FrameImage`が選択・ホバー表示です。ボタン本体を押して選択できます。
本の生成、ランダム化、選択上限、スクロール、確認・取り出しの処理は共通です。
色による絞り込み、箱の残り冊数、別の色で選んだ本の維持については`Docs/BookColorFilters.md`を参照してください。
チュートリアルは上部中央寄りに配置し、見出しと重ならないようにしています。

## 素材・フォント

背景はbuilt-in image_genで既存の部屋を編集し、文字を取り除いて幾何学模様の
本を加えたものです。生成プロンプトは`Art/ScenePresentation/prompts.json`、
画像は`Assets/Resources/Title.png`と`ClearImage.png`です。

英字見出しはGoogle FontsのEB Garamondを使用します。
出典: https://github.com/google/fonts/tree/main/ofl/ebgaramond
ライセンスは`Assets/Fonts/Presentation/OFL.txt`に同梱しています。
ビルドにも含まれるよう`Assets/StreamingAssets/Licenses/EBGaramond-OFL.txt`にも配置しています。
ソースTTFはEditor専用フォルダーに置き、ゲーム用には静的なSDFを使用します。
日本語は既存の`BookListJapanese`の文字集合を拡張しています。

検証手順は`Tests/README.md`を参照してください。WebGLビルドとブラウザ上の
操作・性能確認は別途必要です。

保存した進行がある場合の「続きから」と「はじめから」、確認画面については
`Docs/SaveAndContinue.md`を参照してください。
