# Gameplay UI smoke check

Requires the Unity CLI and `com.unity.pipeline` in the local project. The check
uses the real ShelfCare scene and its persistent button listeners. It does not
save Play mode changes. No Pipeline dependency is needed in a player build.

1. Open `Assets/Scenes/ShelfCare.unity` in the Editor and start a fresh Play session.
2. Verify the room is shown, with the book list closed and shelf buttons visible.
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
  confirmation closes. Press Escape again: the list closes.
- Zoom a shelf, use its upper-left Close button, then repeat with Escape and with
  right-click outside the UI. Each returns to the room.
- While the answer panel is open, clicking outside its graphic must not place or
  return a book behind it.
- Check the zoom Close button at 1280×720 and 960×540; it should remain fully
  visible at the top left and disappear behind modal panels.

The smoke check calls the same `Back` action as Escape/right-click, but does not
synthesize OS keyboard or mouse events. WebGL/browser input is a separate check.

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
