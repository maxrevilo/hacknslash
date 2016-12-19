#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;

namespace CodeStage.Maintainer.Tools
{
	public class CSReflectionTools
	{
		// for caching
		private static PropertyInfo inspectorModePropertyInfo;
		private static PropertyInfo sortingLayersPropertyInfo;

		public static PropertyInfo GetInspectorModePropertyInfo()
		{
			if (inspectorModePropertyInfo == null)
			{
				inspectorModePropertyInfo = typeof(SerializedObject).GetProperty("inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
			}

			return inspectorModePropertyInfo;
		}

		public static PropertyInfo GetSortingLayersPropertyInfo()
		{
			if (sortingLayersPropertyInfo == null)
			{
				sortingLayersPropertyInfo = typeof(InternalEditorUtility).GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
			}

			return sortingLayersPropertyInfo;
		}
	}
}
#endif