# Gameplay UI smoke check

Requires the Unity CLI and `com.unity.pipeline` in the local project. The check
uses the real ShelfCare scene and its persistent button listeners. Progress now
autosaves in Play mode. No Pipeline dependency is needed in a player build.

Before any Play tests, run `Tests/BackupProgressForSmoke.cs` in Edit mode. Keep
that backup until testing is finished. Create each fresh game through the title's
New Game confirmation, or, while already in gameplay, call
`SceneButtonController.LoadGameScene()` through the CLI. Merely stopping and
starting Play resumes saved progress. After all tests (including on failure),
stop Play and run `Tests/RestoreProgressAfterSmoke.cs`. Only the two progress
preferences and the tutorial preference are backed up/restored; do not use DeleteAll.

1. Start a fresh ShelfCare game using the procedure above.
2. Verify the room is shown, with the book list closed and shelf buttons visible.
   For the baseline checks below, hide the tutorial for this session without
   changing its saved preference:

   ```powershell
   unity command script eval_file --file Tests/SuspendTutorialForSmoke.cs --json
   ```
3. Open the book list and let a frame run so that its `Start` creates the buttons:

   ```powershell
   unity command script eval 'GameplayUIController.Instance.OpenBookList(); return true;'
   ```

4. Run from the project directory:

   ```powershell
   unity command script eval_file --file Tests/GameplayUIFlowSmoke.cs --json
   ```

   Both the command and its nested eval result must report `success: true`.
   The result lists each passing check; a failed assertion throws with its label.

5. Stop Play. The test progress remains saved; start a new game for the next check.

For the fixed Japanese book-list controls, repeat steps 1–3 in another fresh Play
session, then run:

```powershell
unity command script eval_file --file Tests/BookListControlsSmoke.cs --json
```

This checks selection counters and limits, the new confirmation/clear buttons,
full and partial hand capacity, fixed controls while scrolling, reachability of
the last row, reopening, and coverage of the static Japanese font atlas. Stop
Play mode afterwards; start a new game for subsequent checks.

For visual/input verification, start another fresh session and check:

- Click the box, choose books, right-click to confirm, then press Escape: only
  confirmation closes and the books remain selected. Press Escape again: the
  list closes and selection clears.
- Click a shelf card: it directly zooms that shelf. The fixed toolbar shows its
  name, storage count, completion-image and check actions. It does not block
  placement; opening the list or completion image temporarily hides it.
- Zoom a shelf, use its upper-left 部屋に戻る button, then repeat with Escape and with
  right-click outside the UI. Each returns to the room.
- While the answer panel is open, clicking outside its graphic must not place or
  return a book behind it.
- Check the zoom return button at 1280×720 and 960×540; it should remain fully
  visible at the top left and disappear behind modal panels.

The smoke check calls the same `Back` action as Escape/right-click, but does not
synthesize OS keyboard or mouse events. WebGL/browser input is a separate check.

## Shelf navigation and confirmation

Repeat steps 1–3 in a fresh Play session, then run:

```powershell
unity command script eval_file --file Tests/ShelfNavigationDialogSmoke.cs --json
```

This checks the physical shelf order, one-click zoom, toolbar context and
visibility, immediate progress after actual placement/return, 1/3/6/9-book preview
alignment, preserving selection on Back/Cancel, inventory capacity and duplicate
Take calls, and Japanese glyph coverage. Repeat at 1366×768 to check the scaled
layout as well as 1920×1080.

For the rendered raycast check, start a fresh session, open the list, select any
book and open confirmation. Let a frame render, then run:

```powershell
unity command script eval_file --file Tests/ConfirmationRaycastSmoke.cs --json
```

This verifies the backdrop intercepts the underlying list Close and all four
screen corners, while both dialog actions remain reachable. Canvas raycast depth
is assigned during rendering, so run this separately from the call that opens
confirmation. Stop Play afterwards and restore the original preferences after testing.

## Shelf layout checks

With ShelfCare open in Edit mode:

```powershell
unity command script eval_file --file Tests/ShelfLayoutEditSmoke.cs --json
```

This checks the saved shelf endpoints, heights, slot identities, uniform spacing,
empty/single/inactive slots, numbered order after hierarchy changes, local-space
rotation/scaling, and the Editor action's Undo. Temporary objects are removed
from a separate test scene afterwards.

In another fresh ShelfCare Play session, keep the book list closed and run:

```powershell
unity command script eval_file --file Tests/ShelfLayoutPlaySmoke.cs --json
```

This places all 140 books through the gameplay controller, exercises wrong color
and wrong order, returns/replaces endpoint books, checks colliders and completed
shelf locks, and verifies game clear. It silences test audio for that Play session.
Stop Play afterwards to discard audio changes. Start a new game for subsequent checks.

## Nine-book hand inventory

In another fresh ShelfCare Play session, open the book list and let its first
frame create the books, then run:

```powershell
unity command script eval_file --file Tests/HandInventorySmoke.cs --json
```

This takes nine real books, rejects a tenth, selects/deselects the ninth pocket,
checks hover and modal input, places and returns the ninth book with its identity
intact, and checks returning into a full/partial hand. It also verifies immediate
counts/guidance, empty pockets, screen bounds and Japanese glyphs. Repeat at
1366×768 and stop Play afterwards. Inventory changes are saved; use a new game for the next check.

## Book placement tutorial

Start a fresh Play session. Open the book list, let a frame create its buttons,
then close it before running this check (do not suspend the tutorial):

```powershell
unity command script eval 'GameplayUIController.Instance.OpenBookList(); return true;'
# Let a frame run before this next call.
unity command script eval 'GameplayUIController.Instance.CloseBookList(); return true;'
unity command script eval_file --file Tests/BookPlacementTutorialSmoke.cs --json
```

This checks startup against the saved preference, all seven action steps,
selection cancellation, Back, wrong-shelf recovery, answer/list detours, failed
placement, completion, skip/replay, existing/full inventory and game-clear input.
It verifies Japanese glyphs and text bounds at each step. The check restores its
original PlayerPrefs key in `finally`, including on assertion failure. Stop Play
mode afterwards, and use a new game for the next check.

For startup persistence, check once with no tutorial preference (guide starts)
and once after skipping/completing (only the help button appears). Record and
restore the original key when testing: `ShelfCare.BookPlacementTutorial.v1`.
Play mode changes to PlayerPrefs, inventory and placement persist after stopping.

For visual/input verification at 1920×1080 and 1366×768:

While the tutorial is at its take or place step, let a frame render and run:

```powershell
unity command script eval_file --file Tests/TutorialPresentationSmoke.cs --json
```

This verifies guide/focus bounds, text, button raycasts and the reachability of
empty shelf positions beneath the spotlight and pulse.

- The box itself pulses green without a frame against a dimmed room; its open animation leads to
  the book list. Its original colors return when the target changes or the guide closes.
- The list and confirmation guides stay above the books and action buttons.
- Buttons/books themselves pulse green; no frame or rectangular fill is drawn.
  The pulse and surrounding dimmer do not intercept clicks. The guide stays
  bright and readable; completion, skip and the answer image hide the dimmer.
- Wrong-shelf guidance points to 部屋に戻る, then to the matching shelf card.
- Placement guidance stays above the book row and does not cover hand pockets.

## Geometric art and shelf presentation

In Edit mode run `Tests/BookPatternAssetSmoke.cs` to check all 140 sprite identities,
image proportions and sampled pixels against the eight completion pictures.
`Tools/BuildBookPatternAssets.cs` also verifies that rebuilding preserves every
existing sprite GUID and internal ID. Source art and prompts are in `Art/BookPatterns`.

In a fresh Play session, suspend the tutorial, zoom a shelf, open its completion
picture and let a frame render. Then run `Tests/ShelfPresentationSmoke.cs`.
Repeat at 1920×1080 and 1366×768. This checks the paper card, wooden picture display,
all eight shelf titles/images, Japanese glyphs, modal raycasts and both return buttons.
Also run the existing full shelf Play check after replacing artwork to verify
all 140 books can still be placed, returned, judged and cleared.

## Title, clear and paper book selection

In a fresh TitleScene Play session, let the intro render, then run:

```powershell
unity command script eval_file --file Tests/PresentationSceneSmoke.cs --json
```

Repeat after completing the game (or invoking `PlayClearEffect` and waiting for
its fade) to check the completion card, all eight earned patterns and both buttons.
Check at 1920×1080 and 1366×768. Verify that background, headline vertices and
ambient geometry change over time, including with Time.timeScale set to zero;
restore timeScale afterwards. The buttons must remain stationary and reachable.

Click Start to enter ShelfCare; Replay must create an empty fresh game, and Title
must return to TitleScene. In the Editor, Quit should finish its fade and allow
further input. WebGL/browser testing is separate; the Quit button is hidden there.

Run the existing book-list controls, tutorial flow and confirmation raycast checks
with the new cards. Verify full-card clicks, persistent selection, nine-book
capacity, scrolling to the last row and that guide text does not cover the heading.

## Automatic saving and Continue

In Edit mode run `Tests/GameProgressStoreSmoke.cs` (33 checks). It validates
round trips, malformed/versioned data, duplicates, limits, completion consistency,
previous-snapshot recovery, reset of both generations and the 140-book catalogue.
Its own `finally` restores both progress preferences.

Back up preferences using the procedure at the start of this document before
Play checks. In TitleScene after the intro run `Tests/SaveTitleSmoke.cs`: 16 checks
without progress, 35 with progress. With progress, open New Game confirmation,
let one frame render, then run `Tests/NewGameDialogSmoke.cs` (4 checks).
Check at 1920×1080 and 1366×768. Cancel and Escape must retain both saved copies.
Verify keyboard focus starts on Cancel, cannot enter the underlying title actions,
and returns when cancelling. Double Start/Confirm must create only one transition.

For a persistence fixture, use actual controls to complete one shelf, place two
books in reversed order in another, and fill the hand to nine. Include a placement
and return before filling the hand. After a frame's autosave, record the expected
snapshot before ending Play:

```powershell
unity command script eval 'GameProgressStore.Read(out var data); System.IO.File.WriteAllText(".utmp/save-smoke-expected.json", JsonUtility.ToJson(data)); return true;' --json
```

Stop Play, restart TitleScene and choose Continue. Check that it initially shows
the room, no selected hand book, and no old zoom or panels. Open the list and let
a frame create its buttons, then close it and run `Tests/SaveRestorePlaySmoke.cs`.
This compares every restored book, hand order, completion lock, shelf count and
box contents with the recorded expectation (84 checks for 20 placed / 9 hand /
1 completed; 443 for a cleared 140-book / 8-shelf game). Neither the fixture nor
the backup files belong in Git.

Verify autosave after Take, Place, Return and Check; rejected full-hand/locked-shelf
operations must leave the snapshot unchanged. Confirm New Game must empty the
room and replace both saved generations. Cancelling must leave them intact.
Check Clear → Title → Continue restores Clear, while Replay creates an empty game.
Test transitions with Time.timeScale=0, then restore it to 1.

For recovery, replace only the primary preference with malformed JSON while a
valid backup remains. Title must report recovery; Continue must restore the
backup and repair the primary on its next save. With both copies invalid, Title
must explain the problem and preserve them until the explicit fresh start.
Restore the original preferences after all cases. Storage-denied / quota errors,
page reload and private browsing require a separate Web build/browser check.
