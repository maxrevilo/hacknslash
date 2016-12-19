#if UNITY_EDITOR

using System;
using System.IO;
using System.Linq;
using CodeStage.Maintainer.Tools;
using UnityEditor;
using UnityEngine;

namespace CodeStage.Maintainer.UI
{
	internal abstract class RecordsTab<T> where T : RecordBase
	{
		protected const int RECORDS_PER_PAGE = 100;
		protected readonly CSLayout layout = new CSLayout();

		protected MaintainerWindow window;
		protected Vector2 searchSectionScrollPosition;
		protected Vector2 settingsSectionScrollPosition;
		protected int recordsCurrentPage;
		protected int recordsTotalPages;
		protected int recordToDeleteIndex;
		protected T[] filteredRecords;
		protected T[] records;
		private IShowableRecord gotoRecord;

		/* virtual methods */

		internal virtual void Refresh()
		{
			records = null;
			filteredRecords = null;
			recordsCurrentPage = 0;
			searchSectionScrollPosition = Vector2.zero;
			settingsSectionScrollPosition = Vector2.zero;
		}

		internal virtual void Draw(MaintainerWindow parentWindow)
		{
			if (records == null)
			{
				records = LoadLastRecords();
				ApplySorting();
				recordsTotalPages = (int)Math.Ceiling((double)filteredRecords.Length / RECORDS_PER_PAGE);

				PerformPostRefreshActions();
			}

			window = parentWindow;

			using (layout.Horizontal())
			{
				DrawLeftSection();
				DrawRightSection();
			}

			if (gotoRecord != null)
			{
				gotoRecord.Show();
				gotoRecord = null;
			}
		}

		protected virtual T[] GetRecords()
		{
			return records;
		}

		protected virtual void ClearRecords()
		{
			records = null;
			filteredRecords = null;
		}

		protected virtual void DeleteRecord()
		{
			T record = filteredRecords[recordToDeleteIndex];
			records = CSArrayTools.RemoveAt(records, Array.IndexOf(records, record));
			ApplySorting();

			if (filteredRecords.Length > 0)
			{
				recordsTotalPages = (int)Math.Ceiling((double)filteredRecords.Length / RECORDS_PER_PAGE);
			}
			else
			{
				recordsTotalPages = 1;
			}

			if (recordsCurrentPage + 1 > recordsTotalPages) recordsCurrentPage = recordsTotalPages - 1;

			SaveSearchResults();
			window.Repaint();

		}

		protected virtual void DrawLeftSection()
		{
			using (layout.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.Width(240)))
			{
				DrawSettings();
				DrawLeftExtra();
			}
		}

		protected virtual void DrawSettings()
		{
			GUILayout.Space(10);
			GUILayout.Label("<size=14><b>Settings</b></size>", UIHelpers.centeredLabel);
			GUILayout.Space(10);

			DrawSettingsBody();
		}

		protected virtual void DrawRightSection()
		{
			using (layout.Vertical(UIHelpers.panelWithBackground, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true)))
			{
				GUILayout.Space(10);
				DrawSearch();
			}
		}

		protected virtual void DrawSearch()
		{
			DrawSearchTop();

			GUILayout.Space(5);

			if (filteredRecords == null || filteredRecords.Length <= 0) return;

			ShowCollectionPages();

			GUILayout.Space(5);

			DrawSearchBottom();
		}

		protected virtual void DrawSearchBottom()
		{
			using (layout.Horizontal())
			{
				DrawSelectAllButton();
				DrawSelectNoneButton();
				DrawExpandAllButton();
				DrawCollapseAllButton();
			}

			using (layout.Horizontal())
			{
				DrawCopyReportButton();
				DrawExportReportButton();
				DrawClearResultsButton();
			}
		}

		protected virtual void ShowCollectionPages()
		{
			int fromIssue = recordsCurrentPage * RECORDS_PER_PAGE;
			int toIssue = fromIssue + Math.Min(RECORDS_PER_PAGE, filteredRecords.Length - fromIssue);

			using (layout.Horizontal(UIHelpers.panelWithBackground))
			{
				GUILayout.Label(fromIssue + 1 + " - " + toIssue + " from " + filteredRecords.Length/* + " (" + records.Count + " total)"*/);
				GUILayout.FlexibleSpace();
				using (layout.Horizontal())
				{
					DrawPagesRightHeader();
				}
			}
			UIHelpers.Separator();

			searchSectionScrollPosition = GUILayout.BeginScrollView(searchSectionScrollPosition);
			for (int i = fromIssue; i < toIssue; i++)
			{
				DrawRecord(i);
			}
			GUILayout.EndScrollView();

			UIHelpers.Separator();

			if (recordsTotalPages <= 1) return;

			GUILayout.Space(5);
			using (layout.Horizontal())
			{
				GUILayout.FlexibleSpace();

				GUI.enabled = recordsCurrentPage > 0;
				if (UIHelpers.IconButton(CSIcons.DoubleArrowLeft))
				{
					window.RemoveNotification();
					recordsCurrentPage = 0;
					searchSectionScrollPosition = Vector2.zero;
				}
				if (UIHelpers.IconButton(CSIcons.ArrowLeft))
				{
					window.RemoveNotification();
					recordsCurrentPage--;
					searchSectionScrollPosition = Vector2.zero;
				}
				GUI.enabled = true;
				GUILayout.Label(recordsCurrentPage + 1 + " of " + recordsTotalPages, UIHelpers.centeredLabel);
				GUI.enabled = recordsCurrentPage < recordsTotalPages - 1;
				if (UIHelpers.IconButton(CSIcons.ArrowRight))
				{
					window.RemoveNotification();
					recordsCurrentPage++;
					searchSectionScrollPosition = Vector2.zero;
				}
				if (UIHelpers.IconButton(CSIcons.DoubleArrowRight))
				{
					window.RemoveNotification();
					recordsCurrentPage = recordsTotalPages - 1;
					searchSectionScrollPosition = Vector2.zero;
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();
			}
		}

		protected virtual void ApplySorting()
		{
			filteredRecords = records.ToArray();
		}

		protected virtual void DrawRecordCheckbox(RecordBase record)
		{
			EditorGUI.BeginChangeCheck();
			record.selected = EditorGUILayout.ToggleLeft(new GUIContent(""), record.selected, GUILayout.Width(12));
			if (EditorGUI.EndChangeCheck())
			{
				OnSelectionChanged();
			}
		}

		/* empty virtual methods */

		protected virtual void PerformPostRefreshActions() { }

		protected virtual void DrawPagesRightHeader() { }

		protected virtual void DrawLeftExtra() { }

		protected virtual string GetReportHeader() { return null; }

		protected virtual string GetReportFooter() { return null; }

		protected virtual string GetReportFileNamePart() { return ""; }

		protected virtual void AfterClearRecords() { }

		protected virtual void OnSelectionChanged() { }

		/* abstract methods */

		protected abstract T[] LoadLastRecords();
		protected abstract void DrawSettingsBody();
		protected abstract void DrawSearchTop();
		protected abstract void DrawRecord(int recordIndex);
		protected abstract void SaveSearchResults();
		protected abstract string GetModuleName();

		/* protected methods */

		protected void DrawShowButtonIfPossible(T record)
		{
			IShowableRecord showableIssueRecord = record as IShowableRecord;
			if (showableIssueRecord == null) return;

			string hintText;
			switch (record.location)
			{
				case RecordLocation.Unknown:
					hintText = "Oh, sorry, but looks like I have no clue about this record.";
					break;
				case RecordLocation.Scene:
					hintText = "Selects item in the scene. Opens scene with target item if necessary and highlights this scene in the Project Browser.";
					break;
				case RecordLocation.Asset:
					hintText = "Selects asset file in the Project Browser.";
					break;
				case RecordLocation.Prefab:
					hintText = "Selects Prefab file with item in the Project Browser.";
					break;
				case RecordLocation.BuildSettings:
					hintText = "Opens BuildSettings window.";
					break;
				case RecordLocation.TagsAndLayers:
					hintText = "Opens Tags and Layers in inspector.";
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			if (UIHelpers.RecordButton(record, "Show", hintText, CSIcons.Show))
			{
				gotoRecord = showableIssueRecord;
			}
		}

		protected void DrawCopyButton(T record)
		{
			if (UIHelpers.RecordButton(record, "Copy", "Copies record text to the clipboard.", CSIcons.Copy))
			{
				EditorGUIUtility.systemCopyBuffer = record.ToString(true);
				MaintainerWindow.ShowNotification("Record copied to clipboard!");
			}
		}

		protected void DrawExpandCollapseButton(RecordBase record)
		{
			Rect r = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(12));
			record.compactMode = !EditorGUI.Foldout(r, !record.compactMode, GUIContent.none, UIHelpers.richFoldout);
		}

		/* private methods */

		private void DrawSelectAllButton()
		{
			if (UIHelpers.ImageButton("Select all", CSIcons.SelectAll))
			{
				foreach (var record in filteredRecords)
				{
					record.selected = true;
				}

				OnSelectionChanged();
			}
		}

		private void DrawSelectNoneButton()
		{
			if (UIHelpers.ImageButton("Select none", CSIcons.SelectNone))
			{
				foreach (var record in filteredRecords)
				{
					record.selected = false;
				}

				OnSelectionChanged();
			}
		}

		private void DrawExpandAllButton()
		{
			if (UIHelpers.ImageButton("Expand all", CSIcons.Expand))
			{
				foreach (var record in filteredRecords)
				{
					record.compactMode = false;
				}
			}
		}

		private void DrawCollapseAllButton()
		{
			if (UIHelpers.ImageButton("Collapse all", CSIcons.Collapse))
			{
				foreach (var record in filteredRecords)
				{
					record.compactMode = true;
				}
			}
		}

		private void DrawCopyReportButton()
		{
			if (UIHelpers.ImageButton("Copy report to clipboard", CSIcons.Copy))
			{
				EditorGUIUtility.systemCopyBuffer = ReportsBuilder.GenerateReport(GetModuleName(), filteredRecords, GetReportHeader(), GetReportFooter());
				MaintainerWindow.ShowNotification("Report copied to clipboard!");
			}
		}

		private void DrawExportReportButton()
		{
			if (UIHelpers.ImageButton("Export report...", CSIcons.Export))
			{
				string filePath = EditorUtility.SaveFilePanel("Save " + GetModuleName() + " report", "", "Maintainer " + GetReportFileNamePart() + "Report.txt", "txt");
				if (!string.IsNullOrEmpty(filePath))
				{
					StreamWriter sr = File.CreateText(filePath);
					sr.Write(ReportsBuilder.GenerateReport(GetModuleName(), filteredRecords, GetReportHeader(), GetReportFooter()));
					sr.Close();
					MaintainerWindow.ShowNotification("Report saved!");
				}
			}
		}

		private void DrawClearResultsButton()
		{
			if (UIHelpers.ImageButton("Clear results", CSIcons.Clear))
			{
				ClearRecords();
				AfterClearRecords();
			}
		}
	}
}

#endif