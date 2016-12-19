#if UNITY_EDITOR

using System;
using CodeStage.Maintainer.Cleaner;

namespace CodeStage.Maintainer.Settings
{
	[Serializable]
	public class ProjectCleanerSettings
	{
		/* filtering */

		public string[] pathIgnores = new string[0];
		public string[] sceneIgnores = new string[0];

		public bool ignoreScenesInBuild = true;
		public bool ignoreOnlyEnabledScenesInBuild = true;

		public int filtersTabIndex = 0;

		/* what to find */

		public bool findUnusedAssets;
		public bool findEmptyFolders;
		public bool findEmptyFoldersAutomatically;

		/* sorting */

		public CleanerSortingType sortingType = CleanerSortingType.BySize;
		public SortingDirection sortingDirection = SortingDirection.Descending;

		/* misc */

		public bool useTrashBin;
		public bool firstTime = true;


		public ProjectCleanerSettings()
		{
			Reset();
		}

		internal void Reset()
		{
			useTrashBin = true;

			findUnusedAssets = true;
			findEmptyFolders = true;
			findEmptyFoldersAutomatically = false;
		}

		internal void SwitchAll(bool enable)
		{
			findEmptyFolders = enable;
		}
	}
}

#endif