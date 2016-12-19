#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.Tools
{
	public class CSEditorTools
	{
		private static readonly string[] sizes = { "B", "KB", "MB", "GB" };

		public static string FormatBytes(double bytes)
		{
			int order = 0;

			// 4 - sizes.Length
			while (bytes >= 1024 && order + 1 < 4)
			{
				order++;
				bytes = bytes / 1024;
			}

			// Adjust the format string to your preferences. For example "{0:0.#}{1}" would
			// show a single decimal place, and no space.
			return string.Format("{0:0.##} {1}", bytes, sizes[order]);
		}

		public static int GetPropertyHash(SerializedProperty sp)
		{
			/*Debug.Log("Property: " + sp.name);
			Debug.Log("sp.propertyType = " + sp.propertyType);*/
			StringBuilder stringHash = new StringBuilder();

			stringHash.Append(sp.type);

			if (sp.isArray)
			{
				stringHash.Append(sp.arraySize);
			}
			else
				switch (sp.propertyType)
				{
					case SerializedPropertyType.AnimationCurve:
						if (sp.animationCurveValue != null)
						{
							stringHash.Append(sp.animationCurveValue.length);
							if (sp.animationCurveValue.keys != null)
							{
								foreach (Keyframe key in sp.animationCurveValue.keys)
								{
									stringHash.Append(key.value)
											  .Append(key.time)
											  .Append(key.tangentMode)
											  .Append(key.outTangent)
											  .Append(key.inTangent);
								}
							}
						}
						
						break;
					case SerializedPropertyType.ArraySize:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Boolean:
						stringHash.Append(sp.boolValue);
						break;
					case SerializedPropertyType.Bounds:
						stringHash.Append(sp.boundsValue.center)
								  .Append(sp.boundsValue.extents);
						break;
					case SerializedPropertyType.Character:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Generic: // looks like arrays which we already walk through
						break;
					case SerializedPropertyType.Gradient: // unsupported
						break;
					case SerializedPropertyType.ObjectReference:
						if (sp.objectReferenceValue != null)
						{
							stringHash.Append(sp.objectReferenceValue.name);
						}
						break;
					case SerializedPropertyType.Color:
						stringHash.Append(sp.colorValue);
						break;
					case SerializedPropertyType.Enum:
						stringHash.Append(sp.enumValueIndex);
						break;
					case SerializedPropertyType.Float:
						stringHash.Append(sp.floatValue);
						break;
					case SerializedPropertyType.Integer:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.LayerMask:
						stringHash.Append(sp.intValue);
						break;
					case SerializedPropertyType.Quaternion:
						stringHash.Append(sp.quaternionValue);
						break;
					case SerializedPropertyType.Rect:
						stringHash.Append(sp.rectValue);
						break;
					case SerializedPropertyType.String:
						stringHash.Append(sp.stringValue);
						break;
					case SerializedPropertyType.Vector2:
						stringHash.Append(sp.vector2Value);
						break;
					case SerializedPropertyType.Vector3:
						stringHash.Append(sp.vector3Value);
						break;
					case SerializedPropertyType.Vector4:
						stringHash.Append(sp.vector4Value);
						break;
					default:
						Debug.LogWarning(Maintainer.LOG_PREFIX + "Unknown SerializedPropertyType: " + sp.propertyType);
						break;
				}

			return stringHash.ToString().GetHashCode();
		}

		public static string GetFullTransformPath(Transform transform)
		{
			string path = transform.name;
			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}
			return path;
		}

		public static GameObject[] GetAllSuitableGameObjectsInCurrentScene()
		{
			GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
			List<GameObject> result = new List<GameObject>(allObjects);
			result.RemoveAll(o => !string.IsNullOrEmpty(AssetDatabase.GetAssetPath(o)) || o.hideFlags != HideFlags.None);
			return result.ToArray();
		}

		public static int GetAllSuitableGameObjectsInPrefabAssets(List<GameObject> gameObjects)
		{
            return GetAllSuitableGameObjectsInPrefabAssets(gameObjects, null);
		}

		public static int GetAllSuitableGameObjectsInPrefabAssets(List<GameObject> gameObjects, List<string> paths)
		{
			string[] allAssetPaths = FindAssetsFiltered("t:Prefab");
			return GetSuitablePrefabsFromSelection(allAssetPaths, gameObjects, paths);
		}

		public static int GetSuitablePrefabsFromSelection(string[] selection, List<GameObject> gameObjects, List<string> paths)
		{
			int selectedCount = 0;

			foreach (string path in selection)
			{
				GameObject go = (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));

				if (go == null) continue;

				selectedCount = GetPrefabsRecursive(gameObjects, paths, path, go, selectedCount);
			}

			return selectedCount;
		}

		private static int GetPrefabsRecursive(List<GameObject> gameObjects, List<string> paths, string path, GameObject go, int selectedCount)
		{
			if (go.hideFlags == HideFlags.None || go.hideFlags == HideFlags.HideInHierarchy)
			{
				gameObjects.Add(go);
				if (paths != null) paths.Add(path);
				selectedCount++;
			}

			int childCount = go.transform.childCount;

			for (int i = 0; i < childCount; i++)
			{
				GameObject nestedObject = go.transform.GetChild(i).gameObject;
				selectedCount = GetPrefabsRecursive(gameObjects, paths, path, nestedObject, selectedCount);
			}

			return selectedCount;
		}

		public static string[] FindAssetsFiltered(string filter)
		{
			return FindAssetsFiltered(filter, null, null);
		}

		public static string[] FindAssetsFiltered(string filter, string[] includes, string[] ignores)
		{
			string[] allAssetsGUIDs = AssetDatabase.FindAssets(filter);
			int count = allAssetsGUIDs.Length;

			List<string> paths = new List<string>(count);

			for (int i = 0; i < count; i++)
			{
				string path = AssetDatabase.GUIDToAssetPath(allAssetsGUIDs[i]);

				bool include = false;
				bool skip = false;

				if (includes != null && includes.Length > 0)
				{
					include = CSArrayTools.IsItemContainsAnyStringFromArray(path, includes);
				}

				if (ignores != null && ignores.Length > 0)
				{
					skip = CSArrayTools.IsItemContainsAnyStringFromArray(path, ignores);
				}

				if (skip) continue;

				if (includes != null && includes.Length > 0)
				{
					if (include && !paths.Contains(path)) paths.Add(path);
				}
				else
				{
					if (!paths.Contains(path)) paths.Add(path);
				}
			}

			return paths.ToArray();
		}

		public static string[] FindAssetsInFolders(string filter, string[] folders)
		{
			if (folders == null || folders.Length == 0) return new string[0];

			string[] allAssetsGUIDs = AssetDatabase.FindAssets(filter, folders);
			int count = allAssetsGUIDs.Length;

			string[] paths = new string[count];

			for (int i = 0; i < count; i++)
			{
				paths[i] = AssetDatabase.GUIDToAssetPath(allAssetsGUIDs[i]);
			}

			return paths;
		}

		public static string[] FindFilesFiltered(string filter, string[] ignores)
		{
			string[] files = Directory.GetFiles("Assets", filter, SearchOption.AllDirectories);
			int count = files.Length;

			List<string> paths = new List<string>(count);

			for (int i = 0; i < count; i++)
			{
				string path = files[i];
				bool skip = false;

				if (ignores != null)
				{
					skip = CSArrayTools.IsItemContainsAnyStringFromArray(path, ignores);
				}

				if (!skip) paths.Add(path);
			}

			return paths.ToArray();
		}

		public static string[] FindFoldersFiltered(string filter, string[] ignores = null)
		{
			string[] files = Directory.GetDirectories("Assets", filter, SearchOption.AllDirectories);
			int count = files.Length;

			List<string> paths = new List<string>(count);

			for (int i = 0; i < count; i++)
			{
				string path = files[i];
				bool skip = false;

				if (ignores != null)
				{
					skip = CSArrayTools.IsItemContainsAnyStringFromArray(path, ignores);
				}

				if (!skip) paths.Add(path);
			}

			return paths.ToArray();
		}

		public static int GetDepthInHierarchy(Transform transform, Transform upToTransform)
		{
			if (transform == upToTransform || transform.parent == null) return 0;
			return 1 + GetDepthInHierarchy(transform.parent, upToTransform);
		}

		public static string NicifyAssetPath(string path)
		{
			string nicePath = path.Remove(0, 7);

			int lastSlash = nicePath.LastIndexOf('/');
			int lastDot = nicePath.LastIndexOf('.');

			// making sure we'll not trim path like Test/My.Test/linux_file
			if (lastDot > lastSlash)
			{
				nicePath = nicePath.Remove(lastDot, nicePath.Length - lastDot);
			}

			return nicePath;
		}

		public static ActiveEditorTracker GetActiveEditorTrackerForSelectedObject()
		{
			Type t = typeof(EditorWindow).Assembly.GetType("UnityEditor.InspectorWindow");
			EditorWindow inspectorWindow = EditorWindow.GetWindow(t);

			return (ActiveEditorTracker)inspectorWindow.GetType().GetMethod("GetTracker").Invoke(inspectorWindow, null);
		}
	}
}

#endif