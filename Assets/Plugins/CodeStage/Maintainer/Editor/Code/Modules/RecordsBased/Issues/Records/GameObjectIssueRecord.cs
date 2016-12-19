#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#endif

#if UNITY_5_3_PLUS
using UnityEditor.SceneManagement;
#endif

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Issues
{
	[Serializable]
	public class GameObjectIssueRecord : IssueRecord, IShowableRecord
	{
		public string path;
		public string gameObjectPath;
		public long objectId;
		public string componentName;
		public long componentId;
		public string property;
		public string propertyPath;

		public void Show()
		{
			GameObject go = null;
			if (OpenNeededSceneIfNecessary(true)) go = GetGameObjectWithThisIssue();
			if (go != null)
			{
				CSObjectTools.SelectGameObject(go, location);

				if (location == RecordLocation.Scene)
				{
					EditorApplication.delayCall += () =>
					{
						EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
					};
				}
				else
				{
					if (gameObjectPath.Split('/').Length > 2)
					{
						EditorApplication.delayCall += () =>
						{
							EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath(path, typeof(Object)));
						};
					}
				}

				ActiveEditorTracker tracker = CSEditorTools.GetActiveEditorTrackerForSelectedObject();
				tracker.RebuildIfNecessary();

				Editor[] editors = tracker.activeEditors;

				long[] ids = new long[editors.Length];
				bool targetFound = false;

				for (int i = 0; i < editors.Length; i++)
				{
					Editor editor = editors[i];
					long id = CSObjectTools.GetLocalIdentifierInFileForObject(editor.serializedObject.targetObject);
					ids[i] = id;

					if (id == componentId)
					{
						targetFound = true;

						/* known corner cases when editor can't be set to visible via tracker */

						if (editor.serializedObject.targetObject is ParticleSystemRenderer)
						{
							ParticleSystemRenderer renderer = (ParticleSystemRenderer)editor.serializedObject.targetObject;
							ParticleSystem ps = renderer.GetComponent<ParticleSystem>();
							componentId = CSObjectTools.GetLocalIdentifierInFileForObject(ps);
						}
					}
				}

				if (targetFound)
				{
					for (int i = 0; i < editors.Length; i++)
					{
						tracker.SetVisible(i, ids[i] != componentId ? 0 : 1);
					}
				}
			}
			else
			{
				MaintainerWindow.ShowNotification("Couldn't find object " + gameObjectPath);
			}
		}

		internal static GameObjectIssueRecord Create(RecordType type, RecordLocation location, string path, GameObject gameObject)
		{
			return new GameObjectIssueRecord(type, location, path, gameObject);
		}

		internal static GameObjectIssueRecord Create(RecordType type, RecordLocation location, string path, GameObject gameObject, Component component, Type componentType, string componentName)
		{
			return new GameObjectIssueRecord(type, location, path, gameObject, component, componentType, componentName);
		}

		internal static GameObjectIssueRecord Create(RecordType type, RecordLocation location, string path, GameObject gameObject, Component component, Type componentType, string componentName, string property)
		{
			return new GameObjectIssueRecord(type, location, path, gameObject, component, componentType, componentName, property);
		}

		protected GameObjectIssueRecord(RecordType type, RecordLocation location, string path, GameObject gameObject):base(type,location)
		{
			this.path = path;
			gameObjectPath = CSEditorTools.GetFullTransformPath(gameObject.transform);
			objectId = CSObjectTools.GetLocalIdentifierInFileForObject(gameObject);

#if UNITY_5_3_PLUS
			if (location == RecordLocation.Scene)
			{
				this.path = gameObject.scene.path;
			}
#endif
		}

		protected GameObjectIssueRecord(RecordType type, RecordLocation location, string path, GameObject gameObject, Component component, Type componentType, string componentName) : this(type, location, path, gameObject)
		{
			this.componentName = componentName;

			componentId = CSObjectTools.GetLocalIdentifierInFileForObject(component);
			if (componentId > 0 && gameObject.GetComponents(componentType).Length > 1)
			{
				this.componentName += " (ID: " + componentId + ")";
			}
		}

		protected GameObjectIssueRecord(RecordType type, RecordLocation location, string path, GameObject gameObject, Component component, Type componentType, string componentName, string property):this(type, location, path, gameObject, component, componentType, componentName)
		{
			if (!string.IsNullOrEmpty(property))
			{
				string nicePropertyName = ObjectNames.NicifyVariableName(property);
				TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
				this.property = textInfo.ToTitleCase(nicePropertyName);
			}
		}

		internal override bool CanBeFixed()
		{
			return type == RecordType.MissingComponent || type == RecordType.MissingReference;
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append(location == RecordLocation.Scene ? "<b>Scene:</b> " : "<b>Prefab:</b> ");

			string nicePath = path == "" ? "Untitled (current scene)" : CSEditorTools.NicifyAssetPath(path);

			text.Append(nicePath);

			if (!string.IsNullOrEmpty(gameObjectPath)) text.Append("\n<b>Game Object:</b> ").Append(gameObjectPath);
			if (!string.IsNullOrEmpty(componentName)) text.Append("\n<b>Component:</b> ").Append(componentName);
			if (!string.IsNullOrEmpty(property)) text.Append("\n<b>Property:</b> ").Append(property);
		}

		protected override bool PerformFix(bool batchMode)
		{
			GameObject go = null;
			Component component = null;

			if (OpenNeededSceneIfNecessary(!batchMode)) go = GetGameObjectWithThisIssue();

			if (go == null)
			{
				if (batchMode)
				{
					Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't find Game Object for issue:\n" + this);
				}
				else
				{
					MaintainerWindow.ShowNotification("Couldn't find Game Object " + gameObjectPath);
				}
			
				return false;
			}

			if (!string.IsNullOrEmpty(componentName))
			{
				component = GetComponentWithThisIssue(go);

				if (component == null)
				{
					if (batchMode)
					{
						Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't find component for issue:\n" + this);
					}
					else
					{
						MaintainerWindow.ShowNotification("Can't find component " + componentName);
					}
					return false;
				}
			}

			return IssuesFixer.FixGameObjectIssue(this, go, component, type);
		}

		private bool OpenNeededSceneIfNecessary(bool askForSave)
		{
			if (location == RecordLocation.Scene)
			{
				if (CSSceneTools.GetCurrentScenePath() != path)
				{
					if (askForSave && !CSSceneTools.SaveCurrentSceneIfUserWantsTo())
					{
						return false;
					}
					
					CSSceneTools.OpenScene(path);
				}
			}

			return true;
		}

		private GameObject GetGameObjectWithThisIssue()
		{
			GameObject[] allObjects;

			if (location == RecordLocation.Scene)
			{
				allObjects = CSEditorTools.GetAllSuitableGameObjectsInCurrentScene();
			}
			else
			{
				List<GameObject> prefabs = new List<GameObject>();
				CSEditorTools.GetAllSuitableGameObjectsInPrefabAssets(prefabs);
				allObjects = prefabs.ToArray();
			}

			return FindObjectInCollection(allObjects);
		}

		private Component GetComponentWithThisIssue(GameObject go)
		{
			Component component = null;
			Component[] components = go.GetComponents<Component>();
			for (int i = 0; i < components.Length; i++)
			{
				if (CSObjectTools.GetLocalIdentifierInFileForObject(components[i]) == componentId)
				{
					component = components[i];
					break;
				}
			}

			return component;
		}

		private GameObject FindObjectInCollection(IEnumerable<GameObject> allObjects)
		{
			GameObject candidate = null;

			foreach (GameObject gameObject in allObjects)
			{
				if (CSEditorTools.GetFullTransformPath(gameObject.transform) != gameObjectPath) continue;

				candidate = gameObject;
				if (objectId == CSObjectTools.GetLocalIdentifierInFileForObject(candidate))
				{
					break;
				}
			}
			return candidate;
		}
	}
}
#endif