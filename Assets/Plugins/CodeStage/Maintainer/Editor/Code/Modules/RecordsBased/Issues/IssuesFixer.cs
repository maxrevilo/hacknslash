#if UNITY_EDITOR

#define UNITY_5_0_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#undef UNITY_5_0_PLUS
#endif

#define UNITY_5_2_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1
#undef UNITY_5_2_PLUS
#endif

#define UNITY_5_3_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2
#undef UNITY_5_3_PLUS
#endif

using CodeStage.Maintainer.Tools;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.Issues
{
	public class IssuesFixer
	{
		public static bool FixGameObjectIssue(GameObjectIssueRecord issue, GameObject go, Component component, RecordType type)
		{
			bool result = false;

			if (type == RecordType.MissingComponent)
			{
				bool hasIssue = GameObjectHasMissingComponent(go);

				if (hasIssue)
				{
					FixMissingComponents(issue, go);
					result = !GameObjectHasMissingComponent(go);
				}
				else
				{
					Debug.LogWarning(Maintainer.LOG_PREFIX + "Looks like this issue was already fixed:\n" + issue);
				}
			}
			else if (type == RecordType.MissingReference)
			{
				result = FixMissingReference(issue, component);
			}

			return result;
		}

		#region missing component
		// ----------------------------------------------------------------------------
		// fix missing component
		// ----------------------------------------------------------------------------

		private static void FixMissingComponents(GameObjectIssueRecord issue, GameObject go)
		{
			CSObjectTools.SelectGameObject(go, issue.location);

			ActiveEditorTracker tracker = CSEditorTools.GetActiveEditorTrackerForSelectedObject();
			tracker.RebuildIfNecessary();

			bool touched = false;

			Editor[] activeEditors = tracker.activeEditors;
			for (int i = activeEditors.Length - 1; i >= 0; i--)
			{
				Editor editor = activeEditors[i];
				if (CSObjectTools.GetLocalIdentifierInFileForObject(editor.serializedObject.targetObject) == issue.componentId)
				{
					Object.DestroyImmediate(editor.target, true);
					touched = true;
				}
			}

			if (touched)
			{
#if UNITY_5_0_PLUS
				if (issue.location == RecordLocation.Scene)
				{
					CSSceneTools.MarkSceneDirty();
				}
				else
				{
					EditorUtility.SetDirty(go);
				}
#else
				EditorUtility.SetDirty(go);
#endif
			}

			//CSObjectTools.SelectGameObject(null, issue.location);
		}

		private static bool GameObjectHasMissingComponent(GameObject go)
		{
			bool hasMissingComponent = false;
			Component[] components = go.GetComponents<Component>();
			foreach (Component c in components)
			{
				if (c == null)
				{
					hasMissingComponent = true;
					break;
				}
			}

			return hasMissingComponent;
		}
#endregion

		#region missing reference
		// ----------------------------------------------------------------------------
		// fix missing reference
		// ----------------------------------------------------------------------------

		private static bool FixMissingReference(GameObjectIssueRecord issue, Component component)
		{
			SerializedObject so = new SerializedObject(component);
			SerializedProperty sp = so.FindProperty(issue.propertyPath);

			if (sp.propertyType == SerializedPropertyType.ObjectReference)
			{
				if (sp.objectReferenceValue == null && sp.objectReferenceInstanceIDValue != 0)
				{
					sp.objectReferenceInstanceIDValue = 0;

#if UNITY_5_2_PLUS
					// fixes dirty scene flag after batch issues fix
					// due to the additional undo action
					so.ApplyModifiedPropertiesWithoutUndo();
#else
					so.ApplyModifiedProperties();
#endif

#if UNITY_5_0_PLUS
					if (issue.location == RecordLocation.Scene)
					{
						CSSceneTools.MarkSceneDirty();
					}
					else
					{
						EditorUtility.SetDirty(component);
					}
#else
					EditorUtility.SetDirty(component);
#endif
				}
			}

			return true;
		}
		#endregion
	}
}
#endif