#if UNITY_EDITOR

#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#endif

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI.Filters;
using UnityEditor;
using UnityEngine;
using RecordType = CodeStage.Maintainer.Issues.RecordType;

namespace CodeStage.Maintainer.UI
{
	internal class IssuesTab : RecordsTab<IssueRecord>
	{
		private GUIContent caption;
		internal GUIContent Caption
		{
			get
			{
				if (caption == null)
				{
					caption = new GUIContent(IssuesFinder.MODULE_NAME, CSIcons.Issue);
				}
				return caption;
			}
		}

		protected override IssueRecord[] LoadLastRecords()
		{
			IssueRecord[] loadedRecords = SearchResultsStorage.IssuesSearchResults;

			if (loadedRecords == null)
			{
				loadedRecords = new IssueRecord[0];
			}
			return loadedRecords;
		}

		protected override void ApplySorting()
		{
			base.ApplySorting();

			switch (MaintainerSettings.Issues.sortingType)
			{
				case IssuesSortingType.Unsorted:
					break;
				case IssuesSortingType.ByIssueType:
					filteredRecords = MaintainerSettings.Issues.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderBy(RecordsSortings.issueRecordByType).ThenBy(RecordsSortings.issueRecordByPath).ToArray() :
						filteredRecords.OrderByDescending(RecordsSortings.issueRecordByType).ThenBy(RecordsSortings.issueRecordByPath).ToArray();
					break;
				case IssuesSortingType.BySeverity:
					filteredRecords = MaintainerSettings.Issues.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderByDescending(RecordsSortings.issueRecordBySeverity).ThenBy(RecordsSortings.issueRecordByPath).ToArray() :
						filteredRecords.OrderBy(RecordsSortings.issueRecordBySeverity).ThenBy(RecordsSortings.issueRecordByPath).ToArray();
					break;
				case IssuesSortingType.ByPath:
					filteredRecords = MaintainerSettings.Issues.sortingDirection == SortingDirection.Ascending ?
						filteredRecords.OrderBy(RecordsSortings.issueRecordByPath).ToArray() :
						filteredRecords.OrderByDescending(RecordsSortings.issueRecordByPath).ToArray();
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		protected override void SaveSearchResults()
		{
			SearchResultsStorage.IssuesSearchResults = GetRecords();
		}

		protected override string GetModuleName()
		{
			return IssuesFinder.MODULE_NAME;
		}

		protected override void DrawSettingsBody()
		{
			// ----------------------------------------------------------------------------
			// filtering settings
			// ----------------------------------------------------------------------------

			using (layout.Vertical(UIHelpers.panelWithBackground))
			{
				GUILayout.Space(5);

				if (UIHelpers.ImageButton("Manage Filters...", CSIcons.Gear))
				{
					IssuesFiltersWindow.Create();
				}

				GUILayout.Space(5);

				/* Game Object Issues filtering */

				GUILayout.Label("<b><size=12>Game Object Issues filtering</size></b>", UIHelpers.richLabel);
				UIHelpers.Separator();
				GUILayout.Space(5);

				using (layout.Horizontal())
				{
					MaintainerSettings.Issues.lookInScenes = EditorGUILayout.ToggleLeft(new GUIContent("Scenes", "Uncheck to exclude all scenes from search or select filtering level:\n\n" +
					                                                                                             "All Scenes: all project scenes with respect to configured filters.\n" +
					                                                                                             "Included Scenes: scenes included via Manage Filters > Scene Includes.\n" +
					                                                                                             "Current Scene: currently opened scene including any additional loaded scenes."), MaintainerSettings.Issues.lookInScenes, GUILayout.Width(70));
					GUI.enabled = MaintainerSettings.Issues.lookInScenes;
					MaintainerSettings.Issues.scenesSelection = (IssuesFinderSettings.ScenesSelection)EditorGUILayout.EnumPopup(MaintainerSettings.Issues.scenesSelection);
					GUI.enabled = true;
				}

				MaintainerSettings.Issues.lookInAssets = EditorGUILayout.ToggleLeft(new GUIContent("Prefab assets", "Uncheck to exclude all prefab assets files from the search. Check readme for additional details."), MaintainerSettings.Issues.lookInAssets);
				MaintainerSettings.Issues.touchInactiveGameObjects = EditorGUILayout.ToggleLeft(new GUIContent("Inactive GameObjects", "Uncheck to exclude all inactive Game Objects from the search."), MaintainerSettings.Issues.touchInactiveGameObjects);
				MaintainerSettings.Issues.touchDisabledComponents = EditorGUILayout.ToggleLeft(new GUIContent("Disabled Components", "Uncheck to exclude all disabled Components from the search."), MaintainerSettings.Issues.touchDisabledComponents);

				GUILayout.Space(2);
			}

			using (layout.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true)))
			{
				GUILayout.Space(5);
				GUILayout.Label("<b><size=12>Search for:</size></b>", UIHelpers.richLabel);

				settingsSectionScrollPosition = GUILayout.BeginScrollView(settingsSectionScrollPosition, GUIStyle.none, GUI.skin.verticalScrollbar);

				// ----------------------------------------------------------------------------
				// Game Object Issues
				// ----------------------------------------------------------------------------

				GUI.enabled = UIHelpers.ToggleFoldout(ref MaintainerSettings.Issues.scanGameObjects, ref MaintainerSettings.Issues.gameObjectsFoldout, new GUIContent("<b>Game Object Issues</b>", "Group of issues related to the Game Objects."));
				if (MaintainerSettings.Issues.gameObjectsFoldout)
				{
					GUILayout.Space(-2);
					UIHelpers.Indent();

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.Common, ref MaintainerSettings.Issues.commonFoldout))
					{
						MaintainerSettings.Issues.missingComponents = EditorGUILayout.ToggleLeft(new GUIContent("Missing components", "Search for the missing components on the Game Objects."), MaintainerSettings.Issues.missingComponents);
						MaintainerSettings.Issues.duplicateComponents = EditorGUILayout.ToggleLeft(new GUIContent("Duplicate components", "Search for the multiple instances of the same component with same values on the same object."), MaintainerSettings.Issues.duplicateComponents);
						
						bool show = MaintainerSettings.Issues.duplicateComponents;
						if (show)
						{
							EditorGUI.indentLevel++;
							MaintainerSettings.Issues.duplicateComponentsPrecise = EditorGUILayout.ToggleLeft(new GUIContent("Precise mode", "Uncheck to ignore component's values."), MaintainerSettings.Issues.duplicateComponentsPrecise);
							EditorGUI.indentLevel--;
						}
						
						MaintainerSettings.Issues.missingReferences = EditorGUILayout.ToggleLeft(new GUIContent("Missing references", "Search for any missing references in the serialized fields of the components."), MaintainerSettings.Issues.missingReferences);
						MaintainerSettings.Issues.undefinedTags = EditorGUILayout.ToggleLeft(new GUIContent("Objects with undefined tags", "Search for GameObjects without any tag."), MaintainerSettings.Issues.undefinedTags);
						MaintainerSettings.Issues.inconsistentTerrainData = EditorGUILayout.ToggleLeft(new GUIContent("Inconsistent Terrain Data", "Search for Game Objects where Terrain and TerrainCollider have different Terrain Data."), MaintainerSettings.Issues.inconsistentTerrainData);
					}

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.PrefabsSpecific, ref MaintainerSettings.Issues.prefabsFoldout))
					{
						MaintainerSettings.Issues.missingPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Missing prefabs", "Search for instances of prefabs which were removed from project."), MaintainerSettings.Issues.missingPrefabs);
						MaintainerSettings.Issues.disconnectedPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Disconnected prefabs", "Search for disconnected prefabs instances."), MaintainerSettings.Issues.disconnectedPrefabs);
					}

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.UnusedComponents, ref MaintainerSettings.Issues.unusedFoldout))
					{
						MaintainerSettings.Issues.emptyMeshColliders = EditorGUILayout.ToggleLeft("MeshColliders w/o meshes", MaintainerSettings.Issues.emptyMeshColliders);
						MaintainerSettings.Issues.emptyMeshFilters = EditorGUILayout.ToggleLeft("MeshFilters w/o meshes", MaintainerSettings.Issues.emptyMeshFilters);
						MaintainerSettings.Issues.emptyAnimations = EditorGUILayout.ToggleLeft("Animations w/o clips", MaintainerSettings.Issues.emptyAnimations);
						MaintainerSettings.Issues.emptyRenderers = EditorGUILayout.ToggleLeft("Renders w/o materials", MaintainerSettings.Issues.emptyRenderers);
						MaintainerSettings.Issues.emptySpriteRenderers = EditorGUILayout.ToggleLeft("SpriteRenders w/o sprites", MaintainerSettings.Issues.emptySpriteRenderers);
						MaintainerSettings.Issues.emptyTerrainCollider = EditorGUILayout.ToggleLeft("TerrainColliders w/o Terrain Data", MaintainerSettings.Issues.emptyTerrainCollider);
						MaintainerSettings.Issues.emptyAudioSource = EditorGUILayout.ToggleLeft("AudioSources w/o AudioClips", MaintainerSettings.Issues.emptyAudioSource);
					}

					if (DrawSettingsSearchSectionHeader(SettingsSearchSection.Neatness, ref MaintainerSettings.Issues.neatnessFoldout))
					{
						MaintainerSettings.Issues.emptyArrayItems = EditorGUILayout.ToggleLeft(new GUIContent("Empty array items", "Look for any unused items in arrays."), MaintainerSettings.Issues.emptyArrayItems);
						bool show = MaintainerSettings.Issues.emptyArrayItems;
						if (show)
						{
							EditorGUI.indentLevel++;
							MaintainerSettings.Issues.skipEmptyArrayItemsOnPrefabs = EditorGUILayout.ToggleLeft(new GUIContent("Skip prefab files", "Ignore empty array items in prefab files."), MaintainerSettings.Issues.skipEmptyArrayItemsOnPrefabs);
							EditorGUI.indentLevel--;
						}
						MaintainerSettings.Issues.unnamedLayers = EditorGUILayout.ToggleLeft(new GUIContent("Objects with unnamed layers", "Search for GameObjects with unnamed layers."), MaintainerSettings.Issues.unnamedLayers);
						MaintainerSettings.Issues.hugePositions = EditorGUILayout.ToggleLeft(new GUIContent("Objects with huge positions", "Search for GameObjects with huge world positions (> |100 000| on any axis)."), MaintainerSettings.Issues.hugePositions);
					}

					UIHelpers.UnIndent();
				}
				GUI.enabled = true;

				// ----------------------------------------------------------------------------
				// Project Settings Issues
				// ----------------------------------------------------------------------------

				GUI.enabled = UIHelpers.ToggleFoldout(ref MaintainerSettings.Issues.scanProjectSettings, ref MaintainerSettings.Issues.projectSettingsFoldout, new GUIContent("<b>Project Settings Issues</b>", "Group of issues related to the settings of the current project."));
				if (MaintainerSettings.Issues.projectSettingsFoldout)
				{
					UIHelpers.Indent();

					MaintainerSettings.Issues.duplicateScenesInBuild = EditorGUILayout.ToggleLeft(new GUIContent("Duplicate scenes in build", "Search for the duplicates at the 'Scenes In Build' section of the Build Settings."), MaintainerSettings.Issues.duplicateScenesInBuild);
					MaintainerSettings.Issues.duplicateTagsAndLayers = EditorGUILayout.ToggleLeft(new GUIContent("Duplicates in Tags and Layers", "Search for the duplicate items at the 'Tags and Layers' Project Settings."), MaintainerSettings.Issues.duplicateTagsAndLayers);

					UIHelpers.UnIndent();
				}
				GUI.enabled = true;

				GUILayout.EndScrollView();
				UIHelpers.Separator();

				using (layout.Horizontal())
				{
					if (UIHelpers.ImageButton("Check all", CSIcons.SelectAll))
					{
						MaintainerSettings.Issues.SwitchAll(true);
					}

					if (UIHelpers.ImageButton("Uncheck all", CSIcons.SelectNone))
					{
						MaintainerSettings.Issues.SwitchAll(false);
					}
				}
			}

			if (UIHelpers.ImageButton("Reset", "Resets settings to defaults.", CSIcons.Restore))
			{
				MaintainerSettings.Issues.Reset();
			}
		}

		protected override void DrawSearchTop()
		{
			if (UIHelpers.ImageButton("1. Find issues!", CSIcons.Find))
			{
				EditorApplication.delayCall += StartSearch;
			}

			if (UIHelpers.ImageButton("2. Automatically fix selected issues if possible", CSIcons.AutoFix))
			{
				EditorApplication.delayCall += StartFix;
			}
		}

		protected override void DrawPagesRightHeader()
		{
			base.DrawPagesRightHeader();

			GUILayout.Label("Sorting:", GUILayout.ExpandWidth(false));

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Issues.sortingType = (IssuesSortingType)EditorGUILayout.EnumPopup(MaintainerSettings.Issues.sortingType, GUILayout.Width(100));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}

			EditorGUI.BeginChangeCheck();
			MaintainerSettings.Issues.sortingDirection = (SortingDirection)EditorGUILayout.EnumPopup(MaintainerSettings.Issues.sortingDirection, GUILayout.Width(80));
			if (EditorGUI.EndChangeCheck())
			{
				ApplySorting();
			}
		}

		protected override void DrawRecord(int recordIndex)
		{
			IssueRecord record = filteredRecords[recordIndex];

			// hide fixed records 
			if (record.@fixed) return;

			using (layout.Vertical())
			{
				if (recordIndex > 0 && recordIndex < filteredRecords.Length) UIHelpers.Separator();

				using (layout.Horizontal())
				{
					DrawRecordCheckbox(record);
					DrawExpandCollapseButton(record);
					DrawSeverityIcon(record);
					
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

					if (record.location == RecordLocation.Prefab)
					{
						GUILayout.Space(3);
						UIHelpers.Icon(CSEditorTextures.PrefabIcon, "Issue found in the Prefab.");
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

		protected override string GetReportFileNamePart()
		{
			return "Issues";
		}

		protected override void AfterClearRecords()
		{
			SearchResultsStorage.IssuesSearchResults = null;
		}

		private void StartSearch()
		{
			window.RemoveNotification();
			IssuesFinder.StartSearch(true);
			window.Focus();
		}

		private void StartFix()
		{
			window.RemoveNotification();
			IssuesFinder.StartFix();
			window.Focus();
		}

		private void DrawRecordButtons(IssueRecord record, int recordIndex)
		{
			DrawShowButtonIfPossible(record);
			DrawFixButton(record, recordIndex);

			if (!record.compactMode)
			{
				DrawCopyButton(record);
				DrawHideButton(record, recordIndex);
			}

			GameObjectIssueRecord objectIssue = record as GameObjectIssueRecord;
			if (objectIssue != null)
			{
				DrawMoreButton(objectIssue);
			}
		}

		private void DrawFixButton(IssueRecord record, int recordIndex)
		{
			GUI.enabled = record.CanBeFixed();

			string label = "Fix";
			string hint = "Automatically fixes issue (not available for this issue yet).";

			if (record.type == RecordType.MissingComponent)
			{
				label = "Remove";
				hint = "Removes missing component.";
			}
			else if (record.type == RecordType.MissingReference)
			{
				label = "Reset";
				hint = "Resets missing reference to default None value.";
			}

			if (UIHelpers.RecordButton(record, label, hint, CSIcons.AutoFix))
			{
				if (record.Fix(false))
				{
					HideRecord(recordIndex);

					string notificationExtra = "";

					if (record.location == RecordLocation.Prefab || record.location == RecordLocation.Asset)
					{
						AssetDatabase.SaveAssets();
					}
					else if (record.location == RecordLocation.Scene)
					{
						notificationExtra = "\nDon't forget to save the scene!";
					}

					MaintainerWindow.ShowNotification("Issue successfully fixed!" + notificationExtra);
				}
			}

			GUI.enabled = true;
		}

		private void DrawHideButton(IssueRecord record, int recordIndex)
		{
			if (UIHelpers.RecordButton(record, "Hide", "Hides this issue from the results list.\nUseful when you fixed issue and wish to hide it away.", CSIcons.Hide))
			{
				HideRecord(recordIndex);
			}
		}

		private void HideRecord(int index)
		{
			recordToDeleteIndex = index;
			EditorApplication.delayCall += DeleteRecord;
		}

		private void DrawMoreButton(GameObjectIssueRecord record)
		{
			if (!UIHelpers.RecordButton(record, "Shows menu with additional actions for this record.", CSIcons.More)) return;

			GenericMenu menu = new GenericMenu();
			if (!string.IsNullOrEmpty(record.path))
			{
				menu.AddItem(new GUIContent("Ignore/Add path to ignores"), false, () =>
				{
					if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Issues.pathIgnores, record.path))
					{
						MaintainerWindow.ShowNotification("Ignore added: " + record.path);
						IssuesFiltersWindow.Refresh();
					}
					else
					{
						MaintainerWindow.ShowNotification("Such item already added to the ignores!");
					}
				});

				DirectoryInfo dir = Directory.GetParent(record.path);
				if (dir.Name != "Assets")
				{
					menu.AddItem(new GUIContent("Ignore/Add parent directory to ignores"), false, () =>
					{
						if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Issues.pathIgnores, dir.ToString()))
						{
							MaintainerWindow.ShowNotification("Ignore added: " + dir);
							IssuesFiltersWindow.Refresh();
						}
						else
						{
							MaintainerWindow.ShowNotification("Such item already added to the ignores!");
						}
					});
				}
			}

			if (!string.IsNullOrEmpty(record.componentName))
			{
				menu.AddItem(new GUIContent("Ignore/Add component to ignores"), false, () =>
				{
					if (CSArrayTools.AddIfNotExists(ref MaintainerSettings.Issues.componentIgnores, record.componentName))
					{
						MaintainerWindow.ShowNotification("Ignore added: " + record.componentName);
						IssuesFiltersWindow.Refresh();
					}
					else
					{
						MaintainerWindow.ShowNotification("Such item already added to the ignores!");
					}
				});
			}
			menu.ShowAsContext();
		}

		private bool DrawSettingsSearchSectionHeader(SettingsSearchSection section, ref bool foldout)
		{
			GUILayout.Space(5);
			using (layout.Horizontal())
			{
				foldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(true, GUILayout.Width(145)), foldout, ObjectNames.NicifyVariableName(section.ToString()), true, UIHelpers.richFoldout);

				if (UIHelpers.IconButton(CSIcons.SelectAll))
				{
					typeof(IssuesFinderSettings).InvokeMember("Switch" + section, BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, MaintainerSettings.Issues, new[] {(object)true});
				}

				if (UIHelpers.IconButton(CSIcons.SelectNone))
				{
					typeof(IssuesFinderSettings).InvokeMember("Switch" + section, BindingFlags.InvokeMethod | BindingFlags.NonPublic, null, MaintainerSettings.Issues, new[] {(object)false});
				}
			}
			UIHelpers.Separator();

			return foldout;
		}

		private void DrawSeverityIcon(IssueRecord record)
		{
			Texture icon;

			if (record == null) return;

			switch (record.severity)
			{
				case RecordSeverity.Error:
					icon = CSEditorTextures.ErrorSmallIcon;
					break;
				case RecordSeverity.Warning:
					icon = CSEditorTextures.WarnSmallIcon;
					break;
				case RecordSeverity.Info:
					icon = CSEditorTextures.InfoSmallIcon;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			Rect iconArea = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
			Rect iconRect = new Rect(iconArea);

			GUI.DrawTexture(iconRect, icon, ScaleMode.ScaleAndCrop);
		}

		
	}

	internal enum SettingsSearchSection : byte
	{
		Common,
		PrefabsSpecific,
		UnusedComponents,
		Neatness,
	}
}

#endif