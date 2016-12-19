#if UNITY_EDITOR

#if UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#define UNITY_5_0_TO_5_2
#endif

#if UNITY_5_1 || UNITY_5_2
#define UNITY_5_1_TO_5_2
#endif

#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#endif

#if UNITY_5_3_PLUS
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#else
using System.Collections.Generic;
using UnityEditor;
#endif

namespace CodeStage.Maintainer.Tools
{
	public class CSSceneTools
	{
		public static string GetCurrentScenePath()
		{
#if UNITY_5_3_PLUS
			return SceneManager.GetActiveScene().path;
#else
			return EditorApplication.currentScene;
#endif
		}

		public static void NewScene(bool empty = false)
		{
#if UNITY_5_3_PLUS
			EditorSceneManager.NewScene(empty ? NewSceneSetup.EmptyScene : NewSceneSetup.DefaultGameObjects);
#else
			EditorApplication.NewScene();
#endif
		}

		public static void OpenScene(string path)
		{
#if UNITY_5_3_PLUS
			// we can't open scene without path
			if (string.IsNullOrEmpty(path)) return;

			Scene targetScene = SceneManager.GetSceneByPath(path);

			// we don't need to do anything if target scene is already active
			if (targetScene == SceneManager.GetActiveScene()) return;

			if (targetScene.isLoaded)
			{
				// already loaded, so just make it active
				SceneManager.SetActiveScene(targetScene);
			}
			else if (SceneManager.sceneCount > 1)
			{
				// to avoid any data loss, we need to open scene additive if we have multi scene setup
				EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
				SceneManager.SetActiveScene(targetScene);
			}
			else
			{
				// open not loaded scene as usual
				EditorSceneManager.OpenScene(path);
			}
#else
			EditorApplication.OpenScene(path);
#endif
		}

		public static bool SaveCurrentSceneIfUserWantsTo()
		{
#if UNITY_5_3_PLUS
			return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
#else
			return EditorApplication.SaveCurrentSceneIfUserWantsTo();
#endif
		}

		public static bool SaveCurrentScene()
		{
#if UNITY_5_3_PLUS
			return EditorSceneManager.SaveOpenScenes();
#else
			return EditorApplication.SaveScene();
#endif
		}

#if UNITY_5_3_PLUS
		public static void CloseAllScenesButActive()
		{
			Scene activeScene = SceneManager.GetActiveScene();
			int count = SceneManager.sceneCount;

			List<Scene> scenesToClose = new List<Scene>(count);

			for (int j = 0; j < count; j++)
			{
				Scene scene = SceneManager.GetSceneAt(j);
				if (scene != activeScene)
				{
					scenesToClose.Add(scene);
				}
			}

			foreach (var scene in scenesToClose)
			{
				EditorSceneManager.CloseScene(scene, true);
			}
		}
#endif

		public static string[] GetScenesInBuild(bool includeDisabled = false)
		{
			EditorBuildSettingsScene[] scenesForBuild = EditorBuildSettings.scenes;
			List<string> scenesInBuild = new List<string>(scenesForBuild.Length);

			foreach (EditorBuildSettingsScene sceneInBuild in scenesForBuild)
			{
				if (sceneInBuild.enabled || includeDisabled)
				{
					scenesInBuild.Add(sceneInBuild.path);
				}
			}
			return scenesInBuild.ToArray();
		}


		public static void MarkSceneDirty()
		{
#if UNITY_5_0_TO_5_2
			EditorApplication.MarkSceneDirty();
#elif UNITY_5_3_PLUS
			EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
#endif
		}


		public static bool CurrentSceneIsDirty()
		{
#if UNITY_5_1_TO_5_2
			return EditorApplication.isSceneDirty;
#elif UNITY_5_3_PLUS
			return SceneManager.GetActiveScene().isDirty;
#else
			return true;
#endif
		}
	}
}

#endif