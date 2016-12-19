#if UNITY_EDITOR
using System;
using CodeStage.Maintainer.Cleaner;
using CodeStage.Maintainer.Issues;

namespace CodeStage.Maintainer
{
	internal class RecordsSortings
	{
		internal static Func<CleanerRecord, string>				cleanerRecordByPath = record => record is AssetRecord ? ((AssetRecord)record).path : null;
		internal static Func<CleanerRecord, long>				cleanerRecordBySize = record => record is AssetRecord ? ((AssetRecord)record).size : 0;
		internal static Func<CleanerRecord, Cleaner.RecordType> cleanerRecordByType = record => record.type;
		internal static Func<CleanerRecord, string>				cleanerRecordByAssetType = record => record is AssetRecord ? ((AssetRecord)record).assetType.FullName : null;

		internal static Func<IssueRecord, string>				issueRecordByPath = record => record is GameObjectIssueRecord ? ((GameObjectIssueRecord)record).path : null;
		internal static Func<IssueRecord, Issues.RecordType>	issueRecordByType = record => record.type;
		internal static Func<IssueRecord, RecordSeverity>		issueRecordBySeverity = record => record.severity;
	}
}
#endif