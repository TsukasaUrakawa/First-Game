# 棚の配置を調整する

Hierarchyの`BookShelfJudgeRoot`内にある`BookShelf01`〜`BookShelf08`を選択します。
Inspectorの`Book Shelf Slot Aligner`で次を調整してください。

- `Start Local Position`：最初の本の位置。棚を基準にした座標です。
- `Slot Spacing`：本と本の中心の間隔です。
- 「配置する本の数」：直下にある`BookSlot`の数を表示します。手入力は不要です。

数値を変更するとその場で整列します。「本の位置を整列」ボタンや、
コンポーネントのメニューにある`Align Child Slots`でも整列できます。
整列操作はUndoに対応しています。Play中のInspector操作は無効です。

本は`SlotIndex`順に並び、Hierarchy上の並び順を変えても位置の対応は変わりません。
装飾用オブジェクトや、別の階層にあるスロットは整列対象に含みません。
Play開始時にも設定を適用します。

本の番号は`BookSlot`の名前から決まり、棚の名前は色判定にも使われています。
今回の変更は既存の140スロットをそのまま使います。冊数や本の種類を増やす場合は、
本のデータ・正解番号・判定も合わせて調整してください。

編集したシーンやPrefabを保存した後、SourceTreeで差分を確認してコミットします。
