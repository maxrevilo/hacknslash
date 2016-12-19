#if UNITY_EDITOR

#define UNITY_5_PLUS
#if UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#undef UNITY_5_PLUS
#endif

using UnityEngine;
using UnityEditor;

namespace CodeStage.Maintainer.Tools
{
	public class CSObjectTools
	{
		

		internal static long GetLocalIdentifierInFileForObject(Object unityObject)
		{
			long id = -1;

			if (unityObject == null) return id;

			SerializedObject serializedObject = new SerializedObject(unityObject);
			CSReflectionTools.GetInspectorModePropertyInfo().SetValue(serializedObject, InspectorMode.Debug, null);
			SerializedProperty serializedProperty = serializedObject.FindProperty("m_LocalIdentfierInFile");
#if UNITY_5_PLUS
			id = serializedProperty.longValue;
#else
			id = serializedProperty.intValue;
#endif
			if (id <= 0)
			{
				PrefabType prefabType = PrefabUtility.GetPrefabType(unityObject);
				if (prefabType != PrefabType.None)
				{
					id = GetLocalIdentifierInFileForObject(PrefabUtility.GetPrefabParent(unityObject));
				}
				else
				{
					// this will work for the new objects in scene which weren't saved yet
					id = unityObject.GetInstanceID();
				}
			}

			if (id <= 0)
			{
				GameObject go = unityObject as GameObject;
				if (go != null)
				{
					id = go.transform.GetSiblingIndex();
				}
			}

			return id;
		}

		internal static string GetNativeObjectType(Object unityObject)
		{
			string result;

			try
			{
				string fullName = unityObject.ToString();
				int openingIndex = fullName.IndexOf('(') + 1;
				if (openingIndex != 0)
				{
					int closingIndex = fullName.LastIndexOf(')');
					result = fullName.Substring(openingIndex, closingIndex - openingIndex);
				}
				else
				{
					result = null;
				}
			}
			catch
			{
				result = null;
			}

			return result;
		}

		internal static void SelectGameObject(GameObject go, RecordLocation location)
		{
			if (location == RecordLocation.Scene)
			{
				Selection.activeTransform = go == null ? null : go.transform;
			}
			else
			{
				Selection.activeGameObject = go;
			}
		}
	}
}

#endif