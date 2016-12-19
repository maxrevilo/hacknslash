#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#define UNITY_5_2_MINUS
#endif

#define UNITY_5_5_PLUS
#if UNITY_5_2_MINUS || UNITY_5_3 || UNITY_5_4
#undef UNITY_5_5_PLUS
#endif

#if UNITY_5_3_PLUS
using UnityEditor.SceneManagement;
#endif

#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Issues
{
	/// <summary>
	/// Allows to find issues in your Unity project. See readme for details.
	/// </summary>
	public class IssuesFinder
	{
		internal const string MODULE_NAME = "Issues Finder";
		private const string PROGRESS_CAPTION = MODULE_NAME + ": phase {0} of {1}, item {2} of {3}";

		private static string[] scenesPaths;
		private static List<GameObject> prefabs;
		private static List<string> prefabsPaths;

		private static int phasesCount;
		private static int currentPhase;

		private static int scenesCount;
		private static int prefabsCount;

		private static int toFix;

		private static string searchStartScene;

		#region public methods

		/////////////////////////////////////////////////////////////////////////
		// public methods
		/////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// Starts issues search and generates report. %Maintainer window is not shown.
		/// Useful when you wish to integrate %Maintainer in your build pipeline.
		/// </summary>
		/// <returns>%Issues report, similar to the exported report from the %Maintainer window.</returns>
		public static string SearchAndReport()
		{
			IssueRecord[] foundIssues = StartSearch(false);

			// ReSharper disable once CoVariantArrayConversion
			return ReportsBuilder.GenerateReport(MODULE_NAME, foundIssues);
		}

		/// <summary>
		/// Starts search with current settings.
		/// </summary>
		/// <param name="showResults">Shows results in the %Maintainer window if true.</param>
		/// <returns>Array of IssueRecords in case you wish to manually iterate over them and make custom report.</returns>
		public static IssueRecord[] StartSearch(bool showResults)
		{
			phasesCount = 0;

            if (MaintainerSettings.Issues.scanGameObjects && MaintainerSettings.Issues.lookInScenes)
			{
				searchStartScene = CSSceneTools.GetCurrentScenePath();

				if (MaintainerSettings.Issues.scenesSelection != IssuesFinderSettings.ScenesSelection.CurrentSceneOnly)
				{
					if (!CSSceneTools.SaveCurrentSceneIfUserWantsTo())
					{
						Debug.Log(Maintainer.LOG_PREFIX + "Issues search canceled by user!");
						return null;
					}
					else
					{
						if (CSSceneTools.CurrentSceneIsDirty()) CSSceneTools.NewScene(true);
					}
				}
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			List<IssueRecord> issues = new List<IssueRecord>();
			Stopwatch sw = Stopwatch.StartNew();

			try
			{
				CollectInput();

				bool searchCanceled = false;

				if (MaintainerSettings.Issues.scanGameObjects)
				{
					if (MaintainerSettings.Issues.lookInScenes)
					{
						searchCanceled = !ProcessSelectedScenes(issues);
					}

					if (!searchCanceled && MaintainerSettings.Issues.lookInAssets)
					{
						searchCanceled = !ProcessPrefabFiles(issues);
					}
				}

				if (MaintainerSettings.Issues.scanProjectSettings)
				{
					if (!searchCanceled)
					{
						searchCanceled = !ProcessSettings(issues);
					}
				}
				sw.Stop();

				if (!searchCanceled)
				{
					Debug.Log(Maintainer.LOG_PREFIX + MODULE_NAME + " results: " + issues.Count +
					          " issues in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
					          " seconds, " + scenesCount + " scenes and " + prefabsCount + " prefabs scanned.");
				}
				else
				{
					Debug.Log(Maintainer.LOG_PREFIX + "Search canceled by user!");
				}

				SearchResultsStorage.IssuesSearchResults = issues.ToArray();
				if (showResults) MaintainerWindow.ShowIssues();
			}
			catch (Exception e)
			{
				Debug.LogError(Maintainer.LOG_PREFIX + MODULE_NAME + ": something went wrong :(\n" + e);
			}

			FinishSearch();

			return issues.ToArray();
		}

		/// <summary>
		/// Starts fix of the issues found with StartSearch() method.
		/// </summary>
		/// <param name="recordsToFix">Pass records you wish to fix here or leave null to let it load last search results.</param>
		/// <param name="showResults">Shows results in the %Maintainer window if true.</param>
		/// <param name="showConfirmation">Shows confirmation dialog before performing fix if true.</param>
		/// <returns>Array of IssueRecords which were fixed up.</returns>
		public static IssueRecord[] StartFix(IssueRecord[] recordsToFix = null, bool showResults = true, bool showConfirmation = true)
		{
			IssueRecord[] records = recordsToFix;
			if (records == null)
			{
				records = SearchResultsStorage.IssuesSearchResults;
			}

			if (records.Length == 0)
			{
				return null;
			}

			if (!CSSceneTools.SaveCurrentSceneIfUserWantsTo())
			{
				return null;
			}

			searchStartScene = CSSceneTools.GetCurrentScenePath();

			toFix = 0; 

			foreach (var record in records)
			{
				if (record.selected) toFix++;
			}

			if (toFix == 0)
			{
				EditorUtility.DisplayDialog(MODULE_NAME, "Please select issues to fix!", "Ok");
				return null;
			}

			if (showConfirmation && !EditorUtility.DisplayDialog("Confirmation", "Do you really wish to let Maintainer automatically fix " + toFix + " issues?\n" + Maintainer.DATA_LOSS_WARNING, "Go for it!", "Cancel"))
			{
				return null;
			}
			
			Stopwatch sw = Stopwatch.StartNew();

			bool canceled = FixRecords(records);

			List<IssueRecord> fixedRecords = new List<IssueRecord>(records.Length);
			List<IssueRecord> notFixedRecords = new List<IssueRecord>(records.Length);

			foreach (var record in records)
			{
				if (record.@fixed)
				{
					fixedRecords.Add(record);
				}
				else
				{
					notFixedRecords.Add(record);
				}
			}

			records = notFixedRecords.ToArray();

			sw.Stop();

			EditorUtility.ClearProgressBar();

			if (!canceled)
			{
				string results = fixedRecords.Count +
				                 " issues fixed in " + sw.Elapsed.TotalSeconds.ToString("0.000") +
				                 " seconds";

				Debug.Log(Maintainer.LOG_PREFIX + MODULE_NAME + " results: " + results);
				MaintainerWindow.ShowNotification(results);
			}
			else
			{
				Debug.Log(Maintainer.LOG_PREFIX + "Fix canceled by user!");
			}

			if (!string.IsNullOrEmpty(CSSceneTools.GetCurrentScenePath()) && CSSceneTools.CurrentSceneIsDirty()) CSSceneTools.SaveCurrentScene();

			if (CSSceneTools.GetCurrentScenePath() != searchStartScene)
			{
				if (!File.Exists(searchStartScene))
				{
					CSSceneTools.NewScene();
				}
				else
				{
					EditorUtility.DisplayProgressBar("Opening initial scene", "Opening scene: " + Path.GetFileNameWithoutExtension(searchStartScene), 0);
					CSSceneTools.OpenScene(searchStartScene);
				}
			}

			EditorUtility.ClearProgressBar();

			SearchResultsStorage.IssuesSearchResults = records;
			if (showResults) MaintainerWindow.ShowIssues();

			return fixedRecords.ToArray();
		}

		#endregion

		#region searcher

		/////////////////////////////////////////////////////////////////////////
		// searcher
		/////////////////////////////////////////////////////////////////////////

		private static void CollectInput()
		{
			phasesCount = 0;
			currentPhase = 0;

			scenesCount = 0;
			prefabsCount = 0;

			if (MaintainerSettings.Issues.scanGameObjects)
			{
				if (MaintainerSettings.Issues.lookInScenes)
				{
					EditorUtility.DisplayProgressBar(MODULE_NAME, "Collecting input data: Scenes...", 0);

					switch (MaintainerSettings.Issues.scenesSelection)
					{
						case IssuesFinderSettings.ScenesSelection.AllScenes:
						{
							scenesPaths = CSEditorTools.FindAssetsFiltered("t:Scene", MaintainerSettings.Issues.pathIncludes, MaintainerSettings.Issues.pathIgnores);
							break;
						}
						case IssuesFinderSettings.ScenesSelection.IncludedScenes:
						{
							if (MaintainerSettings.Issues.includeScenesInBuild)
							{
								scenesPaths = CSSceneTools.GetScenesInBuild(!MaintainerSettings.Issues.includeOnlyEnabledScenesInBuild);
							}

							ArrayUtility.AddRange(ref scenesPaths, MaintainerSettings.Issues.sceneIncludes);
							break;
						}
						default:
						{
							scenesPaths = new[] {CSSceneTools.GetCurrentScenePath()};
							break;
						}
					}

					scenesCount = scenesPaths.Length;

					/*for (int i = 0; i < scenesCount; i++)
					{
						scenesPaths[i] = scenesPaths[i].Replace('\\', '/');
					}*/

					phasesCount++;
				}

				if (MaintainerSettings.Issues.lookInAssets)
				{
					if (prefabs == null)
						prefabs = new List<GameObject>();
					else
						prefabs.Clear();

					if (prefabsPaths == null)
						prefabsPaths = new List<string>();
					else
						prefabsPaths.Clear();

					EditorUtility.DisplayProgressBar(MODULE_NAME, "Collecting input data: Prefabs...", 0);

					string[] filteredPaths = CSEditorTools.FindAssetsFiltered("t:Prefab", MaintainerSettings.Issues.pathIncludes, MaintainerSettings.Issues.pathIgnores);
					prefabsCount = CSEditorTools.GetSuitablePrefabsFromSelection(filteredPaths, prefabs, prefabsPaths);

					phasesCount++;
				}
			}

			if (MaintainerSettings.Issues.scanProjectSettings)
			{
				phasesCount++;
			}
		}

		private static bool ProcessSelectedScenes(List<IssueRecord> issues)
		{
			bool result = true;
			currentPhase ++;

			for (int i = 0; i < scenesCount; i++)
			{
				string scenePath = scenesPaths[i];
				string sceneName = Path.GetFileNameWithoutExtension(scenePath);

				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i+1, scenesCount), string.Format("Opening scene: " + Path.GetFileNameWithoutExtension(scenePath)), (float)i / scenesCount))
				{
					result = false;
					break;
				}

				if (MaintainerSettings.Issues.scenesSelection != IssuesFinderSettings.ScenesSelection.CurrentSceneOnly && 
					!File.Exists(scenePath)) continue;

				CSSceneTools.OpenScene(scenePath);

				if (CSSceneTools.GetCurrentScenePath() != scenePath)
				{
					CSSceneTools.OpenScene(scenePath);
				}	
#if UNITY_5_3_PLUS
				// if we're scanning currently opened scene and going to scan more scenes,
				// we need to close all additional scenes to avoid duplicates
				else if (EditorSceneManager.loadedSceneCount > 1 && scenesCount > 1)
				{
					CSSceneTools.CloseAllScenesButActive();
				}
#endif

				GameObject[] gameObjects = CSEditorTools.GetAllSuitableGameObjectsInCurrentScene();
				int objectsCount = gameObjects.Length;

				for (int j = 0; j < objectsCount; j++)
				{
					if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i+1, scenesCount), string.Format("Processing scene: {0} ... {1}%", sceneName, j * 100 / objectsCount), (float)i / scenesCount))
					{
						result = false;
						break;
					}

					CheckObjectForIssues(issues, scenePath, gameObjects[j], true);
				}

				if (!result) break;
			}

			return result;
		}

		private static bool ProcessPrefabFiles(List<IssueRecord> issues)
		{
			bool result = true;
			currentPhase++;

			for (int i = 0; i < prefabsCount; i++)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i+1, prefabsCount), "Processing prefabs files...", (float)i / prefabsCount))
				{
					result = false;
					break;
				}

				CheckObjectForIssues(issues, prefabsPaths[i], prefabs[i], false);
			}

			return result;
		}

		private static void CheckObjectForIssues(List<IssueRecord> issues, string path, GameObject go, bool checkingScene)
		{
			RecordLocation location = checkingScene ? RecordLocation.Scene : RecordLocation.Prefab;

			// ----------------------------------------------------------------------------
			// looking for object-level issues
			// ----------------------------------------------------------------------------

			if (!MaintainerSettings.Issues.touchInactiveGameObjects)
			{
				if (checkingScene)
				{
					if (!go.activeInHierarchy) return;
				}
				else
				{
					if (!go.activeSelf) return;
				}
			}

			// ----------------------------------------------------------------------------
			// checking stuff related to the prefabs in scenes
			// ----------------------------------------------------------------------------

			if (checkingScene)
			{
				PrefabType prefabType = PrefabUtility.GetPrefabType(go);

				if (prefabType != PrefabType.None)
				{
					/* checking if we're inside of nested prefab with same type as root,
					   allows to skip detections of missed and disconnected prefabs children */

					GameObject rootPrefab = PrefabUtility.FindRootGameObjectWithSameParentPrefab(go);
					bool rootPrefabHasSameType = false;
					if (rootPrefab != go)
					{
						PrefabType rootPrefabType = PrefabUtility.GetPrefabType(rootPrefab);
						if (rootPrefabType == prefabType)
						{
							rootPrefabHasSameType = true;
						}
					}

					/* checking for missing and disconnected instances */

					if (prefabType == PrefabType.MissingPrefabInstance)
					{
						if (MaintainerSettings.Issues.missingPrefabs && !rootPrefabHasSameType)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.MissingPrefab, location, path, go));
						}
					}
					else if (prefabType == PrefabType.DisconnectedPrefabInstance ||
							 prefabType == PrefabType.DisconnectedModelPrefabInstance)
					{
						if (MaintainerSettings.Issues.disconnectedPrefabs && !rootPrefabHasSameType)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.DisconnectedPrefab, location, path, go));
						}
					}

					/* checking if this game object is actually prefab instance
					   without any changes, so we can skip it if we have assets search enabled */

					if (prefabType != PrefabType.DisconnectedPrefabInstance &&
						prefabType != PrefabType.DisconnectedModelPrefabInstance &&
						prefabType != PrefabType.MissingPrefabInstance && MaintainerSettings.Issues.lookInAssets)
					{
						bool skipThisPrefabInstance = true;
						 
						// we shouldn't skip object if it's nested deeper 2nd level
						if (CSEditorTools.GetDepthInHierarchy(go.transform, rootPrefab.transform) >= 2)
						{
							skipThisPrefabInstance = false;
						}
						else
						{
							PropertyModification[] modifications = PrefabUtility.GetPropertyModifications(go);
							foreach (PropertyModification modification in modifications)
							{
								Object target = modification.target;

								if (target is Transform)
								{
									if (!MaintainerSettings.Issues.hugePositions) continue;

									Transform transform = (Transform)target;
									if (!TransformHasHugePosition(transform)) continue;
								}

								if (target is GameObject && modification.propertyPath == "m_Name")
								{
									continue;
								}

								skipThisPrefabInstance = false;
								break;
							}
						}

						if (skipThisPrefabInstance)
						{
							GameObject parentObject = PrefabUtility.GetPrefabParent(go) as GameObject;
							if (parentObject != null)
							{
								Component[] goComponents = go.GetComponents<Component>();
								Component[] prefabComponents = parentObject.GetComponents<Component>();
								if (goComponents.Length > prefabComponents.Length)
								{
									skipThisPrefabInstance = false;
								}
							}
						}

						if (skipThisPrefabInstance) return;
					}
				}
			}

			// ----------------------------------------------------------------------------
			// checking for Game Object - level issues
			// ----------------------------------------------------------------------------

			if (MaintainerSettings.Issues.undefinedTags)
			{
				bool undefinedTag = false;
				try
				{
					if (string.IsNullOrEmpty(go.tag))
					{
						undefinedTag = true;
					}
				}
				catch (UnityException e)
				{
					if (e.Message.Contains("undefined tag"))
					{
						undefinedTag = true;
					}
					else
					{
						Debug.LogError(Maintainer.LOG_PREFIX + "Unknown error while checking tag of the " + go.name + "\n" + e);
					}
				}

				if (undefinedTag)
				{
					issues.Add(GameObjectIssueRecord.Create(RecordType.UndefinedTag, location, path, go));
				}
			}

			if (MaintainerSettings.Issues.unnamedLayers)
			{
				int layerIndex = go.layer;
				if (string.IsNullOrEmpty(LayerMask.LayerToName(layerIndex)))
				{
					GameObjectIssueRecord issue = GameObjectIssueRecord.Create(RecordType.UnnamedLayer, location, path, go);
					issue.headerExtra = "(index: " + layerIndex + ")";
					issues.Add(issue);
				}
			}

			// ----------------------------------------------------------------------------
			// checking all components for ignores
			// ----------------------------------------------------------------------------

			bool checkForIgnores = MaintainerSettings.Issues.componentIgnores != null && MaintainerSettings.Issues.componentIgnores.Length > 0;
			bool skipEmptyMeshFilter = false;
			bool skipEmptyAudioSource = false;

			Component[] allComponents = go.GetComponents<Component>();
			int allComponentsCount = allComponents.Length;

			List<Component> components = new List<Component>(allComponentsCount);
			List<Type> componentsTypes = new List<Type>(allComponentsCount);
			List<string> componentsNames = new List<string>(allComponentsCount);
			List<string> componentsNamespaces = new List<string>(allComponentsCount);

			int componentsCount = 0;
			int missingComponentsCount = 0;

			for (int i = 0; i < allComponentsCount; i++)
			{
				Component component = allComponents[i];

				if (component == null)
				{
					missingComponentsCount++;
					continue;
				}

				Type componentType = component.GetType();
				string componentName = componentType.Name;
				string componentFullName = componentType.FullName;
				string componentNamespace = componentType.Namespace;

				/* 
				*  checking object for the components which may affect 
				*  other components and produce false positives 
				*/

				// allowing empty mesh filters for the objects with attached TextMeshPro and 2D Toolkit components.
				if (!skipEmptyMeshFilter)
				{
					skipEmptyMeshFilter = (componentFullName == "TMPro.TextMeshPro") || componentName.StartsWith("tk2d");
				}

				// allowing empty AudioSources for the objects with attached standard FirstPersonController.
				if (!skipEmptyAudioSource)
				{
					skipEmptyAudioSource = componentFullName == "UnityStandardAssets.Characters.FirstPerson.FirstPersonController";
				}

				// skipping disabled components
				if (!MaintainerSettings.Issues.touchDisabledComponents)
				{
					if (EditorUtility.GetObjectEnabled(component) == 0) continue;
				}

				// skipping ignored components
				if (checkForIgnores)
				{
					if (Array.IndexOf(MaintainerSettings.Issues.componentIgnores, componentName) != -1) continue;
				}

				components.Add(component);
				componentsTypes.Add(componentType);
				componentsNames.Add(componentName);
				componentsNamespaces.Add(componentNamespace);
				componentsCount++;
			}

			if (missingComponentsCount > 0 && MaintainerSettings.Issues.missingComponents)
			{
				GameObjectIssueRecord record = GameObjectIssueRecord.Create(RecordType.MissingComponent, location, path, go, null, null, null);
				record.headerFormatArgument = missingComponentsCount;
				issues.Add(record);
			}

			Dictionary<string, int> uniqueTypes = null;
			List<int> similarComponentsIndexes = null;

			TerrainData terrainTerrainData = null;
			TerrainData terrainColliderTerrainData = null;
			bool terrainChecked = false;
			bool terrainColliderChecked = false;

			// ----------------------------------------------------------------------------
			// looking for component-level issues
			// ----------------------------------------------------------------------------

			for (int i = 0; i < componentsCount; i++)
			{
				Component component = components[i];
				Type componentType = componentsTypes[i];
				string componentName = componentsNames[i];
				//string componentFullName = componentsFullNames[i];
				string componentNamespace = componentsNamespaces[i];

				if (component is Transform)
				{
					if (MaintainerSettings.Issues.hugePositions)
					{
						if (TransformHasHugePosition((Transform)component))
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.HugePosition, location, path, go, component, componentType, componentName, "Position"));
						}
					}
					continue;
				}

				if (MaintainerSettings.Issues.duplicateComponents &&
					(componentNamespace != "Fabric"))
				{
					// initializing dictionary and list on first usage
					if (uniqueTypes == null) uniqueTypes = new Dictionary<string, int>(componentsCount);
					if (similarComponentsIndexes == null) similarComponentsIndexes = new List<int>(componentsCount);

					string realComponentType = CSObjectTools.GetNativeObjectType(component);
					if (string.IsNullOrEmpty(realComponentType)) realComponentType = componentType.ToString();

					// checking if current component type already met before
					if (uniqueTypes.ContainsKey(realComponentType))
					{
						int uniqueTypeIndex = uniqueTypes[realComponentType];

						// checking if initially met component index already in indexes list
						// since we need to compare all duplicate candidates against initial component
						if (!similarComponentsIndexes.Contains(uniqueTypeIndex)) similarComponentsIndexes.Add(uniqueTypeIndex);

						// adding current component index to the indexes list
						similarComponentsIndexes.Add(i);
					}
					else
					{
						uniqueTypes.Add(realComponentType, i);
					}
				}

				// ----------------------------------------------------------------------------
				// looping through the component's SerializedProperties via SerializedObject
				// ----------------------------------------------------------------------------

				Dictionary<string, int> emptyArrayItems = new Dictionary<string, int>();
				SerializedObject so = new SerializedObject(component);
				SerializedProperty sp = so.GetIterator();
				int arrayLength = 0;
				bool skipEmptyComponentCheck = false;

				while (sp.NextVisible(true))
				{
					string fullPropertyPath = sp.propertyPath;

					if (sp.isArray)
					{
						arrayLength = sp.arraySize;
					}

					bool isArrayItem = fullPropertyPath.EndsWith("]", StringComparison.Ordinal);

					if (MaintainerSettings.Issues.missingReferences)
					{
						if (sp.propertyType == SerializedPropertyType.ObjectReference)
						{
							if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
							{
								string propertyName = isArrayItem ? GetArrayItemNameAndIndex(fullPropertyPath) : sp.name;
								GameObjectIssueRecord record = GameObjectIssueRecord.Create(RecordType.MissingReference, location, path, go, component, componentType, componentName, propertyName);
								record.propertyPath = sp.propertyPath;
								issues.Add(record);

								if (component is MeshCollider && sp.name == "m_Mesh")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is MeshFilter && sp.name == "m_Mesh")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is Renderer && fullPropertyPath.StartsWith("m_Materials.Array.") && arrayLength == 1)
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is SpriteRenderer && sp.name == "m_Sprite")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is Animation && sp.name == "m_Animation")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is TerrainCollider && sp.name == "m_TerrainData")
								{
									skipEmptyComponentCheck = true;
								}
								else if (component is AudioSource && sp.name == "m_audioClip")
								{
									skipEmptyComponentCheck = true;
								}
							}
						}
					}

					if (checkingScene || !MaintainerSettings.Issues.skipEmptyArrayItemsOnPrefabs)
					{
						// skipping SpriteRenderer as it has hidden array with materials of size 1
						if (MaintainerSettings.Issues.emptyArrayItems && isArrayItem)
						{
							// ignoring components where empty array items is a normal behavior
							if (component is SpriteRenderer) continue;
							if (component is MeshRenderer && arrayLength == 1) continue;
							if (componentName.StartsWith("TextMeshPro")) continue;

							if (sp.propertyType == SerializedPropertyType.ObjectReference &&
							    sp.objectReferenceValue == null &&
							    sp.objectReferenceInstanceIDValue == 0)
							{
								string arrayName = GetArrayItemName(fullPropertyPath);

								// ignoring TextMeshPro's FontAssetArrays with 16 empty items inside
								if (!emptyArrayItems.ContainsKey(arrayName))
								{
									emptyArrayItems.Add(arrayName, 0);
								}
								emptyArrayItems[arrayName]++;
							}
						}
					}
					/*else
					{
						continue;
					}*/
				}

				if (MaintainerSettings.Issues.emptyArrayItems)
				{
					foreach (var item in emptyArrayItems.Keys)
					{
						GameObjectIssueRecord issueRecord = GameObjectIssueRecord.Create(RecordType.EmptyArrayItem, location, path, go, component, componentType, componentName, item);
						issueRecord.headerFormatArgument = emptyArrayItems[item];
						issues.Add(issueRecord);
					}
				}

				// ----------------------------------------------------------------------------
				// specific components checks
				// ----------------------------------------------------------------------------

				if (component is MeshCollider)
				{
					if (MaintainerSettings.Issues.emptyMeshColliders && !skipEmptyComponentCheck)
					{
						if ((component as MeshCollider).sharedMesh == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyMeshCollider, location, path, go, component, componentType, componentName));
						}
					}
				}
				else if (component is MeshFilter)
				{
					if (MaintainerSettings.Issues.emptyMeshFilters && !skipEmptyMeshFilter && !skipEmptyComponentCheck)
					{
						if ((component as MeshFilter).sharedMesh == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyMeshFilter, location, path, go, component, componentType, componentName));
						}
					}
				}
				else if (component is Renderer)
				{
					Renderer renderer = (Renderer)component;
					if (MaintainerSettings.Issues.emptyRenderers && !skipEmptyComponentCheck)
					{
						bool hasMaterial = false;
						foreach (Material material in renderer.sharedMaterials)
						{
							if (material != null)
							{
								hasMaterial = true;
								break;
							}
						}

#if UNITY_5_5_PLUS
						var particleSystemRenderer = renderer as ParticleSystemRenderer;
						if (particleSystemRenderer != null)
						{
							if (particleSystemRenderer.renderMode == ParticleSystemRenderMode.None)
							{
								hasMaterial = true;
							}
						}
#endif

						if (!hasMaterial)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyRenderer, location, path, go, component, componentType, componentName));
						}
					}

					if (component is SpriteRenderer)
					{
						if (MaintainerSettings.Issues.emptySpriteRenderers && !skipEmptyComponentCheck)
						{
							if ((component as SpriteRenderer).sprite == null)
							{
								issues.Add(GameObjectIssueRecord.Create(RecordType.EmptySpriteRenderer, location, path, go, component, componentType, componentName));
							}
						}
					}
				}
				else if (component is Animation)
				{
					if (MaintainerSettings.Issues.emptyAnimations && !skipEmptyComponentCheck)
					{
						Animation animation = (Animation)component;
						bool isEmpty = false;
						if (animation.GetClipCount() <= 0 && animation.clip == null)
						{
							isEmpty = true;
						}
						else
						{
							int clipsCount = 0;
							
							foreach (var clip in animation)
							{
								if (clip != null) clipsCount++;
							}

							if (clipsCount == 0)
							{
								isEmpty = true;
							}
						}

						if (isEmpty)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyAnimation, location, path, go, component, componentType, componentName));
						}
					}
				}
				else if (component is Terrain)
				{
					if (MaintainerSettings.Issues.inconsistentTerrainData)
					{
						terrainTerrainData = (component as Terrain).terrainData;
						terrainChecked = true;
					}
				}
				else if (component is TerrainCollider)
				{
					if (MaintainerSettings.Issues.inconsistentTerrainData)
					{
						terrainColliderTerrainData = (component as TerrainCollider).terrainData;
						terrainColliderChecked = true;
					}

					if (MaintainerSettings.Issues.emptyTerrainCollider && !skipEmptyComponentCheck)
					{
						if ((component as TerrainCollider).terrainData == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyTerrainCollider, location, path, go, component, componentType, componentName));
						}
					}
				}
				else if (component is AudioSource)
				{
					if (MaintainerSettings.Issues.emptyAudioSource && !skipEmptyAudioSource && !skipEmptyComponentCheck)
					{
						if ((component as AudioSource).clip == null)
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.EmptyAudioSource, location, path, go, component, componentType, componentName));
						}
					}
				}
			}

			if (MaintainerSettings.Issues.inconsistentTerrainData && 
				terrainColliderTerrainData != terrainTerrainData &&
				terrainChecked && terrainColliderChecked)
			{
				issues.Add(GameObjectIssueRecord.Create(RecordType.InconsistentTerrainData, location, path, go));
			}

			// ----------------------------------------------------------------------------
			// duplicates search
			// ----------------------------------------------------------------------------

			if (MaintainerSettings.Issues.duplicateComponents)
			{
				if (similarComponentsIndexes != null && similarComponentsIndexes.Count > 0)
				{
					int similarComponentsCount = similarComponentsIndexes.Count;
					List<long> similarComponentsHashes = new List<long>(similarComponentsCount);

					for (int i = 0; i < similarComponentsCount; i++)
					{
						int componentIndex = similarComponentsIndexes[i];
						Component component = components[componentIndex];

						long componentHash = 0;

						if (MaintainerSettings.Issues.duplicateComponentsPrecise)
						{
							SerializedObject so = new SerializedObject(component);
							SerializedProperty sp = so.GetIterator();
							while (sp.NextVisible(true))
							{
								componentHash += CSEditorTools.GetPropertyHash(sp);
							}
						}

						similarComponentsHashes.Add(componentHash);
					}

					List<long> distinctItems = new List<long>(similarComponentsCount);

					for (int i = 0; i < similarComponentsCount; i++)
					{
						int componentIndex = similarComponentsIndexes[i];

						if (distinctItems.Contains(similarComponentsHashes[i]))
						{
							issues.Add(GameObjectIssueRecord.Create(RecordType.DuplicateComponent, location, path, go, components[componentIndex], componentsTypes[componentIndex], componentsNames[componentIndex]));
						}
						else
						{
							distinctItems.Add(similarComponentsHashes[i]);
						}
					}
				}
			}
		}

		private static bool TransformHasHugePosition(Transform transform)
		{
			Vector3 position = transform.position;

			if (Math.Abs(position.x) > 100000f || Math.Abs(position.y) > 100000f || Math.Abs(position.z) > 100000f)
			{
				return true;
			}
			return false;
		}

		private static bool ProcessSettings(List<IssueRecord> issues)
		{
			bool result = true;
			currentPhase++;

			if (MaintainerSettings.Issues.duplicateScenesInBuild)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, 1, 1), "Checking settings: Build Settings", (float)0/1))
				{
					result = false;
				}
			}

			CheckBuildSettings(issues);

			if (MaintainerSettings.Issues.duplicateTagsAndLayers)
			{
				if (EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, 1, 1), "Checking settings: Tags and Layers", (float)0/1))
				{
					result = false;
				}
			}

			CheckTagsAndLayers(issues);

			return result;
		}

		private static void CheckBuildSettings(List<IssueRecord> issues)
		{
			if (MaintainerSettings.Issues.duplicateScenesInBuild)
			{
				string[] scenesForBuild = CSSceneTools.GetScenesInBuild();
				string[] duplicates = CSArrayTools.FindDuplicatesInArray(scenesForBuild);

				foreach (var duplicate in duplicates)
				{
					issues.Add(BuildSettingsIssueRecord.Create(RecordType.DuplicateScenesInBuild, 
						"<b>Duplicate scene:</b> " + CSEditorTools.NicifyAssetPath(duplicate)));
				}
			}
		}
		 
		private static void CheckTagsAndLayers(List<IssueRecord> issues)
		{
			if (MaintainerSettings.Issues.duplicateTagsAndLayers)
			{
				StringBuilder issueBody = new StringBuilder();

				/* looking for duplicates in tags*/

				List<string> tags = new List<string>(InternalEditorUtility.tags);
				tags.RemoveAll(string.IsNullOrEmpty);
				List<string> duplicateTags = CSArrayTools.FindDuplicatesInArray(tags);

				if (duplicateTags.Count > 0)
				{
					issueBody.Append("Duplicate <b>tag(s)</b>: ");

					foreach (var duplicate in duplicateTags)
					{
						issueBody.Append('"').Append(duplicate).Append("\", ");
					}
					issueBody.Length -= 2;
				}

				/* looking for duplicates in layers*/

				List<string> layers = new List<string>(InternalEditorUtility.layers);
				layers.RemoveAll(string.IsNullOrEmpty);
				List<string> duplicateLayers = CSArrayTools.FindDuplicatesInArray(layers);

				if (duplicateLayers.Count > 0)
				{
					if (issueBody.Length > 0) issueBody.AppendLine();
					issueBody.Append("Duplicate <b>layer(s)</b>: ");

					foreach (var duplicate in duplicateLayers)
					{
						issueBody.Append('"').Append(duplicate).Append("\", ");
					}
					issueBody.Length -= 2;
				}

				/* looking for duplicates in sorting layers*/

				
				

				List<string> sortingLayers = new List<string>((string[])CSReflectionTools.GetSortingLayersPropertyInfo().GetValue(null, new object[0]));
				sortingLayers.RemoveAll(string.IsNullOrEmpty);
				List<string> duplicateSortingLayers = CSArrayTools.FindDuplicatesInArray(sortingLayers);

				if (duplicateSortingLayers.Count > 0)
				{
					if (issueBody.Length > 0) issueBody.AppendLine();
					issueBody.Append("Duplicate <b>sorting layer(s)</b>: ");

					foreach (var duplicate in duplicateSortingLayers)
					{
						issueBody.Append('"').Append(duplicate).Append("\", ");
					}
					issueBody.Length -= 2;
				}

				if (issueBody.Length > 0)
				{
					issues.Add(TagsAndLayersIssueRecord.Create(RecordType.DuplicateTagsAndLayers, issueBody.ToString()));
				}

				issueBody.Length = 0;
			}
		}

		private static void FinishSearch()
		{
			if (MaintainerSettings.Issues.lookInScenes)
			{
				if (string.IsNullOrEmpty(searchStartScene) || !File.Exists(searchStartScene))
				{
					if (MaintainerSettings.Issues.scenesSelection != IssuesFinderSettings.ScenesSelection.CurrentSceneOnly)
						CSSceneTools.NewScene();
				}
				else if (CSSceneTools.GetCurrentScenePath() != searchStartScene)
				{
					EditorUtility.DisplayProgressBar("Opening initial scene", "Opening scene: " + Path.GetFileNameWithoutExtension(searchStartScene), 0);
					CSSceneTools.OpenScene(searchStartScene);
				}
			}
			EditorUtility.ClearProgressBar();
		}

		private static string GetArrayItemNameAndIndex(string fullPropertyPath)
		{
			string propertyPath = fullPropertyPath.Replace(".Array.data", "").Replace("].", "] / ");
			return propertyPath;
		}

		private static string GetArrayItemName(string fullPropertyPath)
		{
			string name = GetArrayItemNameAndIndex(fullPropertyPath);
			int lastOpeningBracketIndex = name.LastIndexOf('[');
			return name.Substring(0, lastOpeningBracketIndex);
		}

		#endregion

		#region fixer

		/////////////////////////////////////////////////////////////////////////
		// fixer
		/////////////////////////////////////////////////////////////////////////

		private static bool FixRecords(IssueRecord[] results, bool showProgress = true)
		{
			bool canceled = false;
			int i = 0;

			IssueRecord[] sortedRecords = results.OrderBy(RecordsSortings.issueRecordByPath).ToArray();

			foreach (var item in sortedRecords)
			{
				if (showProgress && EditorUtility.DisplayCancelableProgressBar(string.Format(PROGRESS_CAPTION, currentPhase, phasesCount, i + 1, toFix), "Resolving selected issues...", (float)i / toFix))
				{
					canceled = true;
					break;
				}

				if (item.selected)
				{
					if (item is GameObjectIssueRecord)
					{
						GameObjectIssueRecord gameObjectIssueRecord = (GameObjectIssueRecord)item;

						if (gameObjectIssueRecord.location == RecordLocation.Scene)
						{
							string scenePath = CSSceneTools.GetCurrentScenePath();
							if (!string.IsNullOrEmpty(scenePath) && scenePath != gameObjectIssueRecord.path)
							{
								CSSceneTools.SaveCurrentScene();
							}
						}
					}

					if (item.CanBeFixed())
					{
						item.Fix(true);
					}

					i++;
				}
			}

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			return canceled;
		}

		#endregion
	}
}

#endif