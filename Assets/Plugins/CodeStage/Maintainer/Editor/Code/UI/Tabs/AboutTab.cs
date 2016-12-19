#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class AboutTab
	{
		private const string UAS_LINK = "https://www.assetstore.unity3d.com/#!/content/32199";
		private const string UAS_PROFILE_LINK = "https://www.assetstore.unity3d.com/#!/search/page=1/sortby=popularity/query=publisher:3918";
		private const string HOMEPAGE = "http://blog.codestage.ru/unity-plugins/maintainer/";
		private const string SUPPORT_LINK = "http://blog.codestage.ru/contacts/";
		private const string CHANGELOG_LINK = "http://codestage.ru/unity/maintainer/changelog.txt";

		private readonly CSLayout layout = new CSLayout();
		private GUIContent caption;
		internal GUIContent Caption 
		{
			get
			{
				if (caption == null)
				{
					caption = new GUIContent("About", CSIcons.About);
				}
				return caption;
			}
		}

		internal void Draw(MaintainerWindow parentWindow)
		{
			using (layout.Horizontal())
			{
				/* logo */

				using (layout.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
				{
					GUILayout.FlexibleSpace();

					using (layout.Horizontal())
					{
						GUILayout.FlexibleSpace();

						Texture logo = CSImages.Logo;
						if (logo != null)
						{
							Rect logoRect = EditorGUILayout.GetControlRect(GUILayout.Width(logo.width), GUILayout.Height(logo.height));
							GUI.DrawTexture(logoRect, logo);
							GUILayout.Space(5);
						}

						GUILayout.FlexibleSpace();
					}

					GUILayout.FlexibleSpace();
				}

				/* buttons and stuff */

				using (layout.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
				{
					GUILayout.Space(10);
					GUILayout.Label("<size=18>Maintainer v.<b>" + Maintainer.VERSION + "</b></size>", UIHelpers.centeredLabel);
					GUILayout.Space(5);
					GUILayout.Label("Developed by Dmitriy Yukhanov\n" +
									"Logo by Daniele Giardini\n" +
									"Icons by Google, Austin Andrews, Cody", UIHelpers.centeredLabel);
					GUILayout.Space(10);
					UIHelpers.Separator();
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("Homepage", CSIcons.Home))
					{
						Application.OpenURL(HOMEPAGE);
					}
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("Support contacts", CSIcons.Support))
					{
						Application.OpenURL(SUPPORT_LINK);
					}
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("Full changelog (online)", CSIcons.Log))
					{
						Application.OpenURL(CHANGELOG_LINK);
					}
					GUILayout.Space(5);

					//GUILayout.Space(10);
					//GUILayout.Label("Asset Store links", UIHelpers.centeredLabel);
					UIHelpers.Separator();
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("Plugin at Unity Asset Store", CSIcons.AssetStore))
					{
						Application.OpenURL(UAS_LINK);
					}
					GUILayout.Label("It's really important to know your opinion,\n rates & reviews are <b>greatly appreciated!</b>", UIHelpers.centeredLabel);
					GUILayout.Space(5);
					if (UIHelpers.ImageButton("My profile at Unity Asset Store", CSIcons.Publisher))
					{
						Application.OpenURL(UAS_PROFILE_LINK);
					}
					GUILayout.Label("Check all my plugins!", UIHelpers.centeredLabel);
				}
			}
		}
	}
}

#endif