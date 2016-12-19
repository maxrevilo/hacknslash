#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal class UIHelpers
	{
		// ----------------------------------------------------------------------------
		// static tooling
		// ----------------------------------------------------------------------------

		public static GUIStyle richLabel;
		public static GUIStyle iconLabel;
		public static GUIStyle richButton;
		public static GUIStyle richButtonMid;
		public static GUIStyle richButtonLeft;
		public static GUIStyle richButtonRight;
		public static GUIStyle compactButton;
		public static GUIStyle recordButton;
		public static GUIStyle iconButton;
		public static GUIStyle richWordWrapLabel;
		public static GUIStyle richFoldout;
		public static GUIStyle centeredLabel;
		public static GUIStyle line;
		public static GUIStyle panelWithBackground;

		public static void SetupStyles()
		{
			if (richLabel != null) return;

			richLabel = new GUIStyle(GUI.skin.label);
			richLabel.richText = true;

			richButton = new GUIStyle(GUI.skin.button);
			richButton.richText = true;
			richButton.name = "csMaintainerRichButton";

			richButtonLeft = new GUIStyle(GUI.skin.FindStyle(GUI.skin.button.name + "left"));
			richButtonLeft.richText = true;
			richButtonLeft.name = "csMaintainerRichButtonLeft";

			richButtonMid = new GUIStyle(GUI.skin.FindStyle(GUI.skin.button.name + "mid"));
			richButtonMid.richText = true;
			richButtonMid.name = "csMaintainerRichButtonMid";

			richButtonRight = new GUIStyle(GUI.skin.FindStyle(GUI.skin.button.name + "right"));
			richButtonRight.richText = true;
			richButtonRight.name = "csMaintainerRichButtonRight";

			GUIStyle[] customStyles = GUI.skin.customStyles;

			bool alreadyExists = false;

			for (int i = 0; i < customStyles.Length; i++)
			{
				if (customStyles[i].name == "csMaintainerRichButtonLeft")
				{
					alreadyExists = true;
					break;
				}
			}

			if (!alreadyExists)
			{
				ArrayUtility.AddRange(ref customStyles, new[] { richButtonLeft, richButtonMid, richButtonRight });
			}

			GUI.skin.customStyles = customStyles;

			iconLabel = new GUIStyle(GUI.skin.label);
			iconLabel.overflow = new RectOffset(0, 0, 0, 0);
			iconLabel.padding = new RectOffset(0, 0, 0, 0);
			//iconLabel.margin = new RectOffset(0, 0, 0, 0);
			iconLabel.fixedHeight = 16;
			iconLabel.fixedWidth = 22;

			compactButton = new GUIStyle(GUI.skin.button);
			compactButton.overflow = new RectOffset(0, 0, 0, 0);
			compactButton.padding = new RectOffset(6, 6, 1, 3);
			compactButton.margin = new RectOffset(2, 2, 3, 2);
			compactButton.richText = true;
			compactButton.fixedHeight = 21;
			compactButton.fontSize = 12;

			recordButton = new GUIStyle(compactButton);
			recordButton.fixedWidth = 80;

			iconButton = new GUIStyle(compactButton);
			iconButton.padding = new RectOffset(0, 0, EditorGUIUtility.isProSkin  ? -5 : - 4, -2);
			iconButton.overflow = EditorGUIUtility.isProSkin ? new RectOffset(1, 1, 1, 1) : new RectOffset(0, 0, 2, 1);
			iconButton.fixedHeight = 18;
			iconButton.fixedWidth = 22;

			richWordWrapLabel = new GUIStyle(richLabel);
			richWordWrapLabel.wordWrap = true;

			richFoldout = new GUIStyle(EditorStyles.foldout);
			richFoldout.active = richFoldout.focused = richFoldout.normal;
			richFoldout.onActive = richFoldout.onFocused = richFoldout.onNormal;
			richFoldout.richText = true;

			centeredLabel = new GUIStyle(richLabel);
			centeredLabel.alignment = TextAnchor.MiddleCenter;

			line = new GUIStyle(GUI.skin.box);
			line.border.top = line.border.bottom = 1;
			line.margin.top = line.margin.bottom = 1;
			line.padding.top = line.padding.bottom = 1;

			panelWithBackground = new GUIStyle(GUI.skin.box);
			panelWithBackground.padding = new RectOffset();

			//toolbarRich = new GUIStyle(EditorStyles.toolbar);
		}

		public static void Separator()
		{
			GUILayout.Box(GUIContent.none, line, GUILayout.ExpandWidth(true), GUILayout.Height(1f));
		}

		public static void VerticalSeparator()
		{
			GUILayout.Box(GUIContent.none, line, GUILayout.Width(1f), GUILayout.ExpandHeight(true));
		}

		public static void Indent(int level = 5, int topPadding = 2)
		{
			GUILayout.Space(topPadding);
			EditorGUILayout.BeginHorizontal();
			GUILayout.Space(level * 4);
			EditorGUILayout.BeginVertical();
		}

		public static void UnIndent(int bottomPadding = 5)
		{
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
			GUILayout.Space(bottomPadding);
		}

		public static void Icon(Texture icon, string hint, params GUILayoutOption[] options)
		{
			GUILayout.Label(new GUIContent(icon, hint), iconLabel, options);
		}

		public static bool ImageButton(string label, Texture image, params GUILayoutOption[] options)
		{
			return ImageButton(label, null, image, options);
		}

		public static bool ImageButton(string label, string hint, Texture image, params GUILayoutOption[] options)
		{
			return ImageButton(label, hint, image, compactButton, options);
		}

		public static bool ImageButton(string label, string hint, Texture image, GUIStyle style, params GUILayoutOption[] options)
		{
			GUIContent content = new GUIContent();

			if (!string.IsNullOrEmpty(label))
			{
				content.text = label;
			}

			if (!string.IsNullOrEmpty(hint))
			{
				content.tooltip = hint;
			}

			content.image = image;
			if (!string.IsNullOrEmpty(label))
			{
				content.text = " " + label;
			}
			
			return GUILayout.Button(content, style, options);
		}

		public static bool IconButton(Texture icon, params GUILayoutOption[] options)
		{
			return IconButton(icon, null, options);
		}

		public static bool IconButton(Texture icon, string hint, params GUILayoutOption[] options)
		{
			return ImageButton(null, hint, icon, iconButton, options);
		}

		public static bool RecordButton(RecordBase record, string hint, Texture image, params GUILayoutOption[] options)
		{
			return RecordButton(record, null, hint, image, options);
		}

		public static bool RecordButton(RecordBase record, string label, string hint, Texture image, params GUILayoutOption[] options)
		{
			return record.compactMode ? IconButton(image, hint) : ImageButton(label, hint, image, recordButton, options);
		}

		public static bool ToggleFoldout(ref bool toggle, ref bool foldout, GUIContent caption)
		{
			GUILayout.BeginHorizontal();
			toggle = EditorGUILayout.ToggleLeft("", toggle, GUILayout.Width(12));
			foldout = EditorGUI.Foldout(EditorGUILayout.GetControlRect(), foldout, caption, true, richFoldout);
			GUILayout.EndHorizontal();

			return toggle;
		}
	}
}

#endif