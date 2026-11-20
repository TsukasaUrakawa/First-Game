# Shelf Care

本を整理し、背表紙の幾何学模様をつなげる本棚パズルです。
箱から本を取り出し、同じ色の棚へ配置・入れ替えをしながら、8つの棚の完成を目指します。

> このリポジトリ名は `First-Game` ですが、中身は Shelf Care のソースコードです。
> 初めて制作したゲームを、unityroomで公開するためにブラッシュアップした作品です。

- プレイする: https://unityroom.com/games/shelf-care
- ポートフォリオ: https://tsukasaurakawa.github.io/
- プレイ映像: https://youtu.be/4qP1IyybPLQ
- 公開日: 2026年11月9日
- 想定プレイ時間: 約30分
- 制作期間: 2026年6月7日〜
- 体制: 個人制作（AIを活用）
- 担当: 初期版の制作 / AIを活用したUI・操作性・素材のブラッシュアップ

## 遊び方

対応環境: PCブラウザ（WebGL / unityroom、Chrome / Edgeを想定）
操作: マウス

1. 箱をクリックし、本一覧から取り出す本を選びます。手持ちは最大9冊です。
2. 手持ちの本を選び、同じ色の棚の空いている場所をクリックして配置します。
3. 棚の本を選び、空き位置をクリックすると移動、別の本をクリックすると入れ替えができます。
4. 「完成図を見る」で模様を確認し、並べ終えたら「並びをチェック」で完成を確定します。

選択を解除するには、同じ本を再クリックするか、右クリック・Escを使います。

棚は2列×4段、8色、合計140冊です。同じ色の棚にのみ置けます。

## 技術

- エンジン: Unity 6000.0.75f1
- 言語: C#
- ライブラリ: DOTween
- UI: uGUI
- 公開形式: WebGL（unityroom）

## AIの活用範囲

初期版を個人で制作し、ブラッシュアップでは次の範囲にAIを活用しました。

- UI・操作処理の改修: Codex
- 画像素材の作成: OpenAIの画像生成機能（Codex内で使用）による幾何学模様・公開用アイコンなど

改修版のコードやビジュアルには、AIによる制作・修正を含みます。

## 実装のポイント

### UI・ビジュアル

タイトルと本編を、深い青緑・紙のような生成り・銅色の模様で統一しました。タイトルには波状に並ぶ立体の本を配置し、本編の本棚と本も3D化しています。
本一覧には色別フィルターと残り冊数、画面下には9冊分の手持ち枠を設け、選んだ本と次の操作を確認しやすい表示にしました。

- 仕様: [Docs/3DGameplay.md](Docs/3DGameplay.md)
- 描画: [Assets/Scripts/BookVisual3D.cs](Assets/Scripts/BookVisual3D.cs)

### 操作性

棚に置いた本を直接選び、空き位置への移動、別の本との入れ替え、手持ちへの返却ができるようにしました。選択中の強調表示と半透明の配置プレビューで、操作する対象と移動先を示しています。

- 仕様: [Docs/ShelfBookMovement.md](Docs/ShelfBookMovement.md)
- コード: [Assets/Scripts/BookSlot.cs](Assets/Scripts/BookSlot.cs)

### 保存・復元と状態の整合性

本の識別名・配置・手持ちの順番・完成した棚をJSONとして保存します。読み込み時には本や配置位置の重複、色の不一致などを検証し、最新の記録が不正な場合は前回の正常な記録からの復元を試みます。

- 仕様: [Docs/SaveAndContinue.md](Docs/SaveAndContinue.md)
- コード: [Assets/Scripts/GameProgressStore.cs](Assets/Scripts/GameProgressStore.cs)

### WebGL公開・検証

公開準備では、使わなくなった旧UI画像をResourcesの外へ移し、本の模様の画質を保ちながら配信データを整理しました。主要3ファイルの合計は、約134.1MBから約101.3MBになっています。

- 記録: [Docs/UnityroomRelease.md](Docs/UnityroomRelease.md)
- 検証手順: [Tests/README.md](Tests/README.md) — スクリーンショット付きスモークテスト群（Playwright中心）

## 資料

`Docs/` 配下の主な資料です。

- `3DGameplay.md` — 3D化の仕様
- `ShelfBookMovement.md` — 移動・入れ替えの仕様
- `SaveAndContinue.md` — 保存・再開の仕様
- `UnityroomRelease.md` — WebGL公開・確認の記録
- `UILayout.md` / `ShelfLayout.md` / `ShelfNavigation.md` — UI・棚・画面遷移
- `BookColorFilters.md` / `BookPlacementTutorial.md` / `HandInventory.md` — 色フィルター・チュートリアル・手持ち
- `AudioMotionSettings.md` / `PauseAndReturn.md` / `ScenePresentation.md` — 音・動きの設定・中断・シーン演出

## 開き方

1. このリポジトリをクローンします。
2. Unity 6000.0.75f1 でプロジェクトを開きます。
3. WebGLビルドでunityroom向けの公開用ビルドを作成できます。

## クレジット

- AI活用: CodexによるUI・操作処理の改修
- 画像素材: OpenAIの画像生成機能（Codex内で使用）による幾何学模様・公開用アイコンなど
- UIアニメーション・画面遷移: DOTween（Demigiant）
- 段ボールのモデル: Cardboard Box (Rigged)（WeanWind / Ezereal）
- フォント: Zen Maru Gothic / EB Garamond
- 音源の作者・配布元は確認中です。
