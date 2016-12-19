#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class CSTextureLoader
	{
		private static readonly Dictionary<string, Texture2D> cachedTextures = new Dictionary<string, Texture2D>();

		public static Texture2D GetTexture(string fileName)
		{
			return GetTexture(fileName, false);
		}

		public static Texture2D GetIconTexture(string fileName)
		{
			return GetTexture(fileName, true);
		}

		private static Texture2D GetTexture(string fileName, bool icon)
		{
			Texture2D texture;
			bool isDark = EditorGUIUtility.isProSkin;

			string path = Maintainer.Directory + "/Images/For" + (isDark ? "Dark/" : "Bright/") + (icon ? "Icons/" : "") + fileName;

			if (cachedTextures.ContainsKey(path))
			{
				texture = cachedTextures[path];
			}
			else
			{
				texture = AssetDatabase.LoadAssetAtPath(path, typeof(Texture2D)) as Texture2D;
				if (texture == null)
				{
					Debug.LogError(Maintainer.LOG_PREFIX + "Some error occurred while looking for image\n" + path);
				}
				else
				{
					cachedTextures[path] = texture;
				}
			}
			return texture;
		}
	}

	internal class CSIcons
	{
		public static Texture About { get { return CSTextureLoader.GetIconTexture("About.png"); } }
		public static Texture ArrowLeft { get { return CSTextureLoader.GetIconTexture("ArrowLeft.png"); } }
		public static Texture ArrowRight { get { return CSTextureLoader.GetIconTexture("ArrowRight.png"); } }
		public static Texture AssetStore { get { return CSTextureLoader.GetIconTexture("AssetStore.png"); } }
		public static Texture AutoFix { get { return CSTextureLoader.GetIconTexture("AutoFix.png"); } }
		public static Texture Clean { get { return CSTextureLoader.GetIconTexture("Clean.png"); } }
		public static Texture Clear { get { return CSTextureLoader.GetIconTexture("Clear.png"); } }
		public static Texture Collapse { get { return CSTextureLoader.GetIconTexture("Collapse.png"); } }
		public static Texture Copy { get { return CSTextureLoader.GetIconTexture("Copy.png"); } }
		public static Texture Delete { get { return CSTextureLoader.GetIconTexture("Delete.png"); } }
		public static Texture DoubleArrowLeft { get { return CSTextureLoader.GetIconTexture("DoubleArrowLeft.png"); } }
		public static Texture DoubleArrowRight { get { return CSTextureLoader.GetIconTexture("DoubleArrowRight.png"); } }
		public static Texture Expand { get { return CSTextureLoader.GetIconTexture("Expand.png"); } }
		public static Texture Export { get { return CSTextureLoader.GetIconTexture("Export.png"); } }
		public static Texture Find { get { return CSTextureLoader.GetIconTexture("Find.png"); } }
		public static Texture Gear { get { return CSTextureLoader.GetIconTexture("Gear.png"); } }
		public static Texture Hide { get { return CSTextureLoader.GetIconTexture("Hide.png"); } }
		public static Texture Home { get { return CSTextureLoader.GetIconTexture("Home.png"); } }
		public static Texture Issue { get { return CSTextureLoader.GetIconTexture("Issue.png"); } }
		public static Texture Log { get { return CSTextureLoader.GetIconTexture("Log.png"); } }
		public static Texture Maintainer { get { return CSTextureLoader.GetIconTexture("Maintainer.png"); } }
		public static Texture Minus { get { return CSTextureLoader.GetIconTexture("Minus.png"); } }
		public static Texture More { get { return CSTextureLoader.GetIconTexture("More.png"); } }
		public static Texture Plus { get { return CSTextureLoader.GetIconTexture("Plus.png"); } }
		public static Texture Publisher { get { return CSTextureLoader.GetIconTexture("Publisher.png"); } }
		public static Texture Restore { get { return CSTextureLoader.GetIconTexture("Restore.png"); } }
		public static Texture Reveal { get { return CSTextureLoader.GetIconTexture("Reveal.png"); } }
		public static Texture SelectAll { get { return CSTextureLoader.GetIconTexture("SelectAll.png"); } }
		public static Texture SelectNone { get { return CSTextureLoader.GetIconTexture("SelectNone.png"); } }
		public static Texture Show { get { return CSTextureLoader.GetIconTexture("Show.png"); } }
		public static Texture Support { get { return CSTextureLoader.GetIconTexture("Support.png"); } }
	}

	internal class CSImages
	{
		public static Texture Logo { get { return CSTextureLoader.GetTexture("Logo.png"); } }
	}

	internal class CSEditorTextures
	{
		public static Texture ErrorSmallIcon { get { return Find("console.erroricon.sml"); } }
		public static Texture ErrorIcon { get { return Find("console.erroricon"); } }
		public static Texture FolderIcon { get { return Find("Folder Icon"); } }
		public static Texture InfoSmallIcon { get { return Find("console.infoicon.sml"); } }
		public static Texture InfoIcon { get { return Find("console.infoicon"); } }
		public static Texture PrefabIcon { get { return Find("PrefabNormal Icon"); } }
		public static Texture SceneIcon { get { return Find("SceneAsset Icon"); } }
		public static Texture ScriptIcon { get { return Find("cs Script Icon"); } }
		public static Texture WarnSmallIcon { get { return Find("console.warnicon.sml"); } }
		public static Texture WarnIcon { get { return Find("console.warnicon"); } }
		

		private static Texture Find(string name)
		{
			Texture result = EditorGUIUtility.FindTexture(name);

			if (result == null)
			{
				Debug.LogWarning(Maintainer.LOG_PREFIX + "Can't find built-in texture " + name + "! Please report it to " + Maintainer.SUPPORT_EMAIL);
			}

			return result;
		}
	}
}
#endif