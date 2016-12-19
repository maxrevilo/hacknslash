#if UNITY_EDITOR

#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#endif

using System;
using System.IO;
using System.Linq;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI.Filters;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class CleanerTab : RecordsTab<CleanerRecord>
	{
		private CleanerResultsStats resultsStats;

		private GUIContent caption;
		internal GUIContent Caption
		{
			get
			{
				if (caption == null)
				{
					caption = new GUIContent(ProjectCleaner.MODULE_NAME, CSIcons.Clean);
				}
				return caption;
			}
		}

		protected override CleanerRecord[] LoadLastRecords()
		{
			CleanerRecord[] loadedRecords = SearchResultsStorage.CleanerSearchResults;
			if (loadedRecords == null) loadedRecords = new CleanerRecord[0];
			if (resultsStats == null) resultsStats = new CleanerResultsStats();
			
			return loadedRecords;
		}

		protected override void PerformPostRefreshActions()
		{
			base.PerformPostRefreshActions();
			resultsStats.Update(filteredRecords);
		}

		protected override void DrawSettingsBody()
		{
			using (layout.Vertical(UIHelpers.panelWithBackground))
			{
				GUILayout.Space(5);

				if (UIHelpers.ImageButton("Manage Filters...", CSIcons.Gear))
				{
					CleanerFiltersWindow.Create();
				}

				GUILayout.Space(5);

				MaintainerSettings.Cleaner.useTrashBin = EditorGUILayout.ToggleLeft(new GUIContent("Use Trash Bin", "All deleted items will be moved to Trash if selected. Otherwise items will be deleted permanently."), MaintainerSettings.Cleaner.useTrashBin);
				UIHelpers.Separator();
				GUILayout.Space(5);
				GUILayout.Label("<b><size=12>Search for:</size></b>", UIHelpers.richLabel);
				MaintainerSettings.Cleaner.findUnusedAssets = EditorGUILayout.ToggleLeft(new GUIContent("Unused assets", "Search for unused assets in project."), MaintainerSettings.Cleaner.findUnusedAssets, GUILayout.Width(100));
				using (layout.Horizontal())
				{
					MaintainerSettings.Cleaner.findEmptyFolders = EditorGUILayout.ToggleLeft(new GUIContent("Empty folders", "Search for all empty folders in project."), MaintainerSettings.Cleaner.findEmptyFolders, GUILayout.Width(100));
					GUI.enabled = MaintainerSettings.Cleaner.findEmptyFolders;

					EditorGUI.BeginChangeCheck();
					MaintainerSettings.Cleaner.findEmptyFoldersAutomatically = EditorGUILayout.ToggleLeft(new GUIContent("Autoclean", "Perform empty folders clean automatically on every scripts reload."), MaintainerSettings.Cleaner.findEmptyFoldersAutomatically, GUILayout.Width(100));
					if (EditorGUI.EndChangeCheck())
					{
						if (MaintainerSettings.Cleaner.findEmptyFoldersAutomatically)
							EditorUtility.DisplayDialog(ProjectCleaner.MODULE_NAME, "In case you're having thousands of folders in your project this may hang Unity for few additional secs on every scripts reload.\n" + Maintainer.DATA_LOSS_WARNING, "Understood");
					}
					GUI.enabled = true;
				}

				GUILayout.Space(5);

				if (UIHelpers.ImageButton("Reset", "Resets settings to defaults.", CSIcons.Restore))
				{
					MaintainerSettings.Cleaner.Reset();
				}
			}
		}

		protected override void DrawLeftExtra()
		{
			using (layout.Vertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
			{
				GUILayout.Space(10);
				GUILayout.Label("<size=14><b>Statistics</b></size>", UIHelpers.centeredLabel);
				GUILayout.Space(10);

				DrawStatsBody();
			}
		}

		private void DrawStatsBody()
		{
			using (layout.Vertical(UIHelpers.panelWithBackground))
			{
				if (resultsStats == null)
				{
					GUILayout.Label("N/A");
				}
				else
				{
					GUILayout.Space(5);
					GUILayout.Label("Physical size");
					UIHelpers.Separator();
					GUILayout.Label("Total found: " + CSEditorTools.FormatBytes(resultsStats.totalSize));
					GUILayout.Label("Selected: " + CSEditorTools.FormatBytes(resultsStats.selectedSize));
					GUILayout.Space(5);
				}
			}
		}

		protected override void DrawSearchTop()
		{
			if (UIHelpers.ImageButton("1. Scan project", CSIcons.Find))
			{
				EditorApplication.delayCall += StartSearch;
			}

			if (UIHelpers.ImageButton("2. Clean selected garbage", CSIcons.Delete))
			{
				EditorApplication.delayCall += StartClean;
			}
		}

		protected override void DrawPagesRightHeader()
		{
			base.DrawPagesRightHeader();

			GUILayout.Label("Sorting:", GUILayout.ExpandWidth(false));

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Cleaner.sortingType = (CleanerSortingType)EditorGUILayout.EnumPopup(MaintainerSettings.Cleaner.sortingType, GUILayout.Width(100));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Cleaner.sortingDirection = (SortingDirection)EditorGUILayout.EnumPopup(MaintainerSettings.Cleaner.sortingDirection, GUILayout.Width(80));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}
		}

		protected override void DrawRecord(int recordIndex)
		{
			CleanerRecord record = filteredRecords[recordIndex];

			// hide cleaned records 
			if (record.cleaned) return;

			using (layout.Vertical())
			{
				if (recordIndex > 0 && recordIndex < filteredRecords.Length) UIHelpers.Separator();

				using (layout.Horizontal())
				{
					DrawRecordCheckbox(record);
					DrawExpandCollapseButton(record);
					DrawIcon(record);

					if (record.compactMode)
					{
						DrawRecordButtons(record, recordIndex);
						GUILayout.Label(record.GetCompactLine(), UIHelpers.richLabel);
					}
					else
					{
						GUILayout.Space(5);
						GUILayout.Label(record.GetHeader(), UIHelpers.richLabel);
					}
				}

				if (!record.compactMode)
				{
					UIHelpers.Separator();
					using (layout.Horizontal())
					{
						GUILayout.Space(5);
						GUILayout.Label(record.GetBody(), UIHelpers.richLabel);
					}
					using (layout.Horizontal())
					{
						GUILayout.Space(5);
						DrawRecordButtons(record, recordIndex);
					}
					GUILayout.Space(3);
				}
			}

			if (Event.current != null && Event.current.type == EventType.MouseDown)
			{
				Rect guiRect = GUILayoutUtility.GetLastRect();
				guiRect.height += 2; // to compensate the separator's gap

				if (guiRect.Contains(Event.current.mousePosition))
				{
					record.compactMode = !record.compactMode;
					Event.current.Use();
				}
			}
		}

		protected override void ApplySorting()
		{
			base.ApplySorting();

			switch (MaintainerSettings.Cleaner.sortingType)
			{
				case CleanerSortingType.Unsorted:
					break;
				case CleanerSortingType.ByPath:
					filteredRecords = MaintainerSettings.Cleaner.sortingDirection == SortingDirection.Ascending ? 
						filteredRecords.OrderBy(RecordsSortings.cleanerRecordByPath).ToArray() : 
						filteredRecords.OrderByDescending(RecordsSortings.cleanerRecordByPath).ToArray();
					break;
				case CleanerSortingType.ByType:
					filteredRecords = MaintainerSettings.Cleaner.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderBy(RecordsSortings.cleanerRecordByType).ThenBy(RecordsSortings.cleanerRecordByAssetType).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray() :
						filteredRecords.OrderByDescending(RecordsSortings.cleanerRecordByType).ThenBy(RecordsSortings.cleanerRecordByAssetType).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray();
					break;
				case CleanerSortingType.BySize:
					filteredRecords = MaintainerSettings.Cleaner.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderByDescending(RecordsSortings.cleanerRecordBySize).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray() :
						filteredRecords.OrderBy(RecordsSortings.cleanerRecordBySize).ThenBy(RecordsSortings.cleanerRecordByPath).ToArray();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected override void SaveSearchResults()
		{
			SearchResultsStorage.CleanerSearchResults = GetRecords();
			resultsStats.Update(filteredRecords);
		}

		protected override string GetModuleName()
		{
			return ProjectCleaner.MODULE_NAME;
		}

		protected override string GetReportHeader()
		{
			return resultsStats != null ? "Total found garbage size: " + CSEditorTools.FormatBytes(resultsStats.totalSize) : null;
		}

		protected override string GetReportFileNamePart()
		{
			return "Cleaner";
		}

		protected override void AfterClearRecords()
		{
			resultsStats = null;
			SearchResultsStorage.CleanerSearchResults = null;
		}

		protected override void OnSelectionChanged()
		{
			resultsStats.Update(filteredRecords);
		}

		private void StartSearch()
		{
			window.RemoveNotification();
			ProjectCleaner.StartSearch(true);
			window.Focus();
		}

		private void StartClean()
		{
			window.RemoveNotification();
			ProjectCleaner.StartClean();
			window.Focus();
		}

		private void DrawRecordButtons(CleanerRecord record, int recordIndex)
		{
			DrawShowButtonIfPossible(record);

			AssetRecord assetRecord = record as AssetRecord;
			if (assetRecord != null)
			{
				DrawDeleteButton(assetRecord, recordIndex);

				if (record.compactMode)
				{
					DrawMoreButton(assetRecord);
				}
				else
				{
					DrawRevealButton(assetRecord);
					DrawCopyButton(assetRecord);
					DrawMoreButton(assetRecord);
				}
			}
		}

		private void DrawIcon(CleanerRecord record)
		{
			Texture icon = null;

			AssetRecord ar = record as AssetRecord;
			if (ar != null)
			{
				if (ar.isTexture)
				{
					icon = AssetPreview.GetMiniTypeThumbnail(ar.assetType);
				}

				if (icon == null)
				{
					icon = AssetDatabase.GetCachedIcon(ar.assetDatabasePath);
				}
			}

			CleanerErrorRecord er = record as CleanerErrorRecord;
			if (er != null)
			{
				icon = CSEditorTextures.ErrorSmallIcon;
			}

			if (icon != null)
			{
				Rect position = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
				GUI.DrawTexture(position, icon);
			}
		}

		private void DrawDeleteButton(CleanerRecord record, int recordIndex)
		{
			if (UIHelpers.RecordButton(record, "Delete", "Deletes this single item.", CSIcons.Delete))
			{
				if (record.Clean())
				{
					recordToDeleteIndex = recordIndex;
					EditorApplication.delayCall += DeleteRecord;
				}
			}
		}

		private void DrawRevealButton(AssetRecord record)
		{
			if (UIHelpers.RecordButton(record, "Reveal", "Reveals item in system default File Manager like Explorer on Windows or Finder on Mac.", CSIcons.Reveal))
			{
				EditorUtility.RevealInFinder(record.path);
			}
		}

		private void DrawMoreButton(AssetRecord assetRecord)
		{
			if (UIHelpers.RecordButton(assetRecord, "Shows menu with additional actions for this record.", CSIcons.More))
			{
				GenericMenu menu = new GenericMenu();
				if (!string.IsNullOrEmpty(assetRecord.path))
				{
					menu.AddItem(new GUIContent("Ignore/Add path to ignores"), false, () =>
					{
						if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Cleaner.pathIgnores, assetRecord.assetDatabasePath))
						{
							MaintainerWindow.ShowNotification("Ignore added: " + assetRecord.assetDatabasePath);
							CleanerFiltersWindow.Refresh();
						}
						else
						{
							MaintainerWindow.ShowNotification("Such item already added to the ignores!");
						}
					});

					DirectoryInfo dir = Directory.GetParent(assetRecord.assetDatabasePath);
					if (dir.Name != "Assets")
					{
						menu.AddItem(new GUIContent("Ignore/Add parent directory to ignores"), false, () =>
						{
							if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Cleaner.pathIgnores, dir.ToString()))
							{
								MaintainerWindow.ShowNotification("Ignore added: " + dir);
								CleanerFiltersWindow.Refresh();
							}
							else
							{
								MaintainerWindow.ShowNotification("Such item already added to the ignores!");
							}
						});
					}
				}
				menu.ShowAsContext();
			}
		}

		private class CleanerResultsStats
		{
			public long totalSize;
			public long selectedSize;

			public void Update(CleanerRecord[] records)
			{
				selectedSize = totalSize = 0;

				for (int i = 0; i < records.Length; i++)
				{
					AssetRecord assetRecord = records[i] as AssetRecord;
					if (assetRecord == null || assetRecord.cleaned) continue;

					totalSize += assetRecord.size;
					if (assetRecord.selected) selectedSize += assetRecord.size;
				}
			}
		}
	}
}

#endif