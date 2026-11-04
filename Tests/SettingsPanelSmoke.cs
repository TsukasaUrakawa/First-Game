// Play in TitleScene or a gameplay room/shelf. Back up progress/settings first.
if(!UnityEditor.EditorApplication.isPlaying)throw new System.Exception("Run in Play mode.");
int checks=0;void Check(bool ok,string message){if(!ok)throw new System.Exception("FAIL: "+message);checks++;}
GameObject Named(string n)=>UnityEngine.Object.FindObjectsByType<Transform>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None).Single(t=>t.name==n).gameObject;
var settings=UnityEngine.Object.FindFirstObjectByType<SettingsPanelController>();var title=UnityEngine.Object.FindFirstObjectByType<TitleSceneController>();bool game=title==null;
GamePreferences.SetMusicVolume(1);GamePreferences.SetEffectsVolume(1);GamePreferences.SetMuted(false);GamePreferences.SetReducedMotion(false);settings.SaveNow();
var sources=UnityEngine.Object.FindObjectsByType<AudioSource>(UnityEngine.FindObjectsInactive.Include,UnityEngine.FindObjectsSortMode.None);var baseline=sources.ToDictionary(s=>s,s=>s.volume);var muted=sources.ToDictionary(s=>s,s=>s.mute);var music=sources.Single(s=>s.name==(game?"GameBGM":"TitleBGM"));
string progress=PlayerPrefs.GetString(GameProgressStore.PreferenceKey,"");float listener=AudioListener.volume;
var ui=GameplayUIController.Instance;var camera=game?UnityEngine.Object.FindFirstObjectByType<CameraController>():null;bool zoomed=game&&camera.IsZoomed;var position=game?camera.transform.position:Vector3.zero;
var hand=game?UnityEngine.Object.FindFirstObjectByType<HandItemController>():null;string[] handBooks=game?hand.Books.Select(b=>b.Sprite.name).ToArray():new string[0];
var tutorial=game?UnityEngine.Object.FindFirstObjectByType<BookPlacementTutorial>():null;bool running=game&&tutorial.IsRunning;
var box=game?UnityEngine.Object.FindFirstObjectByType<BoxOpenAndClose>().GetComponent<Animator>():null;if(game)box.SetBool("IsOpen",true);
var access=Named("OpenSettingsButton").GetComponent<UnityEngine.UI.Button>();UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(access.gameObject);access.onClick.Invoke();
Check(settings.IsOpen,"Settings opens");Check(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==Named("CloseSettingsButton"),"Close receives keyboard focus");settings.Open();Check(settings.IsOpen,"Opening twice is harmless");
if(game){
    Check(ui.HasModal&&ui.BlocksWorldInput,"Settings blocks world input");ui.OpenBookList();Check(!ui.IsBookListOpen,"Opening the box behind settings is rejected");
    Check(!box.GetBool("IsOpen"),"Opening settings cancels an unfinished box-opening animation");
    var slots=UnityEngine.Object.FindObjectsByType<BookSlot>(UnityEngine.FindObjectsSortMode.None);foreach(var slot in slots.Where(s=>!s.IsFilled).Take(1))Check(!HandBookPlacementController.Instance.TryPlaceBook(slot),"Placement behind settings is rejected");
    foreach(var slot in slots.Where(s=>s.IsFilled&&!s.IsLocked).Take(1))Check(!HandBookPlacementController.Instance.TryReturnBookToHand(slot),"Return behind settings is rejected");
    string status=Named("CurrentShelfStatus").GetComponent<TMPro.TMP_Text>().text;Named("JudgeButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Check(Named("CurrentShelfStatus").GetComponent<TMPro.TMP_Text>().text==status,"Shelf judgement behind settings is rejected");
    if(running)Check(tutorial.IsRunning&&!Named("TutorialPanel").activeInHierarchy,"Tutorial is hidden without completing it");
    Check(!Named("OpenSettingsButton").activeInHierarchy,"Underlying access is hidden in gameplay");
}else{
    title.StartGame();title.StartNewGame();title.QuitGame();Check(!DG.Tweening.DOTween.IsTweening(title)&&!Named("NewGameDialog").activeSelf,"Underlying title actions cannot start a transition");
    Check(!Named("StartButton").GetComponent<UnityEngine.UI.Button>().IsInteractable()&&!access.interactable,"Underlying title controls are excluded from keyboard navigation");
}
Named("MusicVolumeSlider").GetComponent<UnityEngine.UI.Slider>().value=.25f;Named("EffectsVolumeSlider").GetComponent<UnityEngine.UI.Slider>().value=.6f;
Check(GamePreferences.MusicVolume==.25f&&GamePreferences.EffectsVolume==.6f&&Named("MusicVolumePercent").GetComponent<TMPro.TMP_Text>().text=="25%"&&Named("EffectsVolumePercent").GetComponent<TMPro.TMP_Text>().text=="60%","Sliders update preferences and percentages");
foreach(var source in sources)Check(Mathf.Abs(source.volume-baseline[source]*(source == music ? .25f : .6f))<.0001f,"Authored mix scaled correctly for "+source.name);
Named("MuteAudioToggle").GetComponent<UnityEngine.UI.Toggle>().isOn=true;Check(sources.All(s=>s.mute),"Mute silences music and all effects");
Named("MusicVolumeSlider").GetComponent<UnityEngine.UI.Slider>().value=0;Check(music.volume==0&&music.isPlaying,"Zero volume does not stop or restart music");Named("MusicVolumeSlider").GetComponent<UnityEngine.UI.Slider>().value=.25f;
Named("MuteAudioToggle").GetComponent<UnityEngine.UI.Toggle>().isOn=false;Check(sources.All(s=>s.mute==muted[s])&&GamePreferences.MusicVolume==.25f&&GamePreferences.EffectsVolume==.6f,"Unmute restores authored mute flags and volume choices");
Named("MuteAudioToggle").GetComponent<UnityEngine.UI.Toggle>().isOn=true;Named("PreviewEffectsButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();Check(music.isPlaying&&sources.All(s=>s.mute)&&AudioListener.volume==listener,"Preview respects mute without changing the global listener");
Named("ReducedMotionToggle").GetComponent<UnityEngine.UI.Toggle>().isOn=true;Check(GamePreferences.ReducedMotion,"Motion toggle applies live");
if(game)ui.Back();else Named("CloseSettingsButton").GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
Check(!settings.IsOpen&&!GamePreferences.IsDirty,"Closing saves and dismisses settings");Check(GamePreferences.TryParse(PlayerPrefs.GetString(GamePreferences.PreferenceKey),out var saved)&&saved.music==.25f&&saved.effects==.6f&&saved.muted&&saved.reducedMotion,"Saved settings contain every current value");
Check(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject==access.gameObject,"Closing restores keyboard focus");
if(game){Check(camera.IsZoomed==zoomed&&camera.transform.position==position&&!ui.HasModal,"Closing preserves the camera and removes only the settings modal");Check(hand.Books.Select(b=>b.Sprite.name).SequenceEqual(handBooks),"Settings does not alter inventory");if(running)Check(tutorial.IsRunning&&Named("TutorialPanel").activeInHierarchy,"Tutorial resumes at its current step");}
else {Check(title.CanUseTitleActions&&Named("StartButton").GetComponent<UnityEngine.UI.Button>().IsInteractable(),"Title actions are restored");if(Named("NewGameButton").activeSelf){title.StartNewGame();Check(!access.IsInteractable(),"New Game confirmation disables settings access");settings.Open();Check(!settings.IsOpen,"Settings cannot open above New Game confirmation");title.CancelNewGame();}}
Check(PlayerPrefs.GetString(GameProgressStore.PreferenceKey,"")==progress,"Settings does not overwrite progress");
access.onClick.Invoke();return new{passed=checks,scene=game?"game":"title",audioSources=sources.Length,musicPlaying=music.isPlaying,open=settings.IsOpen};
