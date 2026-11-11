# Shelf Care — プロジェクト引き継ぎ

最終確認日: **2026-11-12（日本時間）**

この文書は、参照できた会話履歴、ローカルのソース・シーン・設定・開発資料・Git履歴を照合して作成した。別のCodex環境でも読めるよう、ファイルパスは原則プロジェクトルートからの相対パスで記載する。元の作業場所は `C:\UnityProjects\ShelfCare`。

「コードで確認した実装」「過去の検証記録」「ユーザーの報告」「未確認」を区別する。本書作成時にUnityのPlay、テスト、ビルド、公開サイトの再検証は実施していない。過去の成功記録を現在の環境での実行結果として扱わないこと。

## 1. 目的と現在地

**本を並べ、背表紙に描かれた線や図形をつなげて幾何学模様を完成させるパズルゲーム。** 単に色や番号で分類するゲームではなく、模様を読み取って並べることが中心のコンセプト。

- 表示名: **Shelf Care**。
- 公開先: [unityroomのゲームページ](https://unityroom.com/games/shelf-care)。ユーザーは2026-11-09に公開完了を報告。会話には当時の公開ページとアイコン表示の確認記録もある。本書作成時点のサイト状態は再確認していない。
- 以前の平面的な表現から、タイトル・本編ともに立体の本と本棚を使う表現へ移行済み。UIはuGUIを使用する。
- 棚は **2列×4段、8色、合計140冊**。手持ちは **最大9冊**。
- 日本語UI、取り出し確認、色フィルター、移動・交換・返却、完成図、チュートリアル、保存・続きから、設定、中断機能まで実装済み。
- 現在の `main` はPR #19（WebGL公開準備）までマージ済み。公開準備をこれから始める状態ではない。
- 会話とコードから、現在着手中と断定できる未完了の機能実装はない。残る確認事項は「11. 未完了・未確認事項」を参照。

### 確認時のGit状態

| 項目 | 内容 |
| --- | --- |
| 本体リポジトリ | `https://github.com/TsukasaUrakawa/First-Game.git` |
| ブランチ | `main`。ローカル参照上は `origin/main` と同じ位置。今回fetchはしていない |
| HEAD | `71625dacff1b02eb7b49e3e5285d0954d4cf814c` |
| コミット | `Merge pull request #19 from TsukasaUrakawa/codex/webgl-release-prep` |
| 作業開始前からの変更 | `Assets/Settings/Mobile_RPAsset.asset`、`ProjectSettings/ProjectSettings.asset` |

この2件の設定変更は本書作成前から存在する。前者にはURPのprefilter関連フラグの差分がある。後者はGitでは変更扱いだが、確認したテキストdiffには差分が出なかった。変更の発生理由や意図は未確認。勝手に破棄・上書き・コミットしないこと。

## 2. ユーザーとの進め方

ユーザーは **SourceTreeの操作を基本的に自分で行う** こと、**PRは1件ずつ作り、レビューを待つ** ことを希望している。

通常の実装サイクル:

1. 前のPRがマージされたことを確認する。
2. ユーザーがUnityを閉じ、SourceTreeで `main` をPullする。
3. ユーザーが `main` から `codex/<作業名>` を作成・チェックアウトする。
4. ユーザーがUnityを開き、「準備できました」と伝える。
5. Codexが現在のブランチ・既存差分を確認し、合意した範囲を実装・検証する。
6. ユーザーが動作を確認し、SourceTreeでコミット・Pushする。
7. Push後にCodexが対象差分を確認してPRを作成する。
8. ユーザーのレビュー・マージと `main` のPullを待ち、次の作業へ進む。

- 「準備できました」だけでブランチ切り替えを推測しない。過去には実際には `main` のままだったことがある。
- ブランチ作成、コミット、Push、マージ、PullをCodex側で先回りしない。今回の依頼は引き継ぎ文書作成であり、これらのGit操作は含まない。
- 必要な質問は適宜行う。大きな見た目の変更は案を提示して確認する。タイトル背景では、ユーザーから **実装前に案の承認を取る** よう明示されている。
- ユーザーが手作業で調整したシーン・オブジェクトを尊重する。特に段ボールの回転と大きさはユーザー自身が修正した経緯がある。
- 公開告知用のX投稿ではGitHub URLを載せることへの懸念が示されている。一般向けの案内にはunityroomのURLを使う。

## 3. 決定済みのゲーム仕様

### 本・棚・正解の対応

対応表の実装は `Assets/Scripts/BookColorUtility.cs`。インデックスは全棚を通した0始まり。

| 色番号 | Sprite名の接頭辞 | 冊数 | 正解インデックス |
| --- | --- | ---: | --- |
| 0 | `Green` | 18 | 0–17 |
| 1 | `Blue` | 17 | 18–34 |
| 2 | `Beige` | 18 | 35–52 |
| 3 | `Red` | 17 | 53–69 |
| 4 | `Purple` | 18 | 70–87 |
| 5 | `Brown` | 17 | 88–104 |
| 6 | `White` | 18 | 105–122 |
| 7 | `Black` | 17 | 123–139 |

- 永続的な本の識別子は `Green01` などの **Sprite名**。接頭辞と2桁の番号の組み合わせを使う。
- `BookShelf01`〜`BookShelf08` の名前を色判定に使い、`Slot1` などの名前から `BookSlot.SlotIndex` を求める。見た目だけの名前ではない。
- 棚の直下にあるスロットを `SlotIndex` 順に直線配置する。Hierarchyの並び順は正解順を決めない。
- 本は同じ色の棚にのみ置ける。棚内の誤った順番は許容し、プレイヤーが並べ直す。
- 「並びをチェック」で正解の棚が完成・ロックされる。配置や入れ替えのたびに自動判定する仕様ではない。全棚の正しい配置を判定するとクリアする。
- 本の画像の **Sprite名・GUID・内部ID・切り出し領域と正解番号の対応を保持** してきた。変更時にはセーブ互換性と全140冊への影響を検討する。

### 取り出し・手持ち・並べ替え

- 段ボールから本一覧を開く。最初のカード生成時に一覧をランダム化する。
- 一覧には「すべて」と8色のフィルター、残り冊数、選択数、確認・選択解除の操作がある。
- 選択上限は **手持ちの空き数**。手持ちと合わせて9冊を超えない。
- 色フィルターを変えても、隠れた色の選択と選択順は保つ。確認・解除は全色の選択に作用する。
- 残り冊数は箱に残る本の数。確定前に選んだ本はまだ箱に含める。取り出した本・配置済みの本は一覧から除く。
- 確認画面では選んだ本と受け取り後の手持ち数を表示する。「選び直す」やEscapeでは選択を維持して一覧に戻る。一覧自体を閉じると未確定の選択を解除する。
- 手持ちは画面下部の9枠。選択中の本を再クリックすると解除できる。配置後はその本を手持ちから取り除き、選択も解除する。返却した本は手持ちの末尾へ入る。
- 配置済みの本をクリックして選択し、同じ棚の空き位置をクリックすると移動、本をクリックすると交換する。部屋から本をクリックした場合は該当棚を拡大する。
- 完成済みの棚は編集不可。他の色・他の棚への移動も不可。
- 手持ちが9冊でも棚内の移動・交換は可能。手持ちへの返却は空きが必要。
- 手持ちの選択と棚上の本の選択は排他的。棚の選択中にEscape・右クリックを押した場合、まず選択を解除する。

### 画面・操作・中断

- 左の棚カードは実際の棚と同じ2列×4段。色名と収納冊数を表示し、1クリックで拡大する。冊数は正解数ではない。
- 拡大中は棚名・冊数・完成図・並びのチェックを上部に表示し、チェック後も拡大を維持する。
- 完成図は紙色のカードと木枠で表示し、閉じると元の棚へ戻る。
- Escapeは手前の画面から1段ずつ戻る。部屋では中断、拡大では部屋へ戻る。本の選択・確認・一覧・完成図・設定・中断の各状態を優先して処理する。
- 設定や中断、一覧、完成図、クリアなどの表示中は背後の本・棚への入力を遮断する。見た目を隠すだけでは不十分。
- 中断の「遊びに戻る」は元の画面、カメラ、選択、スクロール、フィルター、案内を保つ。「保存してタイトルへ」は進行と設定の保存成功を確認してから移動する。失敗時は画面を残して再試行・続行できる。
- 中断中は `Time.timeScale = 0`。復帰・シーン終了・Play停止時に元の値を戻す。BGMは再生を続ける。
- 中断と設定は同時に開かない。クリア後は中断を出さない。
- 右上の中断・設定の列、棚の操作バー、チュートリアルを別の段に置く。一覧表示中は中断ボタンの上辺を閉じるボタンに合わせる。
- 解像度・アスペクト比によってUIと本・段ボールのクリック領域が重ならないことを重視する。

### チュートリアル・設定

- 初回は、段ボール → 本の選択 → 確認 → 取り出し → 手持ちの選択 → 同色の棚の拡大 → 配置、の7段階で案内する。
- 注目対象の **オブジェクト自体を緑色に点滅** させ、周囲を暗くする。オレンジ枠や囲み枠は不要、というユーザーの明示的な希望がある。
- 正解位置を教える案内ではない。進行は実際のゲーム状態を見て更新する。
- スキップまたは初回配置成功で既読を保存。案内の再表示は現在のゲーム状態から行い、ゲームを初期化しない。
- 設定はBGM音量・効果音音量・ミュート・効果音の試聴・「画面の動きを控えめに」。進行とは独立して保存し、新規開始でも維持する。
- 動きを控えめにすると、背景・文字・タイトルの本の演出を静止し、案内や選択の緑色も一定にする。
- クリック音は共通処理を使うが、専用の音があるボタンに共通音を重ねない。音量スライダーは連続クリック音を発生させない。

## 4. 見た目・素材の設計方針

### コンセプト上の重要な経緯

- ユーザーは「何番目の本かわかりやすくしたい」と希望したが、番号などで幾何学模様のコンセプトが失われる案は却下した。
- 最終的に **最初の太い線の案** を基準として8色へ展開することに合意。左右で異なる図形、曲線の高さ、交点、塗りつぶしなどで区別する。数字・位置ラベルを画像へ焼き込まない。
- タイトル背景はインパクトを重視し、一枚絵にこだわらずシーン内の立体オブジェクトで構成する方向を承認した。本編の正解模様をタイトルへそのまま流用する案は好まれていない。
- タイトルの本は現実的な直方体の形にする。画像案で見られた、現実には不自然なくさび形などにはしない。
- タイトルを3Dにした後、本編も同じ雰囲気の3D本棚・本にする変更を承認し、実装済み。

### 現在の表現

- 深い青緑、落ち着いた木、生成りの紙・文字、金／銅色の線と縁を共通基準とする。
- 基準色: 空間 `#0D242A` / `#091C22`、青緑 `#173A40`、朱 `#913E29`、紙 `#F5E6C7`、銅 `#D9A66B`、補助文字 `#BEC7BC`。
- タイトルは26冊の立体の本。タイトル専用の放射状・平行線模様を使う。開始時に順番に収まり、その後22秒周期で右端の3冊を取り出して戻す。
- 本編は本体・紙の厚みを持つ3Dの本に、元の背表紙画像をUVで貼る。模様を比較しやすいようカメラは **平行投影**。棚の拡大時にもわずかに立体の厚みが見える。
- 配置ゴーストも3D。場所ごとに初回生成して再利用し、Colliderを持たない。メニュー中やホバー終了時に隠す。
- 段ボールの接地は見えるメッシュの底面を基準にし、大きめのクリック用Colliderを基準にしない。接地影は床上の独立オブジェクトで、箱の回転・拡大を二重に受けない。
- 箱のユーザー調整済みの回転・大きさは部屋再生成時にも維持する。カメラ側で位置・画角を調整し、拡大中は箱と接地影を隠す。
- 左18冊／右17冊に合わせて棚の内幅を調整済み。端の大きな余白は除去し、接触回避の0.02単位だけ残す。
- クリアは独立したシーンではなく、`ShelfCare` 内の `ClearPanel`。8色の完成図と文字・金色のUI装飾を表示する。

## 5. コードとアセットの構成

有効なビルドシーンは `Assets/Scenes/TitleScene.unity` と `Assets/Scenes/ShelfCare.unity` の2つ。

| 主な場所・クラス | 責務／変更時の入口 |
| --- | --- |
| `Assets/Scripts/TitleSceneController.cs` | 開始・続きから・新規確認・設定との入力制御・タイトル遷移。WebGLでは終了ボタンを隠す |
| `GameplayUIController.cs` | 共有HUD、モーダル、戻る操作、ワールド入力ガードの調整役。各パネルの表示自体は個別Controllerも担当 |
| `BookManager.cs` / `BookColorUtility.cs` | `Resources/Books` の本を読み込み、8色・140冊の識別と番号を対応付ける |
| `BookListUI.cs` / `BookColorFilterUI.cs` | 一覧カードの生成・キャッシュ・色絞り込み・残数。フィルターごとにカードを作り直さない |
| `BookSelectionController.cs` / `BookListControls.cs` / `BookPopUpController.cs` | 未確定の選択、上限、確認と取り出し |
| `HandItemController.cs` / `HandInventoryUI.cs` / `HandItemSlotUI.cs` | 9冊の手持ち、順番、選択、表示 |
| `HandBookPlacementController.cs` / `BookSlot.cs` / `BookObject.cs` | 配置・返却、正解インデックス、配置済み参照・親・Colliderの更新 |
| `ShelfBookMoveController.cs` / `ShelfBookMoveUI.cs` / `PlacedBookClick.cs` | 配置済みの本の選択と移動・交換 |
| `BookShelfJudge.cs` / `BookJudgeManager.cs` | 棚の正解判定・完成ロック、全体クリア |
| `BookShelfZoomButton.cs` / `ShelfActionMenuController.cs` / `ShelfAnswerPanelController.cs` | 棚選択、拡大時の操作、完成図 |
| `BookShelfSlotAligner.cs` / `Assets/Editor/BookShelfSlotAlignerEditor.cs` | スロットを番号順・等間隔で整列。Edit操作はUndo対応 |
| `CameraController.cs` | 平行投影、画面比率に応じた部屋・棚・段ボールの配置と表示範囲 |
| `BookVisualLibrary.cs` / `BookVisual3D.cs` | 共有メッシュ・色別素材・背表紙UV、個別の緑色強調 |
| `GameProgressData.cs` / `GameProgressStore.cs` / `GameProgressController.cs` | 保存形式・検証、PlayerPrefs入出力、シーンへの復元と自動保存 |
| `GamePreferences.cs` / `SettingsPanelController.cs` / `SceneAudioSettings.cs` | 独立した設定保存、設定画面、音量・ミュート・共通クリック音 |
| `PauseMenuController.cs` / `SceneButtonController.cs` | 中断・保存してタイトルへ・再開・リプレイ・シーン遷移 |
| `BookPlacementTutorial.cs` / `TutorialFocusEffect.cs` / `TutorialSpotlightGraphic.cs` | 状態に応じた案内、緑色の強調、周囲の暗転 |
| `TitleBookSculpture.cs` / `PresentationMotion.cs` / `AmbientGeometryGraphic.cs` | 立体タイトル、文字・背景・装飾の動き |
| `GameClearEffectController.cs` | クリア時の即時入力ロック、効果音、パネルの遅延・フェード |
| `Assets/Editor/GameplayRoom3DBuilder.cs` / `ShelfCareMeshGeometry.cs` | 本編の部屋・本・素材・ゴーストを編集時に生成 |
| `Assets/Editor/TitleBookStageBuilder.cs` | タイトルの本・専用模様・素材・照明の編集時生成 |
| `Assets/Editor/UnityroomBuild.cs` | 公開用設定、ビルド待機、生成物検証、結果JSON |

表のファイル名のみの項目は `Assets/Scripts/` 配下。ただし明示したEditorファイルは除く。

### 設計上守ること

- **uGUIの一覧やカードと、ワールドの棚の配置を混同しない。** 当初はSplineからGrid Layout Groupへの置換が相談されたが、現在の棚は3D Transformと `BookShelfSlotAligner`。3Dの本をCanvasのGridに置き換える方針は確定していない。
- 本のメッシュ・素材・背表紙UVは共有し、実行中に配置ごとの画像コピーやメッシュ再生成をしない。選択色は `MaterialPropertyBlock` を使い、共有素材を変更しない。
- 移動・交換では、両方の本・親・位置・クリック先・Colliderを確定してから変更を通知する。途中の状態を保存・判定させない。
- `BookShelfJudge.StateChanged` 等のイベントで冊数やUI、保存を更新する。既存のキャッシュを無視して毎フレーム全探索する処理を増やさない。
- 一時的な中断開閉の `PauseChanged` は通常の `StateChanged` と分けてあり、棚上の本の選択を維持するために必要。
- モーダル表示ではポインターだけでなく背後のキーボード操作・スクロールも遮る。装飾や保存通知は不要なRaycastを遮らない。
- 音源を実行中に追加する場合は `SceneAudioSettings` への登録方法を確認する。初期音量を基準にして掛け合わせているため、設定を繰り返すたびに音量を重複乗算しない。

## 6. セーブデータと互換性

| PlayerPrefsキー | 用途 |
| --- | --- |
| `ShelfCare.Progress.v1` | 進行のJSON（version 1） |
| `ShelfCare.Progress.v1.backup` | 前回の正常な進行のコピー |
| `ShelfCare.Settings.v1` | 音量・ミュート・動きを控えめにする設定（version 1） |
| `ShelfCare.BookPlacementTutorial.v1` | チュートリアル既読の整数値 |

- 進行JSONは `version`、手持ち順 `hand`、本とスロット番号の組 `placed`、判定済みの `completedShelves`、`cleared`。座標、UnityのインスタンスID、描画コンポーネントは保存しない。
- 未確定の一覧選択、色フィルター、スクロール、手持ち・棚上の選択、開いたパネル、拡大位置は保存しない。再開は部屋から。クリア済みの進行はクリア画面を復元する。
- 重複、存在しない本、9冊超過、スロット範囲、棚の色、完成フラグとの整合性などを復元前に検証する。現行版では本の識別名の変更はセーブ破壊につながる。
- primaryが不正ならbackupを読む。両方不正なら、明示的な新規開始まで壊れた記録を自動上書きしない。
- `GameProgressController` は通常初期化後に復元し、変更を同一フレームの `LateUpdate` でまとめて保存する。フォーカス喪失・アプリ一時停止・終了等でも保存する。
- 新規開始はタイトルの確認後、ゲーム初期化時に進行を置き換える。確認を開く・キャンセルするだけでは削除しない。チュートリアル既読と設定は維持する。
- APIで検出できた保存失敗は表示し、直前の値を保つ。ブラウザの非同期ディスク書き込み完了をAPI呼び出しだけで保証できるわけではない。
- Webの保存先はブラウザのIndexedDBで、Editorの保存とは別。端末間同期やブラウザが消した保存の復旧機能は実装として確認できない。
- 現在のEditor側設定は companyName=`DefaultCompany`、productName=`FirstGame`。公開ビルド時だけproductNameを `Shelf Care` にし、終了時に戻している。名前の恒久変更は既存Editor保存領域への影響を考慮する。

## 7. 編集・再生成時の注意

### 幾何学模様

- 原画8色と制作時プロンプト: `Art/BookPatterns/Source/`、`Art/BookPatterns/prompts.json`。画像生成で制作した素材であり、第三者ストアの本画像ではない。
- 再生成: `Tools/BuildBookPatternAssets.cs`。詳細は `Art/BookPatterns/README.md`。
- 出力は `Assets/Resources/Books/Book1.png`〜`Book8.png` と完成図 `Books1.png`〜`Books8.png`。
- 原画は18冊分、17冊の棚には先頭17冊分を使う。完成図と背表紙は同じ切り出し結果から作る。
- 模様を編集したら完成図も生成し直し、既存Spriteの識別・参照を保持する。

### 3Dシーン

- 本編: 保存済みの `ShelfCare` をEditモードで開き、`Tools > Shelf Care > Rebuild 3D Gameplay Room`。
- タイトル: 保存済みの `TitleScene` をEditモードで開き、`Tools > Shelf Care > Rebuild Title Book Sculpture`。
- 生成先: `Assets/Presentation/Gameplay3D/`、`Assets/Presentation/TitleBooks/`。本のPrefabは `Assets/Prefabs/BookPrefabs/`。
- 再生成は定義済みの木枠・素材・配置・Prefab・照明等を再構成する。手動の見た目調整は、必要に応じてBuilder側にも反映してから再生成する。段ボールの回転・大きさの保持処理は削らない。
- 棚スロットの位置・間隔は各 `BookShelf01`〜`08` の `BookShelfSlotAligner` で調整する。Play開始時にも整列されるため、スロットTransformだけを手動で動かしても恒久設定にはならない。
- クリア演出の確認はPlay中の `Tools > Shelf Care > Preview Clear Screen`。Editor専用メニューであり、実際の全棚クリア検証の代わりにはしない。

### フォント・日本語UI

- 日本語: `Assets/Fonts/BookList/BookListJapanese.asset`。元TTFはEditorフォルダーのZen Maru Gothic Medium。
- 文言追加時は `Assets/Fonts/BookList/Characters.txt` を更新し、Editモードで `Tools/BuildBookListFont.cs` を実行して静的SDFを再生成する。文字列だけの修正では欠字が起きる。
- 英字見出し: EB Garamond、`Assets/Fonts/Presentation/PresentationSerif.asset`。生成は `Tools/BuildPresentationFont.cs`。
- 日本語フォントの安定化、選択画面の固定操作部、効果音試聴とスライダーの重なり対策は既に実装済み。RectTransformの上下余白は符号によって想定外に伸びるため、実際のドラッグ位置とRaycastも確認する。

## 8. 別環境への導入

| 項目 | 確認した値・要件 |
| --- | --- |
| Unity | **6000.0.75f1**、revision `26349cd2a5c8` |
| Webビルド | 対応EditorのWeb Build Supportが必要 |
| URP | `com.unity.render-pipelines.universal` **17.0.4** |
| uGUI / TMP | `com.unity.ugui` **2.0.0** |
| Input System | **1.19.0**。既存コードには `UnityEngine.Input` もあり、現在のActive Input HandlingはBoth（値2） |
| Unity Pipeline | `com.unity.pipeline` **0.9.1-exp.1** |
| Test Framework | **1.6.0**。後述のスモークスクリプトが通常のTest Runnerテストになるわけではない |

その他の正確な依存は `Packages/manifest.json` / `packages-lock.json` を参照する。パッケージがあるだけで、その機能をゲームで使用しているとは限らない。DOTweenは `Assets/Plugins/Demigiant/DOTween/` にあり、タイトル・フェード・確認演出等から参照されている。

### 非公開アセットとローカル依存

- `Assets/AssetStoreTools` はGit submodule。
- URL: `https://github.com/TsukasaUrakawa/ShelfCare-PrivateAssets.git`。別環境ではこの非公開リポジトリへの権限が必要。
- submoduleの `.gitattributes` はPNG、音声、FBX等にGit LFSを指定している。ポインターテキストだけでなく実ファイルを取得する必要がある。
- クローン・submodule・LFSの取得はユーザーのGit操作方針に合わせて進める。認証情報を文書やリポジトリへ書かない。
- `Library/`、`Temp/`、`Builds/`、`.utmp/` 等はGit対象外。元PCにあるだけのファイルが別環境にもあると仮定しない。
- unityroom用アイコンは元PCの `Builds/Unityroom/ShelfCare-unityroom-icon.png` と `ShelfCare-unityroom-icon-v2.jpg` に存在する。後者は512×512、装飾を増やした案。これらはGit管理外なので、今後編集するなら別途引き継ぐ必要がある。

### Unity CLI

マニフェストへのPipeline導入は済んでいる。別環境のUnity CLI本体の導入状況・バージョンは未確認。CLIが使える環境では、Editorでこのプロジェクトを開き、読み込み・コンパイル完了後にプロジェクトルートから接続する。

```powershell
unity status
unity command script eval_file --file Tests/Gameplay3DAssetSmoke.cs --json
```

- 後者はEditモードでの素材検証の例。他のスクリプトにはPlay状態や事前条件がある。
- 過去に接続待ちや `PIPELINE_UNREACHABLE` が発生した。Unityを前面に出し、ロード・コンパイル・再接続を待って回復した経緯がある。すぐにパッケージ再導入や設定変更で解決しようとしない。
- Pipeline設定は `ProjectSettings/Packages/com.unity.pipeline/EditorPipelineConfig.json`。Play中のバックグラウンド実行は現在無効。
- Editorでの操作用Pipelineと、公開プレイヤーの実行時機能は別。公開ビルドでは `ENABLE_RUNTIME_PIPELINE` / `ENABLE_RUNTIME_PIPELINE_DEV` を有効にしない。

## 9. 検証の進め方と確認済み範囲

**手順の一次資料は `Tests/README.md`。** `Tests/*.cs` と `Tools/*.cs` の多くはUnity CLIの `script eval_file` 用スクリプト本体で、通常のコンパイル対象のテストクラスではない。安易に `Assets/` に移さない。

### セーブを壊さないための必須手順

1. Playを停止したEditモードで、`Tests/BackupProgressForSmoke.cs` を実行する。設定を変更する検証では `Tests/BackupSettingsForSmoke.cs` も実行する。
2. 新規ゲームはタイトルの新規開始確認から作るか、Play中の `SceneButtonController.LoadGameScene()` を使う。Playの停止・再開だけでは保存した進行が復元される。
3. 各テストの指定されたシーン・状態・解像度・初期化フレームの条件を満たす。レイアウトやRaycastはパネルを開いた後に1フレーム描画させてから調べる。
4. **失敗時も含めてPlayを停止してから** `Tests/RestoreProgressAfterSmoke.cs` と、設定を退避した場合は `Tests/RestoreSettingsAfterSmoke.cs` を実行する。
5. 時計、Game view、開いているシーンを作業前の状態へ戻す。

バックアップは `.utmp/progress-smoke-original.txt` と `.utmp/settings-smoke-original.txt`。既存バックアップがある場合、上書きせず先の検証と復元状況を調べる。`PlayerPrefs.DeleteAll()` は使わない。本書作成時点では両バックアップファイルは存在しなかった。

### 代表的な検証入口

| 対象 | スクリプト例 |
| --- | --- |
| 本・3D素材・模様 | `Tests/Gameplay3DAssetSmoke.cs`、`BookPatternAssetSmoke.cs` |
| 全140冊・誤色・誤順・完成ロック・クリア | `Tests/ShelfLayoutPlaySmoke.cs` |
| 一覧・9冊・確認・フィルター | `Tests/BookListControlsSmoke.cs`、`HandInventorySmoke.cs`、`BookColorFilterSmoke.cs` |
| 移動・交換・保存復元 | `Tests/ShelfBookMoveSmoke.cs`、`SaveRestorePlaySmoke.cs`、`GameProgressStoreSmoke.cs` |
| 案内・中断・設定 | `Tests/BookPlacementTutorialSmoke.cs`、`PauseMenuFlowSmoke.cs`、`SettingsPanelSmoke.cs` |
| UI重なり・全スロット・箱のクリック | `Tests/UILayoutOverlapSmoke.cs`、`ShelfReachabilitySmoke.cs`、`BoxReachabilitySmoke.cs` |
| 3Dゴースト・棚端 | `Tests/Gameplay3DPreviewSmoke.cs`、`ShelfBayFitSmoke.cs` |
| クリック音・スライダー | `Tests/UIButtonAudioWiringSmoke.cs`、`UIClickAudioPlaySmoke.cs`、`SettingsSliderInputSmoke.cs` |

表で名前だけのファイルも `Tests/` 配下。テストごとにfixtureを変更するため、全ファイルを無条件に連続実行しない。CLI呼び出しの成功とネストしたevalの成功を両方確認する。重なり監査はさらに `issueCount == 0` と空の `issues` を確認する。

### 過去の記録と限界

- PR #18までの記録には、3D素材749項目、模様6177項目、全140冊配置・クリア、移動・交換、保存・復元、UI重なりと到達性の検証成功がある。UI監査の基準サイズは1920×1080、1366×768、1024×768。
- 公開準備のローカルChromiumでは、チュートリアル、9冊選択、フィルター、確認、配置・3Dゴースト・移動・交換・返却・完成図・誤答チェック、設定、中断、ページ再読み込み後の復元を確認した記録がある。復元例は棚1冊＋手持ち8冊。表示サイズは960×540、1024×768、1280×720。
- その時点でゲームの例外・シェーダーエラー・読込エラーは記録されていない。保存同期APIの将来廃止警告は残っている。
- Editor内のハンドラー呼び出し・uGUIイベント検証は、実ブラウザでのマウス・キーボード・音声・保存・描画の確認と同義ではない。
- ユーザーはアップロード後に「確認できました」、その後「公開できました」と報告した。ただし全140冊をunityroom上で通しプレイしたか、Chrome／Edge／全画面の各条件をどこまで試したかは記録から確定できない。

## 10. WebGLビルド・公開

詳細は `Docs/UnityroomRelease.md`、実装は `Assets/Editor/UnityroomBuild.cs`。

1. シーンを保存しPlayを停止、コンパイル完了を待つ。
2. Web Build Supportを用意し、Build Profilesで **WebをActive** にする。
3. `Tools > Shelf Care > Build for unityroom` を実行する。
4. `.utmp/unityroom-build-status.json` の `status` が `succeeded` になるのを待つ。失敗時は `error` とEditorログを確認する。

- 出力先は `Builds/Unityroom/ShelfCare`。
- Gzip、Decompression Fallback無効、ファイル名ハッシュ無効、開発ビルド／デバッグ／Profiler接続無効。960×540、初期メモリ128MB、GenericのTexture Compression、Webの品質設定はPC。
- domain reloadをまたいで待機する仕組みがある。未保存シーン・Play中・別ターゲット・Pipeline実行時定義を検出したら拒否する。
- ビルド結果の3つのGzipファイルを解凍検査し、WasmヘッダーとSHA-256を確認して `unityroom-files.json` を出力する。**loaderもアップロードに必要** だが、この3ファイルの検証リストには入っていない。
- 元PCの直近ステータスは2026-11-09の成功記録、警告0、約101.3MB。別環境での再ビルドの所要時間・容量・成功は保証しない。
- 本の画像の画質と切り出しを優先し、最終実装では本PNGのDXT強制設定は採用していない。

### ローカル確認

`index.html` の直接ダブルクリックではなく、GzipヘッダーとWasm MIMEに対応したサーバーを使う。

```powershell
./Tools/ServeUnityroomBuild.ps1
```

`http://127.0.0.1:8765/` を開く。サーバー終了はCtrl+C。スクリプトは対象UnityのWebGL同梱Pythonを使う。インストール先が標準と異なる場合は `-EditorPath` にUnity.exeを指定するか、任意のPython 3で `Tools/ServeUnityroomBuild.py` を実行する。

### unityroom側

- ゲームIDは `shelf-care`。スクリーンショットで確認した設定はUnity `6000.0.x`、960×540、縦横比を維持した拡大、モバイル対応チェックなし。現在の全設定値は再取得していない。
- Webビルドアップロードで選ぶのは、出力先直下の **`Build` フォルダー**。少なくとも以下4ファイルを含める。
  - `ShelfCare.loader.js`
  - `ShelfCare.data.gz`
  - `ShelfCare.framework.js.gz`
  - `ShelfCare.wasm.gz`
- 過去にはドラッグで進まなかったが、中央の点線枠をクリックして `Build` フォルダーを選択し、Chromeの確認でアップロードすると完了した。
- フォントライセンスは `Assets/StreamingAssets/` にある。Webビルド本体とは別のStreamingAssetsアップロード項目もあるため、実際の配信に含まれているか確認する。登録完了の証拠は会話から確認できない。
- ビルド作成やPR作成は公開サイトの更新ではない。アップロード・公開設定の操作はユーザーが行ってきた。

## 11. 未完了・未確認事項

以下は未実装機能を推測した一覧ではなく、引き継ぎ時に確認が残る事項。

| 項目 | 確認できたこと／残ること |
| --- | --- |
| 既存の設定差分2件 | ローカル変更として存在。意図・採否は未確認。ユーザーに確認せず整理しない |
| 音源の出典 | 音声アセットは使用されているが、元の配布元・作者・URLを特定できていない。ユーザーの元ダウンロード履歴等の確認が必要 |
| クレジット登録 | 使用アセット名称とURLの一覧を求められ、調査・案内した会話がある。unityroomへの実際の登録完了は未確認 |
| ライセンス配信 | ローカルのフォントOFLファイルは存在。unityroomのStreamingAssets側への配置・閲覧可否は未確認 |
| 公開環境の通しプレイ | 公開・基本操作確認の報告はある。unityroom上で全140冊の収納からクリア、さらに再読込後の復元まで通した記録は確認できない |
| Chrome／Edge／全画面 | ローカルの検証用ブラウザは全画面に制約があった。実環境のブラウザ別・全画面別の詳細確認結果は不明 |
| モバイル | 公開準備はPC向け。タッチ入力・モバイル性能・モバイルUIの検証や対応完了は確認できない |
| 保存の警告 | Unityの `JS_FileSystem_Sync` に関する将来廃止警告の記録がある。当時の保存・復元は成功。更新時に再評価する課題で、現在の保存不具合と断定しない |
| ローカル成果物の移行 | Git対象外のアイコン・ビルド・スクリーンショット・一時検証ファイルは自動では引き継がれない |
| 公開告知 | X用の短い文章案と動画構成の相談がある。投稿画面で19秒動画の添付準備は確認されたが、実際の投稿完了・動画内容全体は未確認 |

今回検索した `Assets/Scripts`、`Assets/Editor`、`Tools`、`Tests`、`Docs` に `TODO` / `FIXME` / `HACK` / `NotImplementedException` の明示的な該当はなかった。ただし、それだけで不具合や改善余地がないとは判断しない。新しい機能・大きな改修を始める際はユーザーの希望を確認する。

## 12. 使用アセットと出典の引き継ぎ

会話で調査した名称・URLと、ローカルの参照・ライセンスの記録。ストアの現在の表示内容や規約は本書作成時に再取得していない。公開用の正確なクレジットは音源も含めて確認すること。

| 素材 | 出典・状況 |
| --- | --- |
| Cardboard Box (Rigged) | [Unity Asset Store / 301834](https://assetstore.unity.com/packages/3d/props/cardboard-box-rigged-301834)。段ボール。過去の調査では同梱ReadmeにWeanWind、ストア表示にEzerealの名前があり、表記差がある |
| DOTween (HOTween v2) | [Unity Asset Store / 27676](https://assetstore.unity.com/packages/tools/animation/dotween-hotween-v2-27676)、Demigiant。コードから使用を確認 |
| Zen Maru Gothic Medium | [Google Fontsのソース](https://github.com/google/fonts/tree/main/ofl/zenmarugothic)。日本語UI。OFLは `Assets/StreamingAssets/ThirdParty/ZenMaruGothic-OFL.txt` にも同梱 |
| EB Garamond | [Google Fontsのソース](https://github.com/google/fonts/tree/main/ofl/ebgaramond)。英字見出し。OFLは `Assets/StreamingAssets/Licenses/EBGaramond-OFL.txt` にも同梱 |
| TextMesh Proの標準リソース | Liberation Sans・EmojiOne等の同梱リソースがある。実配布物・同梱ライセンスを確認して扱う |
| Cartoon FX Remaster Free | [Unity Asset Store / 109565](https://assetstore.unity.com/packages/vfx/particles/cartoon-fx-remaster-free-109565)。インポートされているが、過去の依存調査では現在の2シーンにFX Prefabの使用を確認できなかった。ゲームの演出に使用中と断定しない |
| BGM・効果音 | 非公開submodule内の `Sound/`。TitleBGM、GameBGM、ClearBGM等を参照。リネーム済みのファイル・メタデータから配布元の名称／URLは確定できなかった |
| 本の模様・旧背景・公開アイコン | 会話では画像生成による制作。模様・旧背景のプロンプトは `Art/` に保存。現在の3Dの本・棚・タイトル専用模様はEditor生成コードも参照 |

アセットを取得済みであることと、その出典を特定できること、配布に必要な表示を済ませたことは別。未確認の音源に推測の作者・URLを付けない。

## 13. 実装履歴と読むべき資料

### マージ済みPRの流れ

| PR | 主な内容 |
| --- | --- |
| #1–#2 | UI遷移・モーダル入力、Unity CLI／Pipeline導入 |
| #3–#5 | 日本語の本一覧操作、フォント安定化、棚スロット整列 |
| #6–#8 | 棚ナビ・確認画面、手持ち9冊、チュートリアル・模様関連改善 |
| #9–#10 | タイトル／クリア演出、自動保存と続きから |
| #11–#13 | 棚内移動・交換、色フィルター、音量・ミュート・動きを控えめにする設定 |
| #14–#16 | 中断と保存してタイトルへ、UI重なり監査、一覧ヘッダーの位置合わせ |
| #17–#18 | 立体のタイトル、本編の3D本棚・本・ゴースト、箱の接地影・棚端の余白調整 |
| #19 | WebGL公開ビルド、ローカル配信、旧画像のResources外への整理 |

### 仕様・編集・検証資料

- `Docs/3DGameplay.md`、`Docs/ScenePresentation.md`: 現在の立体表現、Builder、演出。
- `Docs/ShelfLayout.md`、`Docs/ShelfNavigation.md`、`Docs/UILayout.md`: 棚の配置・操作・重なり対策。
- `Docs/HandInventory.md`、`Docs/ShelfBookMovement.md`、`Docs/BookColorFilters.md`: 9冊の手持ち・並べ替え・一覧。
- `Docs/BookPlacementTutorial.md`、`Docs/SaveAndContinue.md`、`Docs/PauseAndReturn.md`、`Docs/AudioMotionSettings.md`: 案内・永続化・中断・設定。
- `Docs/UnityroomRelease.md`、`Tests/README.md`: ビルド、ブラウザ確認、各スモーク検証の事前条件。
- `Art/BookPatterns/README.md`、`Assets/Fonts/BookList/README.md`: 模様と日本語フォントの再生成。

### 既存資料の古い記述

- `Docs/UnityroomRelease.md` の「公開前に残っている確認」は公開前の記録。公開そのものは後の会話で完了している。一方、個々の確認が全部完了したとは判断できない。
- 同資料の結果欄の日付は2026-11-05だが、元PCの現存ビルドステータスは2026-11-09。日付が同じ実行記録とは扱わない。
- `Docs/ScenePresentation.md` と `Art/ScenePresentation/README.md` に残る `Assets/Resources/Title.png` / `ClearImage.png` は旧パス。現在は `Assets/Art/LegacyUI/` 配下へ移動済み。現行のタイトルと部屋は3D構成で、旧背景の説明をそのまま現行仕様としない。
- 旧UI画像34枚はGUIDと元画像を保持してResources外へ移し、自動同梱を減らしている。性能対策として元のResources位置へ戻さない。

### 参照した会話と確認の限界

- 本プロジェクトの開発会話: **「Unity CLIの導入を進める」**、ID `01a19a4f-647c-76a3-9142-01ade58dbbaa`。本書のユーザー希望・レビュー手順・仕様変更・公開報告の主要な根拠。
- 関連する告知会話: **「ShelfCareの公開告知案を作成」**、ID `01a1ccb2-80af-72b2-8e63-45b27464f042`。X告知案・動画案・GitHub URLへの懸念を確認。
- 関連する事前相談 **「X投稿案作成」** も参照したが、そこにある初期の「2Dお片付け」等の仮の理解は、現行の3D幾何学パズルの仕様根拠にはしない。
- 別環境で上記チャットや元PCの添付画像へアクセスできる保証はない。会話の全ツール出力を本書へ転記してはいない。仕様変更の理由が不明な場合、推測で埋めずユーザーへ確認する。

この引き継ぎ作業では既存コード・シーン・設定を変更せず、ルートの `PROJECT_CONTEXT.md` のみを追加する。
