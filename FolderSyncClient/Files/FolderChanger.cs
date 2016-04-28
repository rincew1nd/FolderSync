using System;
using System.Collections.Generic;
using System.IO;
using FolderSyncClient.Models;
using System.Web.Script.Serialization;

namespace FolderSyncClient
{
	internal class FolderChanger
	{
		// Folder that changer manipulate with
		private string _folderToChange;

		// Event + delegate for notify client to send info about changes
		public delegate void FileWatherEventHandler(List<FileModel> changedFiles);
		public event FileWatherEventHandler OnFileSend;

		public FolderChanger(string path)
		{
			_folderToChange = path;

		}

		public void CheckFilesInQuery(List<FileModel> files)
		{
			foreach (var fileModel in files)
			{
				Console.WriteLine($"{fileModel.name} -- {fileModel.status}");
			}
			//TODO check query for already added files
			//OnFileSend.Invoke(files);
		}
	}
}