﻿#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer
{
	/// <summary>
	/// Use it to guess current directory of the Maintainer.
	/// </summary>
	public class MaintainerMarker : ScriptableObject
	{
		/// <summary>
		/// Returns raw path of the MaintainerMarker script for further reference.
		/// </summary>
		/// <returns>Path of the MaintainerMarker ScriptableObject asset.</returns>
		public static string GetAssetPath()
		{
			string result;

			MaintainerMarker tempInstance = CreateInstance<MaintainerMarker>();
			MonoScript script = MonoScript.FromScriptableObject(tempInstance);
			if (script != null)
			{
				result = AssetDatabase.GetAssetPath(script);
			}
			else
			{
				result = AssetDatabase.FindAssets("MaintainerMarker")[0];
				result = AssetDatabase.GUIDToAssetPath(result);
			}
			
			DestroyImmediate(tempInstance);
			return result;
		}
	}
}
#endif