# unityroom公開準備

対象はPCのChrome / Edge、ゲーム画面は960×540（16:9）です。
Unity 6000.0.75f1とWeb Build Supportを使います。

## 公開用ビルド

1. シーンを保存し、Playを停止します。
2. Build ProfilesでWebをActiveにします。
3. `Tools > Shelf Care > Build for unityroom`を実行します。
4. `.utmp/unityroom-build-status.json`の`status`が`succeeded`になるまで待ちます。

出力先は`Builds/Unityroom/ShelfCare`です。ビルドごとに次の3ファイルを検証し、
サイズとSHA-256を`unityroom-files.json`に記録します。

- `Build/ShelfCare.data.gz`
- `Build/ShelfCare.framework.js.gz`
- `Build/ShelfCare.wasm.gz`

開発ビルド・スクリプトデバッグ・Profiler接続は無効、Gzip圧縮、Decompression Fallbackは無効です。
これらは[unityroom公式のビルド手順](https://help.unityroom.com/unityroom-351dc3ed5de980eebd79eef3b153be31)に合わせています。
公開用の製品名はビルド中だけ`Shelf Care`とし、Editorの既存PlayerPrefs用の名前は戻します。
Editorで使うUnity Pipelineの実行時機能は配布ビルドに含めません。

Web版はPC品質を使います。Texture CompressionはUse Player Settings（Generic）です。
本のPNG・切り出し領域・GUID・インポート設定は維持します。
現在の本の画像は非圧縮のまま同梱し、模様の画質と切り出し位置を優先しています。
現在使っていない旧UI画像は`Assets/Art/LegacyUI`へ移し、Resourcesによる自動同梱を避けています。
元の素材とGUIDは保管しています。実行時のResources読み込みは`Books`です。

## ブラウザでの確認

`index.html`をダブルクリックせず、Gzip用のHTTPヘッダーを付けるローカルサーバーを使います。
Windowsではプロジェクトのルートで次を実行します。Unity WebGLに付属するPythonを使うため、
別途Pythonをインストールする必要はありません。

```powershell
./Tools/ServeUnityroomBuild.ps1
```

`http://127.0.0.1:8765/`をブラウザで開きます。終了時はサーバーのPowerShellでCtrl+Cを押します。
Unityのインストール先が標準と違う場合は`-EditorPath 'D:/Unity/Editor/Unity.exe'`を指定します。
任意のPython 3でも`python Tools/ServeUnityroomBuild.py`で起動できます。
ビルドし直したらページを再読み込みします。

確認する流れ：

- タイトルの本・模様・日本語が表示され、ボタン操作で開始できる。
- 箱から本を最大9冊取り出し、手持ちから選び、同色の棚に配置できる。
- ゴースト・移動・交換・返却・完成図・並びのチェックが使える。
- 設定でBGM・効果音・ミュート・動きを控えめにする操作が反映される。
- 中断してタイトルへ戻り、続きから再開できる。ページ再読み込み後も配置・手持ち・設定が戻る。
- 960×540と大きな画面、全画面表示でUIの欠け・重なりや本のクリック漏れがない。
- Consoleにゲームの例外・シェーダーエラー・読込エラーがない。

ブラウザ保存はEditorの保存とは別です。IndexedDBが使えない設定ではゲーム内の保存失敗表示を確認してください。
このローカル確認の後、unityroomの非公開プレビューでも同じ流れを確認してから公開します。
公開・アップロードはビルド作成だけでは行いません。

## 今回の確認結果（2026-11-05）

Unity 6000.0.75f1で公開ビルドに成功し、ビルド警告は0件でした。
配信用3ファイルの合計は約101.3MBです（整理前は約134.1MB）。
34枚の旧画像は移動前後で画像データとGUIDが一致しています。

ローカルサーバー上のChromiumで、次を実際に操作しました。

- 初回チュートリアル、色フィルター、9冊の選択上限、確認画面からの取り出し。
- 3Dゴースト、配置、移動、交換、手持ちへの返却、完成図、未完成時の並びチェック。
- BGM・効果音の音量、効果音の試聴、ミュート、動きを控えめにする設定。
- 設定の再読み込み後の復元、中断保存、ページ再読み込み後の「続きから」。
  棚の1冊と手持ち8冊が復元されることを確認しました。
- 960×540・1024×768・1280×720での表示。

ゲームの例外・シェーダーエラー・読込エラーはありませんでした。
Unityの保存同期APIについて将来の廃止を知らせる警告は出ますが、
現バージョンでの保存と復元は通っています。
Editorの3D素材検証749項目、模様・切り出し検証6177項目も通過しました。
Editorの既存進行状況と設定は検証前の状態に戻しています。

公開前に残っている確認：

- 実際のChrome / Edgeとunityroomの非公開プレビューでの全画面切り替え。
  検証用ブラウザでは全画面時の表示に制約があり、確認済みには含めていません。
- unityroom上で最初から全棚の完成まで通して遊ぶこと。
- ゲーム説明、操作方法、紹介画像、クレジットの確認と登録。
