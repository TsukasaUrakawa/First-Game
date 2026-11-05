# タイトル・クリア・本選択画面

タイトルは深い青緑の空間に立体の本を並べ、生成りの文字と銅色の模様で見せます。
本編は同じ配色の立体の本棚と本、クリアは立体の部屋に暗い表示を重ねる構成です。
本選択・確認・設定は、読みやすい紙色のカードを使います。
タイトルの本・模様・文字・操作ボタンはそれぞれ独立したオブジェクトです。

## 立体のタイトル

- `TitleBookStage/Composition/Books`の26冊は、厚み・平行な表紙・紙の小口を持つ
  通常の直方体の本です。同じ本体メッシュを共有し、変形や非等方スケールを使いません。
  画像案のくさび形の本は使わず、高さをずらした波状の列として構成しています。
- 背表紙の模様はタイトル専用の放射状の図形と平行線です。本編の正解画像を使いません。
  波の両端で線がつながるよう本ごとのローカル座標に焼き付けてあります。
- `TitleBookSculpture`が、開始時に本を一冊ずつ奥へ収めます。その後は22秒周期で
  右端の3冊を順に手前へ取り出し、待機位置へ移動して、逆順に戻します。
  横へ移動する前に本を引き出すため、隣の本を突き抜けません。
- 本体・印刷は編集時に生成してシーンへ保存しています。実行中のメッシュ生成や
  コライダーはありません。床・ライト・カメラもタイトルシーン内で編集できます。
- `TitleBackdropGraphic`は左側の文字の後ろへ薄い暗色のグラデーションを描きます。
  ボタンは固定位置に置き、立体の本はウィンドウの縦横比に応じて右側へ収めます。
- 「画面の動きを控えめに」では、最初から全冊が揃った状態を表示し、
  取り出し・浮遊・文字・装飾の動きを止めます。設定を戻すと待機演出を再開します。

`Tools > Shelf Care > Rebuild Title Book Sculpture`は、保存済みのTitleSceneを開いた
Editモードで実行します。本・印刷メッシュ・素材・照明・タイトルの配置を再生成します。
生成コードは`Assets/Editor/TitleBookStageBuilder.cs`、共有素材とメッシュは
`Assets/Presentation/TitleBooks`です。再生成時も既存の素材GUIDとメッシュ参照を保ちます。

タイトルと本編で共有する配色基準（本編の構成は`Docs/3DGameplay.md`）:

| 用途 | 色 |
| --- | --- |
| 空間・文字の下地 | `#0D242A` / `#091C22` |
| 布の青緑 | `#173A40` |
| 差し色の朱 | `#913E29` |
| 紙と主ボタン | `#F5E6C7` |
| 模様・縁の銅色 | `#D9A66B` |
| 控えめな文字 | `#BEC7BC` |

## 動く演出

- `PresentationMotion`が背景の小さな揺れとマウス位置による視差、見出し全体の
  浮遊、英字一文字ごとの微小な上下移動、表示時のフェードを行います。
- `AmbientGeometryGraphic`は金色の光の粒とゆっくり回る幾何学模様を、
  UIの頂点で描きます。追加の動画やパーティクル素材は使いません。
  タイトルでは粒だけを控えめに表示し、幾何学の主役は立体の本に置いています。
- 演出はunscaled timeで動き、Time.timeScaleが0でも動作します。
  非表示のクリア画面では更新しません。装飾はクリックを遮りません。
- ボタンは浮遊する文字とは別の階層にあり、クリック位置が動きません。
- タイトルまたはゲーム中の「設定」で「画面の動きを控えめに」を選ぶと、
  背景・文字を基準位置に戻し、装飾も静止させます。設定はクリア画面にも反映します。
  詳しくは`Docs/AudioMotionSettings.md`を参照してください。

## シーン内の編集場所

タイトルでPlayし、ゲームを開始した後、Unityの
`Tools > Shelf Care > Preview Clear Screen`からクリア演出を確認できます。
このメニューはEditor専用で、ゲーム中だけ有効になります。

- `TitleScene`: `TitleBookStage`、`Background`、`TitlePresentation/TitleBrand`、
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

旧タイトル背景と現在のクリア背景はbuilt-in image_genで既存の部屋を編集したものです。
生成プロンプトは`Art/ScenePresentation/prompts.json`、画像は
`Assets/Resources/Title.png`と`ClearImage.png`です。現在のタイトルは画像を背景に使いません。

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
