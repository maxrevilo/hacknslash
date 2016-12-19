#if UNITY_EDITOR

using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Settings;

namespace CodeStage.Maintainer.UI.Filters
{
	internal class IssuesFiltersWindow : FiltersWindow
	{
		internal static IssuesFiltersWindow instance;

		internal static IssuesFiltersWindow Create()
		{
			IssuesFiltersWindow window = GetWindow<IssuesFiltersWindow>(true);
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
				new SceneFiltersTab(FilterType.Includes, 
									"Included scenes will be checked for issues.",
									MaintainerSettings.Issues.sceneIncludes,
									MaintainerSettings.Issues.includeScenesInBuild,
									MaintainerSettings.Issues.includeOnlyEnabledScenesInBuild, 
									OnSceneIgnoresSettingsChange, OnSceneIncludesChange),

				new PathFiltersTab(FilterType.Includes, MaintainerSettings.Issues.pathIncludes, true, OnPathIncludesChange),
				new PathFiltersTab(FilterType.Ignores, MaintainerSettings.Issues.pathIgnores, true, OnPathIgnoresChange),
				new ComponentFiltersTab(FilterType.Ignores, MaintainerSettings.Issues.componentIgnores, OnComponentIgnoresChange)
			};

			Init(IssuesFinder.MODULE_NAME, tabs, MaintainerSettings.Issues.filtersTabIndex, OnTabChange);

			instance = this;
		}

		protected override void UnInitOnDisable()
		{
			instance = null;
		}

		private static void OnSceneIncludesChange(string[] collection)
		{
			MaintainerSettings.Issues.sceneIncludes = collection;
		}

		private void OnSceneIgnoresSettingsChange(bool ignoreScenesInBuild, bool ignoreOnlyEnabledScenesInBuild)
		{
			MaintainerSettings.Issues.includeScenesInBuild = ignoreScenesInBuild;
			MaintainerSettings.Issues.includeOnlyEnabledScenesInBuild = ignoreOnlyEnabledScenesInBuild;
		}

		private void OnPathIgnoresChange(string[] collection)
		{
			MaintainerSettings.Issues.pathIgnores = collection;
		}

		private void OnPathIncludesChange(string[] collection)
		{
			MaintainerSettings.Issues.pathIncludes = collection;
		}

		private void OnComponentIgnoresChange(string[] collection)
		{
			MaintainerSettings.Issues.componentIgnores = collection;
		}

		private void OnTabChange(int newTab)
		{
			MaintainerSettings.Issues.filtersTabIndex = newTab;
		}
	}
}

#endif