#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	// Original idea (c) Yuri Tikhomirov at Cerevrum Inc.
	// https://gist.github.com/yuri-tikhomirov/dd09c244cb75041f180640fc71722078

	internal class CSLayout : IDisposable
	{
		private readonly Stack<LayoutMode> stack = new Stack<LayoutMode>(32);

		public IDisposable Area(Rect rect)
		{
			GUILayout.BeginArea(rect);
			stack.Push(LayoutMode.Area);
			return this;
		}

		public IDisposable ScrollView(ref Vector2 scrollPosition)
		{
			scrollPosition = GUILayout.BeginScrollView(scrollPosition);
			stack.Push(LayoutMode.Scrollview);
			return this;
		}

		public CSLayout Horizontal(params GUILayoutOption[] options)
		{
			return Horizontal(GUIStyle.none, options);
		}

		public CSLayout Horizontal(GUIStyle style, params GUILayoutOption[] options)
		{
			GUILayout.BeginHorizontal(style, options);
			stack.Push(LayoutMode.Horizontal);
			return this;
		}

		public CSLayout Vertical(params GUILayoutOption[] options)
		{
			return Vertical(GUIStyle.none, options);
		}

		public CSLayout Vertical(GUIStyle style, params GUILayoutOption[] options)
		{
			GUILayout.BeginVertical(style, options);
			stack.Push(LayoutMode.Vertical);
			return this;
		}

		public void Dispose()
		{
			var scope = stack.Pop();
			switch (scope)
			{
				case LayoutMode.Area:
					GUILayout.EndArea();
					break;
				case LayoutMode.Vertical:
					GUILayout.EndVertical();
					break;
				case LayoutMode.Horizontal:
					GUILayout.EndHorizontal();
					break;
				case LayoutMode.Scrollview:
					GUILayout.EndScrollView();
					break;
			}
		}

		private enum LayoutMode : byte
		{
			Area,
			Vertical,
			Horizontal,
			Scrollview
		}
	}
}

#endif