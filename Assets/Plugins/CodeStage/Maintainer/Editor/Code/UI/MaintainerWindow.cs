#if UNITY_EDITOR

#define UNITY_5_0_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#undef UNITY_5_0_PLUS
#endif

#define UNITY_5_1_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0
#undef UNITY_5_1_PLUS
#endif

using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Settings;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	public class MaintainerWindow : EditorWindow
	{
		private static MaintainerWindow windowInstance;

		private static bool needToRepaint;

		private int currentTab;
		private GUIContent[] tabsCaptions;

		private IssuesTab issuesTab;
		private CleanerTab cleanerTab;
		private AboutTab aboutTab;

		public static MaintainerWindow Create()
		{
			MaintainerWindow window = GetWindow<MaintainerWindow>();
			window.Init();

			return window;
		}

		public static void ShowIssues()
		{
			Create().currentTab = 0;
			MaintainerSettings.Instance.selectedTabIndex = 0;
		}

		public static void ShowCleaner()
		{
#if UNITY_5_0_PLUS
			AssetPreview.SetPreviewTextureCacheSize(50);
#endif
			ShowProjectCleanerWarning();

			Create().currentTab = 1;
			MaintainerSettings.Instance.selectedTabIndex = 1;
		}

		public static void ShowAbout()
		{
			Create().currentTab = 2;
			MaintainerSettings.Instance.selectedTabIndex = 2;
		}

		public static void ShowNotification(string text)
		{
			if (windowInstance)
			{
				windowInstance.ShowNotification(new GUIContent(text));
			}
		}

		private static void ShowProjectCleanerWarning()
		{
			if (MaintainerSettings.Cleaner.firstTime)
			{
				EditorUtility.DisplayDialog(ProjectCleaner.MODULE_NAME + " BETA", "Please note, this module is in experimental BETA mode and may have bugs and false positives.\nUse it on your own, author is not responsible for any damage made due to the module usage!\nThis message shows only once.", "Dismiss");
				MaintainerSettings.Cleaner.firstTime = false;
			}
		}

		[DidReloadScripts]
		private static void OnScriptsRecompiled()
		{
			needToRepaint = true;
		}

		private void Init()
		{
			minSize = new Vector2(750f, 400f);
			Focus();
			currentTab = MaintainerSettings.Instance.selectedTabIndex;

			CreateTabs();
			Refresh();
		}

		private void CreateTabs()
		{
			if (issuesTab == null)
				issuesTab = new IssuesTab();

			if (cleanerTab == null)
				cleanerTab = new CleanerTab();

			if (aboutTab == null)
				aboutTab = new AboutTab();

			if (tabsCaptions == null)
			{
				tabsCaptions = new[] {issuesTab.Caption, cleanerTab.Caption, aboutTab.Caption};
			}
		}

		private void Refresh()
		{
			issuesTab.Refresh();
			cleanerTab.Refresh();
		}

		private void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;
			windowInstance = this;

#if UNITY_5_1_PLUS
			titleContent = new GUIContent(" Maintainer", CSIcons.Maintainer);
#else
			title = "Maintainer";
#endif
		}

		private void OnDisable()
		{
			MaintainerSettings.Save();
			SearchResultsStorage.Save();
		}

		private void OnInspectorUpdate()
		{
			if (needToRepaint)
			{
				needToRepaint = false;
				Repaint();

				currentTab = MaintainerSettings.Instance.selectedTabIndex;
			}
		}

		private void OnGUI()
		{
			CreateTabs();

			UIHelpers.SetupStyles();

			EditorGUI.BeginChangeCheck();
			currentTab = GUILayout.Toolbar(currentTab, tabsCaptions, GUILayout.ExpandWidth(false), GUILayout.Height(21));
			if (EditorGUI.EndChangeCheck())
			{
				if (currentTab == 1) ShowProjectCleanerWarning();

				MaintainerSettings.Instance.selectedTabIndex = currentTab;
			}

			if (currentTab == 0)
			{
				issuesTab.Draw(this);
			}
			else if (currentTab == 1)
			{
				cleanerTab.Draw(this); 
			}
			else if (currentTab == 2)
			{
				aboutTab.Draw(this);
			}
		}
	}
}

#endif