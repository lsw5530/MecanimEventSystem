
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

public class MecanimEventEditor : EditorWindow {
	public static MecanimEventInspector eventInspector;
	
	public float PlaybackTime {
		get { return playbackTime; }
	}
	
	public UnityEngine.Object TargetController {
		set {
			targetController = value as AnimatorController;
		}
	}
	
	private AnimatorController targetController;
	private StateMachine targetStateMachine;
	private State targetState;
	private MecanimEvent targetEvent;
	
	private List<MecanimEvent> displayEvents;
	
	static void Init () {
		EditorWindow.GetWindow<MecanimEventEditor>();
	}
	
	void OnEnable() {
		minSize = new Vector2(850,320);
	}
	
	void OnDisable() {
		MecanimEventEditorPopup.Destroy();
		
		if (eventInspector != null) {
			eventInspector.SetPreviewMotion(null);
			eventInspector.SaveData();
		}
	}
	
	void OnInspectorUpdate() {
		Repaint();
	}
	
	public void DelEvent(MecanimEvent e) {
		if (displayEvents != null) {
			displayEvents.Remove(e);
			SaveState();
		}
	}
	
	void Reset() {
		displayEvents = null;
		
		targetController = null;
		targetStateMachine = null;
		targetState = null;
		targetEvent = null;
		
		selectedLayer = 0;
		selectedState = 0;
		selectedEvent = 0;
		
		MecanimEventEditorPopup.Destroy();
	}
	
	public KeyValuePair<string, EventConditionParamTypes>[] GetConditionParameters() {
		List<KeyValuePair<string, EventConditionParamTypes>> ret = new List<KeyValuePair<string, EventConditionParamTypes>>();
		if (targetController != null) {
			for (int i = 0; i < targetController.GetEventCount(); i++) {
				switch(targetController.GetEventType(i)) {
				case 1:		// float
					ret.Add(new KeyValuePair<string, EventConditionParamTypes>(targetController.GetEventName(i), EventConditionParamTypes.Float));
					break;
				case 3:		// int
					ret.Add(new KeyValuePair<string, EventConditionParamTypes>(targetController.GetEventName(i), EventConditionParamTypes.Int));
					break;
				case 4:		// bool
					ret.Add(new KeyValuePair<string, EventConditionParamTypes>(targetController.GetEventName(i), EventConditionParamTypes.Boolean));
					break;
				}
			}
		}
		
		return ret.ToArray();
	}
	
	private void SaveState() {
		if (targetController != null && targetState != null)
			eventInspector.SetEvents(targetController, selectedLayer, targetState.GetUniqueNameHash(), displayEvents.ToArray());
	}
	
	Vector2 controllerPanelScrollPos;
	int selectedController = 0;
	AnimatorController controllerToAdd;
	
	void DrawControllerPanel() {
		
		GUILayout.BeginVertical(GUILayout.Width(200));
		
		// controller to add field.
		GUILayout.BeginHorizontal(); {
			
			controllerToAdd = EditorGUILayout.ObjectField(controllerToAdd, typeof(AnimatorController), false) as AnimatorController;
			
			EditorGUI.BeginDisabledGroup(controllerToAdd == null);
			
			if (GUILayout.Button("Add", GUILayout.ExpandWidth(true), GUILayout.Height(16))) {
				eventInspector.AddController(controllerToAdd);
			}
			
			EditorGUI.EndDisabledGroup();

			//GUILayout.Button("Del", EditorStyles.toolbarButton, GUILayout.Width(38), GUILayout.Height(16));
			
			GUILayout.Space(4);
		
		}
		GUILayout.EndHorizontal();
		
		// controller list
		
		GUILayout.BeginVertical("Box");
		controllerPanelScrollPos = GUILayout.BeginScrollView(controllerPanelScrollPos);
		
		AnimatorController[] controllers = eventInspector.GetControllers();
			
		string [] controllerNames = new string[controllers.Length];
		
		for (int i = 0; i < controllers.Length; i++) {
			
			controllerNames[i] = controllers[i].name;
			
		}
		
		selectedController = GUILayout.SelectionGrid(selectedController, controllerNames, 1);
		
		if (selectedController >= 0 && selectedController < controllers.Length) {
			
			targetController = controllers[selectedController];
			
			eventInspector.SaveLastEditController(targetController);
			
		}
		else {
			targetController = null;
			targetStateMachine = null;
			targetState = null;
			targetEvent = null;
		}
			

		GUILayout.EndScrollView();
		GUILayout.EndVertical();
		
		
		GUILayout.EndVertical();
		
	}
	
	Vector2 layerPanelScrollPos;
	int selectedLayer = 0;
	
	void DrawLayerPanel() {
		
		GUILayout.BeginVertical(GUILayout.Width(200));
		
		if (targetController != null) {
		
			int layerCount = targetController.GetLayerCount();	
			GUILayout.Label(layerCount + " layer(s) in selected controller");
			
			GUILayout.BeginVertical("Box");
			layerPanelScrollPos = GUILayout.BeginScrollView(layerPanelScrollPos);
			
			string[] layerNames = new string[layerCount];
			
			for (int layer = 0; layer < layerCount; layer++) {
				layerNames[layer] = "[" + layer.ToString() + "]" + targetController.GetLayerName(layer);
			}
			
			selectedLayer = GUILayout.SelectionGrid(selectedLayer, layerNames, 1);
			
			if (selectedLayer >= 0 && selectedLayer < layerCount) {
				targetStateMachine = targetController.GetLayerStateMachine(selectedLayer);
			}
			else {
				targetStateMachine = null;
				targetState = null;
				targetEvent = null;
			}
			
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			
		}
		else {
			GUILayout.Label("No layer available.");
		}
		
		GUILayout.EndVertical();
	}
	
	Vector2 statePanelScrollPos;
	int selectedState = 0;
	
	void DrawStatePanel() {
		
		GUILayout.BeginVertical(GUILayout.Width(200));
		
		if (targetStateMachine != null) {
			
			List<State> availableStates = targetStateMachine.statesRecursive;
			List<string> stateNames = new List<string>();
			
			foreach (State s in availableStates) {
				stateNames.Add(s.GetUniqueName());
			}
			
			GUILayout.Label(availableStates.Count + " state(s) in selected layer.");
			
			GUILayout.BeginVertical("Box");
			statePanelScrollPos = GUILayout.BeginScrollView(statePanelScrollPos);
			
			selectedState = GUILayout.SelectionGrid(selectedState, stateNames.ToArray(), 1);
			
			if (selectedState >= 0 && selectedState < availableStates.Count) {
				targetState = availableStates[selectedState];
			}
			else {
				targetState = null;
				targetEvent = null;
			}
			
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			
		}
		else {
			
			GUILayout.Label("No state machine available.");
		}
		
		GUILayout.EndVertical();
	}
	
	Vector2 eventPanelScrollPos;
	int selectedEvent = 0;
	
	void DrawEventPanel() {
		
		GUILayout.BeginVertical();
		
		if (targetState != null) {
			
			displayEvents = new List<MecanimEvent>(eventInspector.GetEvents(targetController, selectedLayer, targetState.GetUniqueNameHash()));
			displayEvents.Sort(
				delegate(MecanimEvent a, MecanimEvent b) 
				{
					return a.normalizedTime.CompareTo(b.normalizedTime); 
				} 
			);
			
			GUILayout.Label(displayEvents.Count + " event(s) in this state.");
			
			List<string> eventNames = new List<string>();
			
			foreach (MecanimEvent e in displayEvents) {
				eventNames.Add(string.Format("{0}({1})@{2}", e.functionName, e.parameter, e.normalizedTime.ToString("0.0000")));
			}
			
			GUILayout.BeginVertical("Box");
			eventPanelScrollPos = GUILayout.BeginScrollView(eventPanelScrollPos);
			
			selectedEvent = GUILayout.SelectionGrid(selectedEvent, eventNames.ToArray(), 1);
			
			if (selectedEvent >= 0 && selectedEvent < displayEvents.Count) {
				targetEvent = displayEvents[selectedEvent];
			}
			else {
				targetEvent = null;
			}
			
			GUILayout.EndScrollView();
			GUILayout.EndVertical();
			
		}
		else {
			GUILayout.Label("No event.");
		}
		
		GUILayout.EndVertical();
	}
	
	private float playbackTime = 0.0f;
	
	private bool enableTempPreview = false;
	private float tempPreviewPlaybackTime = 0.0f;
	
	private static int timelineHash = "timelinecontrol".GetHashCode();
	
	void DrawTimelinePanel() {
		
		if (!enableTempPreview)
			playbackTime = eventInspector.GetPlaybackTime();
		
		
		GUILayout.BeginVertical(); {
			
			GUILayout.Space(10);
		
			GUILayout.BeginHorizontal(); {
				
				GUILayout.Space(20);
				
				playbackTime = Timeline(playbackTime);
				
				GUILayout.Space(10);
				
			}
			GUILayout.EndHorizontal();
			
			GUILayout.FlexibleSpace();
			
			GUILayout.BeginHorizontal(); {
				
				GUILayout.FlexibleSpace();
				
				if (GUILayout.Button("Add", GUILayout.Width(80))) {
					MecanimEvent newEvent = new MecanimEvent();
					newEvent.normalizedTime = playbackTime;
					newEvent.functionName = "MessageName";
					newEvent.paramType = MecanimEventParamTypes.None;
					
					displayEvents.Add(newEvent);
					MecanimEventEditorPopup.Show(this, newEvent, GetConditionParameters());
				}
				
				if (GUILayout.Button("Del", GUILayout.Width(80))) {
					DelEvent(targetEvent);
				}
				
				EditorGUI.BeginDisabledGroup(targetEvent == null);
				
				if (GUILayout.Button("Edit", GUILayout.Width(80))) {
					MecanimEventEditorPopup.Show(this, targetEvent, GetConditionParameters());
				}
				
				EditorGUI.EndDisabledGroup();
				
				if (GUILayout.Button("Save", GUILayout.Width(80))) {
					eventInspector.SaveData();
				}
			}
			GUILayout.EndHorizontal();
		
		}
		GUILayout.EndVertical();
		
		if (enableTempPreview) {
			eventInspector.SetPlaybackTime(tempPreviewPlaybackTime);
			eventInspector.StopPlaying();
		}
		else {
			eventInspector.SetPlaybackTime(playbackTime);
		}
		
		SaveState();
	}
	
	void OnGUI() {
		if (eventInspector == null) {
			Reset();
			ShowNotification(new GUIContent("Select a MecanimEventData object first."));
			return;
		}
		
		RemoveNotification();
		
		GUILayout.BeginHorizontal(); {
			
			EditorGUI.BeginChangeCheck();
			
			DrawControllerPanel();
			
			DrawLayerPanel();
			
			DrawStatePanel();
			
			if (EditorGUI.EndChangeCheck()) {
				MecanimEventEditorPopup.Destroy();
			}
			
			DrawEventPanel();
			
		}
		GUILayout.EndHorizontal();
		
		if (targetState != null && targetState.GetMotion(0) != null) {
			eventInspector.SetPreviewMotion(targetState.GetMotion(0));
		}
		else {
			eventInspector.SetPreviewMotion(null);
		}
		
		GUILayout.Space(5);
		
		GUILayout.BeginHorizontal(GUILayout.MaxHeight(100)); {
			
			
			
			DrawTimelinePanel();
			
		}
		GUILayout.EndHorizontal();
		
	}
	
	private float Timeline(float time) {
		
		Rect rect = GUILayoutUtility.GetRect(500, 10000, 50, 50);
		
		int timelineId = GUIUtility.GetControlID(timelineHash, FocusType.Native, rect);
		
		Rect thumbRect = new Rect(rect.x + rect.width * time - 5, rect.y + 2, 10, 10);
		
		Event e = Event.current;
		
		switch(e.type) {
		case EventType.Repaint:
			Rect lineRect = new Rect(rect.x, rect.y+10, rect.width, 1.5f);
			DrawTimeLine(lineRect, time);
			GUI.skin.horizontalSliderThumb.Draw(thumbRect, new GUIContent(), timelineId);
			break;
			
		case EventType.MouseDown:
			if (thumbRect.Contains(e.mousePosition)) {
				GUIUtility.hotControl = timelineId;
				e.Use();
			}
			break;
			
		case EventType.MouseUp:
			if (GUIUtility.hotControl == timelineId) {
				GUIUtility.hotControl = 0;
				e.Use();
			}
			break;
			
		case EventType.MouseDrag:
			if (GUIUtility.hotControl == timelineId) {
				
				Vector2 guiPos = e.mousePosition;
				float clampedX = Mathf.Clamp(guiPos.x, rect.x, rect.x + rect.width);
				time = (clampedX - rect.x) / rect.width;
				
				e.Use();
			}
			break;
		}
		
		if (displayEvents != null) {
		
			foreach(MecanimEvent me in displayEvents) {
				
				if (me == targetEvent)
					continue;
				
				DrawEventKey(rect, me);
			}
			
			if (targetEvent != null)
				DrawEventKey(rect, targetEvent);
			
		}
		
		return time;
	}
	
	private void DrawTimeLine(Rect rect, float currentFrame) {
		if (Event.current.type != EventType.Repaint)
		{
			return;
		}
		
		HandleUtilityWrapper.handleWireMaterial.SetPass(0);
		Color c = new Color(1f, 0f, 0f, 0.75f);
		GL.Color(c);
		
		GL.Begin(GL.LINES);
		GL.Vertex3(rect.x, rect.y, 0);
		GL.Vertex3(rect.x + rect.width, rect.y, 0);
		
		GL.Vertex3(rect.x, rect.y+25, 0);
		GL.Vertex3(rect.x + rect.width, rect.y+25, 0);

		
		for(int i = 0; i <= 100; i+=1) {
			if (i % 10 == 0) {
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y, 0);
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y + 15, 0);
			}
			else if (i % 5 == 0){
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y, 0);
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y + 10, 0);
			}
			else {
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y, 0);
				GL.Vertex3(rect.x + rect.width*i/100f, rect.y + 5, 0);
			}
		}
		
		c = new Color(1.0f, 1.0f, 1.0f, 0.75f);
		GL.Color(c);
		
		GL.Vertex3(rect.x + rect.width*currentFrame, rect.y, 0);
		GL.Vertex3(rect.x + rect.width*currentFrame, rect.y + 20, 0);
		
		GL.End();
	}
	
	private void SetActiveEvent(MecanimEvent key) {
		int i =  displayEvents.IndexOf(key);
		if (i >= 0) {
			selectedEvent = i;
			targetEvent = key;
		}
	}
	
	private int hotEventKey = 0;
	
	private void DrawEventKey(Rect rect, MecanimEvent key) {
		float keyTime = key.normalizedTime;
		
		Rect keyRect = new Rect(rect.x + rect.width * keyTime - 3, rect.y+25, 6, 18);
		
		int eventKeyCtrl = key.GetHashCode();
		
		Event e = Event.current;
		
		switch(e.type) {
		case EventType.Repaint:
			Color savedColor = GUI.color;
			
			if (targetEvent == key)
				GUI.color = Color.red;
			else
				GUI.color = Color.green;
			
			GUI.skin.button.Draw(keyRect, new GUIContent(), eventKeyCtrl);
			
			GUI.color = savedColor;
			
			if (hotEventKey == eventKeyCtrl || (hotEventKey == 0 && keyRect.Contains(e.mousePosition))) {
				string labelString = string.Format("{0}({1})@{2}", key.functionName, key.parameter, key.normalizedTime.ToString("0.0000"));
				Vector2 size = EditorStyles.largeLabel.CalcSize(new GUIContent(labelString));
				
				Rect infoRect= new Rect(rect.x + rect.width * keyTime - size.x/2, rect.y + 50, size.x, size.y);
				EditorStyles.largeLabel.Draw(infoRect, new GUIContent(labelString), eventKeyCtrl);
			}
			break;
			
		case EventType.MouseDown:
			if (keyRect.Contains(e.mousePosition)) {
				
				hotEventKey = eventKeyCtrl;
				enableTempPreview =true;
				tempPreviewPlaybackTime = key.normalizedTime;
				
				SetActiveEvent(key);
				
				if (e.clickCount > 1)
					MecanimEventEditorPopup.Show(this, key, GetConditionParameters());
				
				e.Use();	
			}
			break;
			
		case EventType.MouseDrag:
			if (hotEventKey == eventKeyCtrl) {
				
				if (e.button == 0) {
					Vector2 guiPos = e.mousePosition;
					float clampedX = Mathf.Clamp(guiPos.x, rect.x, rect.x + rect.width);
					key.normalizedTime = (clampedX - rect.x) / rect.width;
					tempPreviewPlaybackTime = key.normalizedTime;
					
					SetActiveEvent(key);
				}
				
				e.Use();
			}
			break;
			
		case EventType.MouseUp:
			if (hotEventKey == eventKeyCtrl) {
				
				hotEventKey = 0;
				enableTempPreview = false;
				eventInspector.SetPlaybackTime(playbackTime);		// reset to original time
				
				if (e.button == 1)
					MecanimEventEditorPopup.Show(this, key, GetConditionParameters());
				
				e.Use();
			}
			break;
		}
	}
}
