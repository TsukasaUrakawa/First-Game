# Gameplay UI smoke check

Requires the Unity CLI and `com.unity.pipeline` in the local project. The check
uses the real ShelfCare scene and its persistent button listeners. It does not
save Play mode changes. No Pipeline dependency is needed in a player build.

1. Open `Assets/Scenes/ShelfCare.unity` in the Editor and start a fresh Play session.
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

5. Stop Play mode to discard the test inventory and game-clear state.

For the fixed Japanese book-list controls, repeat steps 1–3 in another fresh Play
session, then run:

```powershell
unity command script eval_file --file Tests/BookListControlsSmoke.cs --json
```

This checks selection counters and limits, the new confirmation/clear buttons,
full and partial hand capacity, fixed controls while scrolling, reachability of
the last row, reopening, and coverage of the static Japanese font atlas. Stop
Play mode afterwards to discard the test inventory.

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
confirmation. Stop Play mode afterwards to discard test changes.

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
Stop Play mode afterwards to discard books, completion state, and audio changes.

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
1366×768 and stop Play mode afterwards to discard inventory changes.

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
mode afterwards to discard the test inventory and clear state.

For startup persistence, check once with no tutorial preference (guide starts)
and once after skipping/completing (only the help button appears). Record and
restore the original key when testing: `ShelfCare.BookPlacementTutorial.v1`.
Play mode changes to PlayerPrefs persist after stopping; inventory does not.

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
