# 部屋の背景素材

現在のタイトルは`TitleBookStage`の立体オブジェクトによる演出です。
このフォルダーの画像生成記録と`Assets/Resources/Title.png`は旧タイトル用です。
現在のタイトルの生成コード・編集方法・今後の本編用配色は`Docs/ScenePresentation.md`を参照してください。

既存のタイトル・クリア画像をbuilt-in image_genで編集しました。
文字・中央のロゴ装飾を除き、木の部屋と植物、暖かい光を残しています。
棚の本には現在のゲームの幾何学模様の雰囲気を加えました。

- タイトル: `Assets/Resources/Title.png`
- クリア: `Assets/Resources/ClearImage.png`
- 入力画像と編集条件: `prompts.json`

文字と動く金色の模様は画像に焼き込まず、Unity側で別に描画します。
