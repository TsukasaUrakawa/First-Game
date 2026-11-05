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
For settings checks, also back up and restore the settings key using the separate
helpers described in the audio/motion section below.

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

The gameplay room and books now use real 3D meshes and colliders; see
`Docs/3DGameplay.md`. In Edit mode run `Gameplay3DAssetSmoke.cs` to verify the
original 140 sprite identities/UV regions, eight shared source textures, ordinary
thick book geometry, 3D colliders and genuinely transparent preview materials.
`BookPatternAssetSmoke.cs` still checks the original image pixels/completion pictures.

Back up progress/settings, then in Play use `Gameplay3DRoomSmoke.cs` in the room
and `ShelfReachabilitySmoke.cs` across all eight zoomed shelves. Both use actual
Physics.Raycast hits as well as UI raycasts. Run in an unfinished game because
completed shelves intentionally disable book input. Repeat at 1920×1080,
1366×768 and 1024×768, including resizing an already zoomed shelf.
`Gameplay3DPreviewSmoke.cs` needs a hand book and an empty place of that color;
it checks that a cached translucent preview never intercepts the empty-slot ray,
that its binding/pages have visible depth, that placement exactly replaces its
geometry/pose, and that leaving/opening a modal hides it. `BoxReachabilitySmoke`
also checks the visible cardboard bottom against the floor and its soft contact
shadow, including after a resize or returning from a shelf.
`ShelfBayFitSmoke.cs` runs after full placement and checks actual rendered book
bounds against both physical bay edges, without stretched book transforms.
The existing move/swap, save/restore, full 140-book completion, hand, tutorial, pause and settings checks use the same
gameplay contracts with `BookObject.Sprite` and 3D Collider references.

For the live 3D title, back up progress/settings as above, enter TitleScene Play,
disable reduced motion, and run `Tests/TitleBookSculptureSmoke.cs` at 1920×1080,
1366×768 and 4:3. It samples 49 poses across the intro and full idle cycle,
checks actual projected mesh vertices against the menu/screen area, and uses
oriented bounding boxes to detect intersecting solid books. It also checks that
bindings/pages are rigid shared geometry and that title art is independent of
the gameplay textures. The method restores the animation clock after sampling.

Run the existing `PresentationSceneSmoke`, `SaveTitleSmoke`, `NewGameDialogSmoke`,
`SettingsPanelSmoke`, `SettingsPresentationSmoke` and `UILayoutOverlapSmoke`
for text, saved/no-save actions, modal input and settings. Compare book/orbit
transforms across rendered frames before and after switching reduced motion;
they should move normally and remain unchanged with reduced motion enabled.
Stop Play before restoring both preference backups. Browser/WebGL rendering,
performance and actual audio output remain separate checks.

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

## Moving and swapping shelf books

Back up preferences as described at the top of this document. Start a new game,
open the box once so its cards are created, and close it. Run
`Tests/ShelfBookMoveSmoke.cs` (77 checks). It uses real selection/take actions,
world click handlers and the return button to check:

- Movement and swaps with nine books in hand, preserving every book identity,
  scale, hand order, position, click handler and collider.
- A single shelf notification per transfer, with both references already final
  even if an observer saves immediately.
- Rejected self/null/cross-color/locked targets, cancellation, modal blocking,
  hand versus shelf selection, completed locks and the return-capacity limit.
- First Back cancels; second Back leaves. The room button leaves in one click
  while preserving its click sound. Reopening the tutorial cancels selection.
- Saving the moved layout without changing version 1 of the progress format.

The check leaves a selected blue book, nine red books in hand, 27 placed books
and one completed shelf. Let a frame render, then run
`Tests/ShelfBookMovePresentationSmoke.cs` (34 checks) at 1920×1080 and 1366×768.
It checks text/glyphs, toolbar bounds, preview, both button raycasts, the disabled
full-hand return explanation and that every position in the shelf remains reachable.
Repeat with a free hand slot to check the enabled return state.

The move check writes the expected layout to `.utmp/save-smoke-expected.json`.
Stop Play and restart (or return to Title and Continue). Open and close the box
after its first frame, then run `Tests/SaveRestorePlaySmoke.cs` (105 checks for
this fixture). The stored positions, box contents and hand order must return,
while the transient shelf selection must not. Restore the original preferences
after all Play checks, including on a failure.

Also verify automatic saving without explicitly calling SaveNow after a move,
and that pulse colors change with Time.timeScale=0 and restore on cancellation.
Existing shelf navigation (96), hand inventory (49), book list (27), tutorial
(110) and all-book placement/completion (777) checks should still pass in separate
fresh games. The world-handler check temporarily disables the EventSystem to
avoid dependence on the OS pointer position; it does not synthesize OS input.
Manually check right-click/Escape on the Web build in addition to these CLI checks.

## Book-list color filters

Back up preferences before Play checks as described at the start of this document.
Start a new game, open the box and let a frame create its cards. Run
`Tests/BookColorFilterSmoke.cs` (259 checks). It exercises all eight colors and All,
stable card instances/order, exact remaining counts, switching from the bottom
of a scrolled list, cross-color selection and clearing, hover restoration, nine-book
capacity, mixed-color confirmation order, modal guards, immediate removal on Take,
reopening and both empty-state messages. It uses actual selection/take/place actions
to take all 140 books and leaves 133 placed plus seven in hand, with no box books.

The check writes `.utmp/save-smoke-expected.json`. Let a frame autosave, then
return to Title and Continue (or stop and restart Play). Counts must be correct
before the list's first Start. The view starts on All. Open the list, let its cards
initialize, close it and run `Tests/SaveRestorePlaySmoke.cs` (423 checks for this
fixture). Also test a partial save with Green01 placed and Blue01 in hand: counts
are total 138, green 17 and blue 16, and the restore smoke has 27 checks.

With the list open and a rendered frame, run `Tests/BookColorFilterPresentationSmoke.cs`
at 1920×1080 and 1366×768 (83 checks, 84 while the tutorial is visible). Cover All,
a populated color, a color with no remaining books and the fully empty box.
It checks all nine button raycasts, text/glyphs, selected markers, scroll reachability
and the empty message. A sparse single row is covered by taking nine blue books,
placing them and switching back to the remaining eight blue books from the bottom
of All. The last row must remain reachable without moving the filters.

Select nine books across colors and return to green to check the hidden-selection
limit message. Open confirmation, let a frame render, then run
`Tests/BookColorFilterConfirmationSmoke.cs` (9 checks): the overlay must intercept
all nine filter positions. Back retains the color and the selections; Close clears
all selections but reopening retains the color within the same game.

Run the existing book list (27), tutorial (110), hand inventory (49), shelf movement
(77), shelf navigation/confirmation (96), and full placement/clear (777) checks in
separate fresh games. Restore original preferences after all checks, including on
failure. Browser input, rendering and storage remain separate Web-build checks.

## Audio and motion settings

In Edit mode, before testing, run both `Tests/BackupProgressForSmoke.cs` and
`Tests/BackupSettingsForSmoke.cs`. Keep their ignored `.utmp` backups until all
checks finish. After testing, including on failure, stop Play and run both
`Tests/RestoreProgressAfterSmoke.cs` and `Tests/RestoreSettingsAfterSmoke.cs`.
Never restore while Play is still running; shutdown callbacks may save again.

Run `Tests/GamePreferencesStoreSmoke.cs` in Edit mode (20 checks). It also restores
the previous settings in a finally block. It checks defaults, live events, dirty
state, clamping and nonfinite inputs, mute without losing levels, JSON round trips,
invalid/oversized/versioned data, and isolation from progress.

In TitleScene Play, with a nonempty saved game so New Game is available, run
`Tests/SettingsPanelSmoke.cs` (20 checks). Repeat in gameplay with a selected hand
book, an unlocked placed book, the shelf zoomed and the tutorial running (27 checks).
The check leaves the menu open. It exercises the actual controls, authored gains,
percentages, mute/unmute, preview, BGM playback, saving, focus and modal guards.
It also covers opening settings during a box-opening animation, shelf judgement
behind the modal, and preservation of camera, inventory, tutorial and progress.

After a rendered frame with the menu open, run `Tests/SettingsPresentationSmoke.cs`
in both scenes at 1920×1080 and 1366×768 (65 checks each). It checks text, glyphs,
card bounds, every control's raycast and the backdrop. It also moves both sliders
silently through 0/25/50/75/100%, checks that their thumbs stay inside the row and
never overlap Preview, and raycasts at each thumb's lower edge. This catches
UGUI's driven-anchor stretching and low-volume overlap that a center hit misses.
The original slider values and saved preferences must remain unchanged. Run
`Tests/SettingsSliderInputSmoke.cs` in both scenes (8 checks each) to test native
UGUI pointer/drag events, toggle clicks and slider fill/handle anchors. These calls
do not synthesize OS mouse or keyboard input.

Close the menu, stop Play, restart TitleScene and confirm that BGM 25%, effects 60%,
mute and reduced motion persist. Continue into gameplay and confirm the same levels
apply to its different AudioSources. A fresh game and Replay must retain settings.
In TitleScene, open New Game confirmation, let a frame render and raycast at the
settings access position: the confirmation must receive the hit. Settings access
must also remain below the scene fade in the hierarchy.

Use `Tests/SettingsMotionSnapshot.cs` twice across rendered frames in both Title
and Clear, including with Time.timeScale=0. With reduced motion enabled, background,
brand, glyph and mote coordinates must match; with it disabled, they must change.
BGM should keep playing and its sample position should advance while muted.
Setting changes should save after 0.35 unscaled seconds. Restore Time.timeScale=1.
For the tutorial box, sample its renderer's `_BaseColor` material property block;
for a selected shelf book, sample its BookVisual3D spine renderer's `_BaseColor` property block. Green must remain steady
with reduced motion and pulse normally when disabled.

Run existing list controls (27), tutorial (110), hand inventory (49), shelf moves
(77), color filters (259), navigation/confirmation (96), and full placement/clear
(777) checks in separate fresh games. The title/save checks (35 with a saved game)
and normal/reduced Clear presentation checks (36) must also pass.

Finally restore both original backups, reset the Game view to 1920×1080, and leave
Play stopped. Manually verify audible levels, mute, effect preview, Escape, dragging,
and browser reload/storage in a WebGL build. No browser audio/storage test is
implied by these Editor checks.

For button feedback, run `Tests/UIButtonAudioWiringSmoke.cs` in Edit mode
(105 checks covering all 46 authored buttons, including hidden panels). It also
checks the effect source/clip, settings routing, existing book sounds and absence
of a second generic click on buttons that already provide feedback.

After backing up both progress and settings, provide a nonempty saved game and
run `Tests/UIClickAudioPlaySmoke.cs` in Title Play (13 checks). Repeat in a fresh
game after opening and closing the list once to initialize its cards (23 checks).
It dispatches native UGUI pointer events and checks the effect source's actual
playback state for settings open/close, both toggles, all nine filters, New Game
cancellation and shelf selection cancellation. Right clicks and disabled controls
must remain silent; sliders must not generate repeated clicks. Effect gain and
mute must still apply. These checks do not establish audible quality or OS input.

Show Clear using `Tools > Shelf Care > Preview Clear Screen` and wait for its fade.
Set EventSystem's selected object to `TitleButton`, then run
`Tests/ClearButtonAudioSmoke.cs` (4 immediate checks). In a later CLI call verify
that the scene is TitleScene and saved progress/settings match its returned JSON.
Repeat from gameplay Clear with `ReplayButton` selected: the resulting ShelfCare
game must have no progress, while settings remain unchanged. Each test submits the
chosen button, confirms that sound starts before unload, locks both actions, and
deliberately invokes the opposite callback to test the re-entry guard. The scene
transition must complete with Time.timeScale=0. Restore Time.timeScale=1 afterwards.

The color-filter (259), shelf-movement (77), settings (20 Title / 27 game) and
native slider/toggle input (8 per scene) checks must still pass. Stop Play, restore
both backups and leave the title open. Listen to the clicks in a WebGL build too.

## Pause and return to Title

Back up progress and settings in Edit mode with both helpers above. In a fresh
ShelfCare Play game, open and close the list once to initialize its cards. Run
`Tests/PauseMenuFlowSmoke.cs` (34 checks). It uses native Button clicks to cover
room pause, the original time scale and focus, disabled underlying controls and
scrolling, mixed-color selections, filtering, scroll position, confirmation,
completion image, zoom, selected hand/shelf books, tutorial continuity and Escape
stacking. It also checks the box-opening race and exclusivity with Settings.

The flow check temporarily makes the progress controller unready so SaveNow
rejects the operation, verifies the error and unchanged live/stored progress, then
restores readiness in a finally block. This exercises a rejected save, without
simulating browser storage denial. It leaves Pause open with the error visible
and retry available. Let a frame render and run `Tests/PauseMenuPresentationSmoke.cs`
at 1920×1080 and 1366×768 (24 checks each). Repeat in the normal state after reopening
Pause. These checks cover text/glyphs, screen bounds, both button raycasts, backdrop
capture and exclusion of all covered controls from keyboard navigation.

From that error state, run `Tests/PauseSaveAndReturnSmoke.cs` (6 checks). A retry
must save and reach Title, with the original time scale. Compare progress with
`.utmp/save-smoke-expected.json`, select Continue, open/close the list and run
`Tests/SaveRestorePlaySmoke.cs` (27 checks for this one-placed/one-hand fixture).
Check the persisted music/effects/mute/motion settings as well.

In a separate fresh game, run the existing `Tests/ShelfBookMoveSmoke.cs` (77).
Its fixture has 27 placed books, nine in hand, one completed shelf and a selected
shelf book. Run `Tests/PauseSaveAndReturnSmoke.cs` again (7 checks): it makes one
last transfer and saves within the same eval, before LateUpdate can autosave.
Continue and run the restore check (105). Exact identities, hand order, wrong
positions, completed locks and remaining box books must all match.

Also take Green01, Blue01 and Red01, place Green01 at slot 1, then select Green02
and Blue02 across filters and open confirmation. Pause and save from there.
Continue must restore only the two owned hand books and one placed book (restore
check 27). Green02 and Blue02 must remain in the box; there must be no old popup,
pause, selection, filter or zoom. Stop Play while Pause is open and verify the
clock returns to its original value. Restart Play from Title and Continue again
to check storage across sessions, rather than only a scene change.

During a pause, the clear preview must close Pause, restore the clock and lock
gameplay. After Clear, Pause must stay hidden and Back must not reopen it. Run
the existing flow (36), list (27), tutorial (110), hand (49), shelf movement (77),
color filter (259), navigation (96), 140-book clear (777) and Clear presentation
(36) checks in separate fresh games where required. Audio coverage must include
the three new buttons. Restore both original backups after stopping Play,
including on failure. Browser audio, actual keyboard/pointer input and reload
persistence remain separate WebGL checks.

## UI overlap and shelf reachability

Back up progress and settings before Play as above. Use 1920×1080, 1366×768 and
1024×768 (4:3). Wait for the current view/intro to render before running
`Tests/UILayoutOverlapSmoke.cs`. Require `issueCount == 0` and an empty `issues`
array, in addition to the CLI/eval success flags. This is an audit report: it
returns all detected issues rather than throwing on the first one.

The audit enumerates visible Selectables, occupied hand pockets, rendered text
and main painted panels. It checks screen bounds, unrelated element intersections,
text overflow, and raycasts at the center and four corners of each control.
Text bounds use glyph metrics, excluding transparent SDF padding. Scroll content
is clipped to Mask/RectMask2D bounds. A modal's covered view is excluded, while
the accessible Pause button and tutorial remain included where appropriate.
This catches partial overlap that a center-only button test misses.

Cover these 22 states at all three sizes (66 snapshots):

- Room and zoomed shelf, each with and without the tutorial.
- Book list with/without the tutorial, and the fully empty box.
- Confirmation with one and nine books plus the tutorial, and nine without it.
- Pause above confirmation; selected hand book with its shelf/tutorial; selected
  shelf book with nine books in hand; completion image; Pause above that image.
- Game settings, title with/without progress, New Game confirmation, title settings,
  Pause with a rejected-save message, and Clear.

Use real selection/take actions for the confirmation fixture. Select the placed
book with an actual shelf click before checking its move controls. Keep muted
audio and stable motion for layout snapshots; run the normal-motion regressions
in separate fresh games. `Tests/BookColorFilterPresentationSmoke.cs` (84 with guide),
`Tests/ShelfBookMovePresentationSmoke.cs` (34 or 35 according to shelf length),
`Tests/TutorialPresentationSmoke.cs` and `Tests/SettingsPresentationSmoke.cs` (65)
should still pass for their corresponding states.

In an empty fresh room with no modal, run `Tests/ShelfReachabilitySmoke.cs` at each
size (421 checks). It opens all eight shelves and checks every placement center
is on screen with margin, receives no UI raycast, and receives a real 3D physics
ray at its intended slot. Leave the last shelf zoomed,
resize, let a frame render and repeat (472 when the previous 17-slot shelf is
checked first). The first check uses the existing view before reopening any
shelf, so it also verifies live camera adjustment. The tutorial smoke (110) must
pass at 1024×768; a clipped endpoint previously prevented its focus from expanding.

Existing flow (36), list (27), hand inventory (49), shelf move (77), navigation (96),
Pause (34), filters (259), 140-book placement/clear (777) and Clear presentation
(36) checks must pass. Stop Play and restore both backups. Remove any temporary
custom Game view size and restore 1920×1080. Native OS input and browser rendering
remain separate WebGL checks.

For the room's cardboard box, also run `Tests/BoxReachabilitySmoke.cs` with the
tutorial both hidden and visible at all three sizes. It projects the full enabled
Renderer bounds, checks that the visible mesh and shelf navigation do not overlap,
requires 24 reference units of vertical clearance, and raycasts UI at a 5×5 grid
of points that actually hit the box Collider (25/26 checks, 21/22 hit points for
these views). A center-only hit previously missed the covered top edge.
`UILayoutOverlapSmoke` includes the visible box mesh as an overlap participant
in room view. Finally invoke the real box mouse handler with EventSystem disabled
only for that call, restore EventSystem, and let its opening animation show the
140-book list. This is a handler/animation check, not synthesized OS input.
