#if UNITY_EDITOR
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Issues;
using CodeStage.Maintainer.UI;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer
{
	public class Maintainer
	{
		public const string LOG_PREFIX = "<b>[Maintainer]</b> ";
		public const string VERSION = "1.3.0.0";
		public const string SUPPORT_EMAIL = "focus@codestage.ru";

		internal const string DATA_LOSS_WARNING = "Make sure you've made a backup of your project before proceeding.\nAuthor is not responsible for any data loss due to use of the Maintainer!";

		private static string directory;

		public static string Directory
		{ 
			get
			{
				if (!string.IsNullOrEmpty(directory)) return directory;

				directory = MaintainerMarker.GetAssetPath();

				if (!string.IsNullOrEmpty(directory))
				{
					if (directory.IndexOf("Editor/Code/MaintainerMarker.cs") >= 0)
					{
						directory = directory.Replace("/Code/MaintainerMarker.cs", "");
					}
					else
					{
						directory = null;
						Debug.LogError(LOG_PREFIX + "Looks like Maintainer is placed in project incorrectly!\nPlease, contact me for support: " + SUPPORT_EMAIL);
					}
				}
				else
				{
					directory = null;
					Debug.LogError(LOG_PREFIX + "Can't locate the Maintainer directory!\nPlease, report to " + SUPPORT_EMAIL);
				}
				return directory;
			}
		}

		public static string ConstructError(string errorText)
		{
			return LOG_PREFIX + errorText + " Please report to " + SUPPORT_EMAIL;
		}

		/*[MenuItem("Assets/Code Stage/Maintainer/Find Issues %#&f", false, 100)]
		private static void FindAllIssues()
		{
			IssuesFinder.StartSearch(true);
		}*/

		[MenuItem("Tools/Code Stage/Maintainer/Show %#&`", false, 900)]
		private static void ShowWindow()
		{
			MaintainerWindow.Create();
		}

		[MenuItem("Tools/Code Stage/Maintainer/About", false, 901)]
		private static void ShowAbout()
		{
			MaintainerWindow.ShowAbout();
		}

		[MenuItem("Tools/Code Stage/Maintainer/Find Issues %#&f", false, 1000)]
		private static void FindAllIssues()
		{
			IssuesFinder.StartSearch(true);
		}

		[MenuItem("Tools/Code Stage/Maintainer/Find Garbage %#&g", false, 1001)]
		private static void FindAllGarbage()
		{
			ProjectCleaner.StartSearch(true);
		}
	}
}
#endif