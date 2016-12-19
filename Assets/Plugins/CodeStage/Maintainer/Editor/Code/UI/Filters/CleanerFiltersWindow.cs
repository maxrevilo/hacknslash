#if UNITY_EDITOR

using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Settings;

namespace CodeStage.Maintainer.UI.Filters
{
	internal class CleanerFiltersWindow : FiltersWindow
	{
		internal static CleanerFiltersWindow instance;

		internal static CleanerFiltersWindow Create()
		{
			CleanerFiltersWindow window = GetWindow<CleanerFiltersWindow>(true);
			window.Focus();

			return window;
		}

		internal static void Refresh()
		{
			if (instance == null) return;

			instance.InitOnEnable();
			instance.Focus();
		}

		protected override void InitOnEnable()
		{
			TabBase[] tabs =
			{
				new SceneFiltersTab(FilterType.Ignores, 
									"Ignored scenes will be considered as needed and everything used in them will be excluded from the garbage search.", 
									MaintainerSettings.Cleaner.sceneIgnores, 
									MaintainerSettings.Cleaner.ignoreScenesInBuild, 
									MaintainerSettings.Cleaner.ignoreOnlyEnabledScenesInBuild, 
									OnSceneIgnoresSettingsChange, OnSceneIgnoresChange),

				new PathFiltersTab(FilterType.Ignores, MaintainerSettings.Cleaner.pathIgnores, false, OnPathIgnoresChange),
			};

			Init(ProjectCleaner.MODULE_NAME, tabs, MaintainerSettings.Cleaner.filtersTabIndex, OnTabChange);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private static void OnPathIgnoresChange(string[] collection)
		{
			MaintainerSettings.Cleaner.pathIgnores = collection;
		}

		private static void OnSceneIgnoresChange(string[] collection)
		{
			MaintainerSettings.Cleaner.sceneIgnores = collection;
		}

		private void OnSceneIgnoresSettingsChange(bool ignoreScenesInBuild, bool ignoreOnlyEnabledScenesInBuild)
		{
			MaintainerSettings.Cleaner.ignoreScenesInBuild = ignoreScenesInBuild;
			MaintainerSettings.Cleaner.ignoreOnlyEnabledScenesInBuild = ignoreOnlyEnabledScenesInBuild;
		}

		private void OnTabChange(int newTab)
		{
			MaintainerSettings.Cleaner.filtersTabIndex = newTab;
		}
	}
}

#endif