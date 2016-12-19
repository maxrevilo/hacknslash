#if UNITY_EDITOR

using System;
using System.IO;
using System.Text;
using CodeStage.Maintainer.Settings;
using CodeStage.Maintainer.Tools;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CodeStage.Maintainer.Cleaner
{
	[Serializable]
	public class AssetRecord : CleanerRecord, IShowableRecord
	{
		public string path;
		public long size; // in bytes
		public string beautyPath;
		public string assetDatabasePath;
		public Type assetType;
		public bool isTexture;

		internal static AssetRecord Create(RecordType type, string path)
		{
			AssetRecord newRecord = new AssetRecord(type, path);

			if (newRecord.assetType != null)
			{
				return newRecord;
			}

			return null;
		}

		protected AssetRecord(RecordType type, string path) :base(type, RecordLocation.Asset)
		{
			this.path = path;

			int index = Application.dataPath.IndexOf("/Assets");

			if (!Path.IsPathRooted(path))
			{
				assetDatabasePath = path;
			}
			else
			{
				assetDatabasePath = path.Replace('\\', '/').Substring(index + 1);
			}

			beautyPath = assetDatabasePath.Substring(7);

			Object asset = AssetDatabase.LoadMainAssetAtPath(assetDatabasePath);

			if (asset != null)
			{
				assetType = asset.GetType();

				if (asset is Texture)
				{
					isTexture = true;
				}

				if (type == RecordType.UnusedAsset)
				{
					size = new FileInfo(path).Length;
				}
			}
			else
			{
				Debug.LogError(Maintainer.LOG_PREFIX + ProjectCleaner.MODULE_NAME + " can't find asset at path:\n" + assetDatabasePath + "\nPlease report it to " + Maintainer.SUPPORT_EMAIL);
			}
		}

		protected override void ConstructCompactLine(StringBuilder text)
		{
			text.Append(beautyPath);
		}

		protected override void ConstructHeader(StringBuilder header)
		{
			base.ConstructHeader(header);

			if (type == RecordType.UnusedAsset)
			{
				header.Append(assetType.Name);
			}
		}

		protected override void ConstructBody(StringBuilder text)
		{
			text.Append("<b>Path:</b> ").Append(beautyPath);
			if (size > 0)
			{
				text.AppendLine().Append("<b>Size:</b> ").Append(CSEditorTools.FormatBytes(size));
			}
			if (type == RecordType.UnusedAsset)
			{
				text.AppendLine().Append("<b>Full Type:</b> ").Append(assetType.FullName);
			}
		}

		protected override bool PerformClean()
		{
			bool result;

			if (MaintainerSettings.Cleaner.useTrashBin)
			{
				result = AssetDatabase.MoveAssetToTrash(assetDatabasePath);
			}
			else
			{
				switch (type)
				{
					case RecordType.EmptyFolder:
						{
							if (Directory.Exists(path))
							{
								Directory.Delete(path, true);
							}
							break;
						}
					case RecordType.UnusedAsset:
						{
							if (File.Exists(path))
							{
								File.Delete(path);
							}
							break;
						}
					case RecordType.Error:
						break;
					case RecordType.Other:
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}

				// removes corresponding .meta files
				AssetDatabase.DeleteAsset(assetDatabasePath);
				result = !(Directory.Exists(path) || File.Exists(path));
			}
				
			if (!result)
			{
				Debug.LogWarning(Maintainer.LOG_PREFIX + ProjectCleaner.MODULE_NAME + " can't clean asset: " + beautyPath);
			}
			else
			{
				string directory = Path.GetDirectoryName(path);
				if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory))
				{
					string[] filesInDir = Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly);

					if (filesInDir.Length == 0)
					{
						Create(RecordType.EmptyFolder, directory).Clean();
					}
				}
			}

			return result;
		}

		public void Show()
		{
			Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetDatabasePath);
		}
	}
}

#endif