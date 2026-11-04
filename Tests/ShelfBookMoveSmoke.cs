// Fresh ShelfCare game. Open the box once, let a frame create its cards, then close it.
if(!UnityEditor.EditorApplication.isPlaying||GameplayUIController.Instance.HasModal)throw new System.Exception("Run in a fresh room with a prepared box catalogue.");
int checks=0;
void Check(bool ok,string message){if(!ok)throw new System.Exception("FAIL: "+message);checks++;}
T Find<T>() where T:UnityEngine.Object=>UnityEngine.Object.FindFirstObjectByType<T>(UnityEngine.FindObjectsInactive.Include);
GameObject Named(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(t=>t.name==n).gameObject;
var move=Find<ShelfBookMoveController>();var hand=Find<HandItemController>();var place=Find<HandBookPlacementController>();var progress=Find<GameProgressController>();var ui=GameplayUIController.Instance;
var slots=UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None).OrderBy(s=>s.SlotIndex).ToArray();
Check(hand.BookCount==0&&slots.All(s=>!s.IsFilled)&&slots.Length==140,"Fresh empty game");
foreach(var audio in UnityEngine.Object.FindObjectsByType<AudioSource>(UnityEngine.FindObjectsSortMode.None))audio.volume=0;
Find<BookPlacementTutorial>().Dismiss();
void Take(string[] names){ui.OpenBookList();foreach(string n in names){var b=UnityEngine.Object.FindObjectsByType<BookButton>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(b=>b.BookSprite.name==n);b.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();}Check(Find<BookSelectionController>().SelectedCount==names.Length,"Select intended books");Find<BookSelectionController>().ShowSelectionConfirmation();Find<BookSelectionController>().TakeSelectedBooks();}
void Place(string name,int index){int i=hand.Books.Select((b,i)=>new{b,i}).Single(p=>p.b.Sprite.name==name).i;hand.SelectBook(i);Check(place.TryPlaceBook(slots[index]),"Place "+name);}
void Binding(BookSlot slot,BookObject book){Check(slot.PlacedBook==book&&slot.IsFilled&&book.GetComponent<PlacedBookClick>().CurrentSlot==slot,"Book and click handler agree at "+slot.SlotIndex);Check(Vector3.Distance(book.transform.position,slot.transform.position)<.00002f&&!slot.GetComponent<Collider2D>().enabled&&book.GetComponent<Collider2D>().enabled,"Position and colliders agree at "+slot.SlotIndex);}
Take(new[]{"Green01","Green02","Green03"});Place("Green01",0);Place("Green02",2);Place("Green03",4);
Take(Enumerable.Range(1,9).Select(i=>"Blue"+i.ToString("D2")).ToArray());
var first=slots[0].PlacedBook;var second=slots[2].PlacedBook;var third=slots[4].PlacedBook;var color=first.GetComponent<SpriteRenderer>().color;var scale=first.transform.localScale;string[] originalHand=hand.Books.Select(b=>b.Sprite.name).ToArray();
int notifications=0;
System.Action atomicObserver=()=>{notifications++;var names=slots.Where(s=>s.IsFilled).Select(s=>s.PlacedBook.GetComponent<SpriteRenderer>().sprite.name).Concat(hand.Books.Select(b=>b.Sprite.name)).ToArray();Check(names.Length==12&&names.Distinct().Count()==12,"Observers see no missing or duplicate book during transfer");Check(progress.SaveNow(),"An observer can save the complete transfer immediately");};
var shelf=slots[0].ShelfJudge;shelf.StateChanged+=atomicObserver;
try{
    hand.SelectBook(0);
    Check(move.ClickBook(slots[0])&&move.HasSelection&&!hand.HasSelectedBook&&Find<CameraController>().IsZoomed,"Selecting a shelf book clears the hand selection and opens its shelf");
    Check(!move.CanReturnToHand&&move.TryGetPreview(slots[1],out var preview)&&preview.name=="Green01","Full hands permit movement previews but not returns");
    Check(move.TryMoveSelectedBookTo(slots[1])&&!move.HasSelection&&!slots[0].IsFilled&&slots[0].GetComponent<Collider2D>().enabled,"Move to an empty position without using the hand");Binding(slots[1],first);
    Check(first.CorrectSlotIndex==0&&!slots[1].IsCorrect()&&first.transform.localScale==scale,"Rearranging preserves the book identity and scale");
    Check(move.ClickBook(slots[1])&&move.ClickBook(slots[2])&&!move.HasSelection,"A second filled book swaps the pair");Binding(slots[1],second);Binding(slots[2],first);
    Check(notifications==2,"Exactly one stable shelf notification per move or swap");
}finally{shelf.StateChanged-=atomicObserver;}
Check(hand.Books.Select(b=>b.Sprite.name).SequenceEqual(originalHand)&&hand.BookCount==9,"Moves and swaps preserve all nine hand books in order");
Check(first.GetComponent<SpriteRenderer>().color==color&&!shelf.IsCompleted,"Selection tint is restored and rearranging does not auto-complete a shelf");
Check(!slots[2].TryMoveOrSwapTo(null)&&!slots[2].TryMoveOrSwapTo(slots[2])&&!slots[2].TryMoveOrSwapTo(slots[18]),"Null, self and cross-color transfers rejected");
move.ClickBook(slots[2]);Check(!move.TryMoveSelectedBookTo(slots[18])&&move.SelectedSlot==slots[2],"Wrong-color target preserves the selection and layout");
slots[3].SetLocked(true);Check(!move.TryMoveSelectedBookTo(slots[3]),"Locked destination rejected");slots[3].SetLocked(false);
move.ClickBook(slots[2]);Check(!move.HasSelection,"Clicking the same book cancels");
move.ClickBook(slots[2]);ui.Back();Check(!move.HasSelection&&Find<CameraController>().IsZoomed,"First Back cancels without leaving the shelf");ui.Back();Check(!Find<CameraController>().IsZoomed,"Second Back returns to the room");
move.ClickBook(slots[2]);var back=new UnityEditor.SerializedObject(ui).FindProperty("_zoomBackButton").objectReferenceValue as GameObject;back.GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Check(!move.HasSelection&&!Find<CameraController>().IsZoomed,"Room button cancels selection and leaves in one click");
move.ClickBook(slots[2]);hand.SelectBook(0);Check(!move.HasSelection&&hand.HasSelectedBook,"Selecting a hand book clears the shelf selection");move.ClickBook(slots[2]);Check(!hand.HasSelectedBook&&move.HasSelection,"Selecting a shelf book clears the hand selection");
move.ReturnSelectedToHand();Check(move.HasSelection&&hand.BookCount==9&&slots[2].PlacedBook==first&&!Named("ReturnShelfBookButton").GetComponent<UnityEngine.UI.Button>().interactable,"Full-hand return leaves the book selected and disables the button");
ui.OpenBookList();Check(!move.HasSelection&&!slots[2].TryMoveOrSwapTo(slots[3])&&!move.ClickBook(slots[2]),"Book list cancels selection and blocks transfers");ui.CloseBookList();
move.ClickBook(slots[2]);Find<ShelfActionMenuController>().ShowSelectedShelfAnswer();Check(!move.HasSelection&&ui.HasModal&&!slots[2].TryMoveOrSwapTo(slots[3]),"Completion picture cancels selection and blocks transfers");ui.Back();
move.ClickBook(slots[2]);Find<BookPlacementTutorial>().BeginTutorial();Check(!move.HasSelection,"Reopening the tutorial cancels shelf selection");Find<BookPlacementTutorial>().Dismiss();
// Route the real world click handlers without depending on the OS cursor's current UI position.
var eventSystem=UnityEngine.EventSystems.EventSystem.current;eventSystem.gameObject.SetActive(false);
try{slots[2].PlacedBook.SendMessage("OnMouseDown");Check(move.SelectedSlot==slots[2],"Placed-book handler selects its current slot");slots[3].SendMessage("OnMouseDown");Check(slots[3].PlacedBook==first&&!slots[2].IsFilled,"Empty-slot handler moves the selected book");slots[3].PlacedBook.SendMessage("OnMouseDown");slots[1].PlacedBook.SendMessage("OnMouseDown");Binding(slots[1],first);Binding(slots[3],second);}finally{eventSystem.gameObject.SetActive(true);}
Place("Blue01",18);move.ClickBook(slots[1]);Check(move.CanReturnToHand,"Return becomes available after freeing hand space");Named("ReturnShelfBookButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(!move.HasSelection&&!slots[1].IsFilled&&hand.BookCount==9&&hand.Books.Last().Sprite.name=="Green01","Return button puts the selected book at the end of the hand");
for(int i=2;i<=9;i++)Place("Blue"+i.ToString("D2"),17+i);
Place("Green01",0);move.ClickBook(slots[3]);move.TryMoveSelectedBookTo(slots[1]);move.ClickBook(slots[4]);move.TryMoveSelectedBookTo(slots[2]);
Take(Enumerable.Range(4,9).Select(i=>"Green"+i.ToString("D2")).ToArray());for(int i=4;i<=12;i++)Place("Green"+i.ToString("D2"),i-1);
Take(Enumerable.Range(13,6).Select(i=>"Green"+i.ToString("D2")).ToArray());for(int i=13;i<=18;i++)Place("Green"+i.ToString("D2"),i-1);
move.ClickBook(slots[0]);Check(shelf.CheckShelf()&&shelf.IsCompleted&&!move.HasSelection,"Completing a shelf cancels selection and locks it");
Check(!move.ClickBook(slots[0])&&!slots[0].TryMoveOrSwapTo(slots[1])&&!place.TryReturnBookToHand(slots[0]),"Completed books cannot be selected, moved, swapped or returned");
move.ClickBook(slots[18]);move.TryMoveSelectedBookTo(slots[27]);move.ClickBook(slots[27]);move.ClickBook(slots[19]);
Take(Enumerable.Range(1,9).Select(i=>"Red"+i.ToString("D2")).ToArray());
var expected=GameProgressData.Empty();expected.hand=Enumerable.Range(1,9).Select(i=>"Red"+i.ToString("D2")).ToArray();expected.completedShelves=new[]{0};
expected.placed=Enumerable.Range(0,18).Select(i=>new SavedBookPlacement{book="Green"+(i+1).ToString("D2"),slot=i}).Concat(Enumerable.Range(1,9).Select(i=>new SavedBookPlacement{book="Blue"+i.ToString("D2"),slot=i==1?19:i==2?27:17+i})).OrderBy(b=>b.slot).ToArray();
Check(expected.IsValid(),"Expected moved layout remains compatible with save version 1");System.IO.File.WriteAllText(".utmp/save-smoke-expected.json",JsonUtility.ToJson(expected));
Check(progress.SaveNow()&&GameProgressStore.Read(out var saved)==ProgressReadResult.Loaded&&JsonUtility.ToJson(saved)==JsonUtility.ToJson(expected),"Moved and swapped positions are saved with the original format");
move.ClickBook(slots[19]);
return new{passed=checks,placed=27,hand=9,completed=1,selected=move.SelectedSlot.SlotIndex};
