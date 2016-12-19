#if UNITY_EDITOR

using System;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI.Filters
{
	internal abstract class StringFiltersTab : TabBase
	{
		internal delegate void SaveFiltersCallback(string[] filters);

		protected string[] filters;
		protected SaveFiltersCallback saveFiltersCallback;

		protected bool didFocus;

		private readonly CSLayout layout = new CSLayout();
		private string newItemText = "";

		protected StringFiltersTab(FilterType filterType, string[] filters, SaveFiltersCallback saveFiltersCallback):base(filterType)
		{
			this.filters = filters;
			this.saveFiltersCallback = saveFiltersCallback;
		}

		internal override void Show(FiltersWindow hostingWindow)
		{
			base.Show(hostingWindow);

			newItemText = "";
			didFocus = false;
		}

		protected override void DrawTabContents()
		{
			DrawTabHeader();

			GUILayout.Space(5);
			UIHelpers.Separator();
			GUILayout.Space(5);

			DrawAddItemSection();

			GUILayout.Space(5);
			UIHelpers.Separator();
			GUILayout.Space(5);

			DrawFiltersList();
		}

		protected virtual void DrawAddItemSection()
		{
			EditorGUILayout.LabelField(GetAddNewLabel());
			using (layout.Horizontal())
			{
				GUILayout.Space(6);
				GUI.SetNextControlName("AddButton");

				bool flag = currentEvent.isKey && Event.current.type == EventType.KeyDown && (currentEvent.keyCode == KeyCode.Return || currentEvent.keyCode == KeyCode.KeypadEnter);
				if (UIHelpers.IconButton(CSIcons.Plus, "Adds custom filter to the list.") || flag)
				{
					if (string.IsNullOrEmpty(newItemText))
					{
						window.ShowNotification(new GUIContent("You can't add an empty filter!"));
					}
					else if (newItemText.IndexOf('*') != -1)
					{
						window.ShowNotification(new GUIContent("Masks are not supported!"));
					}
					else
					{
						if (CheckNewItem(ref newItemText))
						{
							if (TryAddNewItemToFilters(newItemText))
							{
								SaveChanges();
								newItemText = "";
								GUI.FocusControl("AddButton");
								didFocus = false;
							}
							else
							{
								window.ShowNotification(new GUIContent("This filter already exists in the list!"));
							}
						}
					}
				}

				if (flag)
				{
					currentEvent.Use();
					currentEvent.Use();
				}

				GUILayout.Space(5);

				GUI.SetNextControlName("filtersTxt");
				newItemText = EditorGUILayout.TextField(newItemText);
				if (!didFocus)
				{
					didFocus = true;
					EditorGUI.FocusTextInControl("filtersTxt");
				}
			}
		}

		protected virtual void DrawFiltersList()
		{
			if (filters == null) return;

			scrollPosition = GUILayout.BeginScrollView(scrollPosition);

			foreach (string filter in filters)
			{
				using (layout.Horizontal(UIHelpers.panelWithBackground))
				{
					if (UIHelpers.IconButton(CSIcons.Minus, "Removes filter from the list."))
					//if (GUILayout.Button("<b><color=#FF4040>X</color></b>", UIHelpers.compactButton, GUILayout.ExpandWidth(false)))
					{
						ArrayUtility.Remove(ref filters, filter);
						SaveChanges();
					}
					GUILayout.Space(5);
					if (GUILayout.Button(filter, UIHelpers.richLabel))
					{
						newItemText = filter;
						GUI.FocusControl("AddButton");
						didFocus = false;
					}
				}
			}
			GUILayout.EndScrollView();

			if (filters.Length > 0)
			{
				if (UIHelpers.ImageButton("Clear All " + caption.text, "Removes all added filters from the list.", CSIcons.Clear))
				{
					string cleanCaption = Regex.Replace(caption.text, @"<[^>]*>", string.Empty);
					if (EditorUtility.DisplayDialog("Clearing the " + cleanCaption + " list",
						"Are you sure you wish to clear all the filters in the " + cleanCaption + " list?",
						"Yes", "No"))
					{
						Array.Resize(ref filters, 0);
						SaveChanges();
					}
				}
			}
		}

		protected bool TryAddNewItemToFilters(string newItem)
		{
			if (Array.IndexOf(filters, newItem) != -1) return false;
			ArrayUtility.Add(ref filters, newItem);
			return true;
		}

		protected void SaveChanges()
		{
			if (saveFiltersCallback != null)
			{
				saveFiltersCallback(filters);
			}
		}

		protected virtual string GetAddNewLabel()
		{
			return "Add new filter to the list:";
		}

		protected abstract void DrawTabHeader();

		protected abstract bool CheckNewItem(ref string newItem);
	}
}

#endif