#define UNITY_5_1_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0
#undef UNITY_5_1_PLUS
#endif

#if UNITY_EDITOR

using CodeStage.Maintainer.Settings;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CodeStage.Maintainer.UI.Filters
{
	internal abstract class FiltersWindow : EditorWindow
	{
		internal delegate void TabChangeCallback(int newTab);

		private static bool needToRepaint;

		private event TabChangeCallback TabChangedCallback;

		private TabBase[] tabs;
		private GUIContent[] tabsCaptions;
        private TabBase currentTab;
        private int currentTabIndex;

		private Event currentEvent;
		private EventType currentEventType;

		protected void Init(string caption, TabBase[] windowTabs, int initialTab, TabChangeCallback tabChangeCallback)
		{
#if UNITY_5_1_PLUS
			titleContent = new GUIContent(caption + " Filters");
#else
			title = caption + " Filters";
#endif

			minSize = new Vector2(600f, 300f);

			TabChangedCallback = tabChangeCallback;

			if (windowTabs != null && windowTabs.Length > 0)
			{
				tabs = windowTabs;

				currentTabIndex = windowTabs.Length > initialTab ? initialTab : 0;

				currentTab = windowTabs[currentTabIndex];
				currentTab.Show(this);

				GUIContent[] captions = new GUIContent[windowTabs.Length];

				for (int i = 0; i < windowTabs.Length; i++)
				{
					captions[i] = windowTabs[i].caption;
				}

				tabsCaptions = captions; 
			}
			else 
			{
				Debug.LogError(Maintainer.LOG_PREFIX + "no tabs were passed to the Filters Window!");
			}
		}

		protected abstract void InitOnEnable();
		protected abstract void UnInitOnDisable();

		protected virtual void OnGUI()
		{
			UIHelpers.SetupStyles();

			currentEvent = Event.current;
			currentEventType = currentEvent.type;

			EditorGUI.BeginChangeCheck();

			EditorGUI.BeginChangeCheck();
			currentTabIndex = GUILayout.Toolbar(currentTabIndex, tabsCaptions, UIHelpers.richButton, GUILayout.Height(21));
			if (EditorGUI.EndChangeCheck())
			{
				RemoveNotification();
			}

			currentTab = tabs[currentTabIndex];
			if (EditorGUI.EndChangeCheck()) 
			{
				currentTab.Show(this);
				if (TabChangedCallback != null)
				{
					TabChangedCallback.Invoke(currentTabIndex);
				}
			}

			currentTab.currentEvent = currentEvent;
			currentTab.currentEventType = currentEventType;

			currentTab.ProcessDrags();
			currentTab.Draw();
		}

		[DidReloadScripts]
		private static void OnScriptsRecompiled()
		{
			needToRepaint = true;
		}

		private void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;
			InitOnEnable();
		}

		private void OnDisable()
		{
			MaintainerSettings.Save();
			UnInitOnDisable();
		}

		private void OnInspectorUpdate()
		{
			if (needToRepaint)
			{
				needToRepaint = false;
				Repaint();
			}
		}
	}
}

#endif