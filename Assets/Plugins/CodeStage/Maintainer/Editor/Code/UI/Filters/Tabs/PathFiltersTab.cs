#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI.Filters
{
	internal class PathFiltersTab : StringFiltersTab
	{
		private bool showNotice;

		internal PathFiltersTab(FilterType filterType, string[] filtersList, bool showNotice, SaveFiltersCallback saveCallback) : base(filterType, filtersList, saveCallback)
		{
			caption = new GUIContent("Path <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + ">" + filterType + "</color>", CSEditorTextures.FolderIcon);
			this.showNotice = showNotice;
		}

		internal override void ProcessDrags()
		{
			if (currentEventType != EventType.DragUpdated && currentEventType != EventType.DragPerform) return;

			string[] paths = DragAndDrop.paths;

			if (paths != null && paths.Length > 0)
			{
				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

				if (currentEventType == EventType.DragPerform)
				{
					bool needToSave = false;
					bool needToShowWarning = false;

					foreach (string path in paths)
					{
						bool added = TryAddNewItemToFilters(path);
						needToSave |= added;
						needToShowWarning |= !added;
					}

					if (needToSave)
					{
						SaveChanges();
					}

					if (needToShowWarning)
					{
						window.ShowNotification(new GUIContent("One or more of the dragged items already present in the list!"));
					}

					DragAndDrop.AcceptDrag();
				}
			}
			Event.current.Use();
		}

		protected override void DrawTabHeader()
		{
			EditorGUILayout.LabelField("Here you may specify full or partial paths to <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + "><b>" + 
										(filterType == FilterType.Includes ? "include" : "ignore") + "</b></color>. " +
										(filterType == FilterType.Includes ? "Includes" : "Ignores") + " are case-sensitive.\n" +
										"You may drag & drop files and folders to this window directly from the Project Browser.",
										UIHelpers.richWordWrapLabel);
			EditorGUILayout.LabelField("Examples:\n" +
									   "folder name/ - " +
										(filterType == FilterType.Includes ? "includes" : "excludes") + " all assets having such folder in the path\n" +
									   ".unity - " +
										(filterType == FilterType.Includes ? "includes" : "excludes") + " all scenes\n" +
									   "SomeFile.ext - " +
										(filterType == FilterType.Includes ? "includes" : "excludes") + " specified file from any folder", UIHelpers.richWordWrapLabel);
			if (showNotice)
			{
				EditorGUILayout.LabelField("<b>Note:</b> If you have both Includes and Ignores added, first Includes are applied, then Ignores are applied to the included paths.",
										UIHelpers.richWordWrapLabel);
			}
			
		}

		protected override bool CheckNewItem(ref string newItem)
		{
			newItem = newItem.Replace('\\', '/');
			return true;
		}
	}
}

#endif