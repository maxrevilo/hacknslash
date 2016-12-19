﻿#if UNITY_EDITOR

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI.Filters
{
	internal class SceneFiltersTab : StringFiltersTab
	{
		public delegate void SaveSceneIgnoresCallback(bool ignoreScenesInBuild, bool ignoreOnlyEnabledScenesInBuild);

		private readonly string headerExtra;
		private bool ignoreScenesInBuild;
		private bool ignoreOnlyEnabledScenesInBuild;
		private readonly SaveSceneIgnoresCallback saveSceneIgnoresCallback;

		public SceneFiltersTab(FilterType filterType, string headerExtra, string[] filtersList, bool ignoreScenesInBuild, bool ignoreOnlyEnabledScenesInBuild, SaveSceneIgnoresCallback saveSceneIgnoresCallback, SaveFiltersCallback saveFiltersCallback) : base(filterType, filtersList, saveFiltersCallback)
		{
			caption = new GUIContent("Scene <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + ">" + filterType + "</color>", CSEditorTextures.SceneIcon);
			
			this.headerExtra = headerExtra;
			this.ignoreScenesInBuild = ignoreScenesInBuild;
			this.ignoreOnlyEnabledScenesInBuild = ignoreOnlyEnabledScenesInBuild;
			this.saveSceneIgnoresCallback = saveSceneIgnoresCallback;
		}

		internal override void ProcessDrags()
		{
			if (currentEventType != EventType.DragUpdated && currentEventType != EventType.DragPerform) return;

			string[] paths = DragAndDrop.paths;

			if (paths != null && paths.Length > 0)
			{

				bool canDrop = false;

				for (int i = 0; i < paths.Length; i++)
				{
					if (LooksLikeSceneFile(paths[i]))
					{
						canDrop = true;
						break;
					}
				}

				if (canDrop)
				{
					DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

					if (currentEventType == EventType.DragPerform)
					{
						bool needToSave = false;
						bool needToShowWarning = false;

						foreach (string path in paths)
						{
							if (LooksLikeSceneFile(path))
							{
								bool added = TryAddNewItemToFilters(path);
								needToSave |= added;
								needToShowWarning |= !added;
							}
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
			}
			Event.current.Use();
		}

		protected override void DrawTabHeader()
		{
			EditorGUILayout.LabelField("Here you may specify which scenes to <color=" +
										(filterType == FilterType.Includes ? "#02C85F" : "#FF4040FF") + "><b>" + 
										(filterType == FilterType.Ignores ? "ignore" : "include") + "</b></color>.\n" +
			                           "You may drag & drop scene files to this window directly from the Project Browser.\n"+
									   "Print <b>t:Scene</b> in the Project Browser search bar to find all scenes in the project.",
										UIHelpers.richWordWrapLabel);

			if (!string.IsNullOrEmpty(headerExtra))
			{
				EditorGUILayout.LabelField(headerExtra, EditorStyles.wordWrappedLabel);
			}

			GUILayout.Space(5);
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(5);
			ignoreScenesInBuild = EditorGUILayout.ToggleLeft(new GUIContent("Scenes in build", "Take into account scenes added to the 'Scenes In Build' list at the Build Settings."), ignoreScenesInBuild, GUILayout.Width(110));
			GUI.enabled = ignoreScenesInBuild;
			ignoreOnlyEnabledScenesInBuild = EditorGUILayout.ToggleLeft(new GUIContent("Only enabled", "Take into account only enabled 'Scenes In Build'."), ignoreOnlyEnabledScenesInBuild, GUILayout.Width(110));
			
			if (GUILayout.Button(new GUIContent("Manage build scenes...", "Opens standard Build Settings window.")))
			{
				EditorWindow.GetWindow(Type.GetType("UnityEditor.BuildPlayerWindow,UnityEditor"), true);
			}
			GUI.enabled = true;
			GUILayout.Space(5);
			EditorGUILayout.EndHorizontal();
			if (EditorGUI.EndChangeCheck())
			{
				saveSceneIgnoresCallback(ignoreScenesInBuild, ignoreOnlyEnabledScenesInBuild);
			}
			GUILayout.Space(5);
			
		}

		protected override bool CheckNewItem(ref string newItem)
		{
			if (LooksLikeSceneFile(newItem))
			{
				return true;
			}

			EditorUtility.DisplayDialog("Can't find specified scene", "Scene " + newItem + " wasn't found in project. Make sure you've entered relative path starting from Assets/.", "Cool, thanks!");
			return false;
		}

		protected override string GetAddNewLabel()
		{
			return "Also you may add specific scenes to the list:";
		}

		private bool LooksLikeSceneFile(string path)
		{
			return File.Exists(path) && Path.GetExtension(path) == ".unity";
		}
	}
}

#endif