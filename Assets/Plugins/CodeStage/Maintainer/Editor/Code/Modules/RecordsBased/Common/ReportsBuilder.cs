#if UNITY_EDITOR

using System;
using System.Text;
using UnityEditorInternal;
using UnityEngine;

namespace CodeStage.Maintainer
{
	/// <summary>
	/// Allows to build reports for the Maintainer modules.
	/// </summary>
	public class ReportsBuilder
	{
		private static readonly StringBuilder reportStringBuilder = new StringBuilder();

		/// <summary>
		/// Iterates over array of specified records and generates report.
		/// </summary>
		/// <param name="module">Module name.</param>
		/// <param name="records">Array of records to iterate.</param>
		/// <param name="optionalHeader">Optional string to add as a report header.</param>
		/// <param name="optionalFooter">Optional string to add as a report footer.</param>
		/// <returns>Final report string.</returns>
		public static string GenerateReport<T>(string module, T[] records, string optionalHeader = null, string optionalFooter = null) where T:RecordBase
		{
			reportStringBuilder.Length = 0;

			string isPro = null;
			if (Application.HasProLicense())
			{
				isPro = " Professional";
			}

			reportStringBuilder.
				AppendLine("////////////////////////////////////////////////////////////////////////////////").
				Append("// ").Append(module).AppendLine(" report").
				Append("// ").AppendLine(Application.dataPath.Remove(Application.dataPath.LastIndexOf("/", StringComparison.Ordinal), 7)).
				AppendLine("////////////////////////////////////////////////////////////////////////////////").
				Append("// Maintainer ").AppendLine(Maintainer.VERSION).
				Append("// Unity ").Append(InternalEditorUtility.GetFullUnityVersion()).AppendLine(isPro).
				AppendLine("//").
				AppendLine("// Homepage: http://blog.codestage.ru/unity-plugins/maintainer").
				AppendLine("// Contacts: http://blog.codestage.ru/contacts").
				AppendLine("////////////////////////////////////////////////////////////////////////////////");

			if (records != null && records.Length > 0)
			{
				if (!string.IsNullOrEmpty(optionalHeader))
				{
					reportStringBuilder.AppendLine(optionalHeader);
				}

				foreach (T record in records)
				{
					reportStringBuilder.AppendLine("---").AppendLine(record.ToString(true));
				}

				if (!string.IsNullOrEmpty(optionalFooter))
				{
					reportStringBuilder.AppendLine("---").AppendLine(optionalFooter);
				}
			}
			else
			{
				reportStringBuilder.AppendLine("No records to report.");
			}
			return reportStringBuilder.ToString();
		}
	}
}

#endif