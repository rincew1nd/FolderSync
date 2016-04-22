using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderSyncClient.Files;
using FolderSyncClient.Models;
using FolderSyncClient.Network;

namespace FolderSyncClient
{
	class FolderSyncClient
	{
		public static readonly FolderSyncClient Instance = new FolderSyncClient();
		
		private FileWatcher _fileWatcher;
		private FolderChanger _folderChanger;
		private UdpUser _updClient;

		public void Run()
		{
			// Folder to monitor
			string path = "E:\\test";

			// Init of main things
			_fileWatcher = new FileWatcher(path);
			_folderChanger = new FolderChanger(path);
			_updClient = new UdpUser("127.0.0.1", 2001);

			// Nonify FileChanger about changed files in folder
			_fileWatcher.OnFilesChanged += _folderChanger.CheckFilesInQuery;
			_folderChanger.OnFileSend += _updClient.Send;

			Console.Read();
		}

		public void PrintAllChanges(List<FileModel> files)
		{
			foreach (var fileModel in files)
				Console.WriteLine($"Name {fileModel.name}, path {fileModel.path}, status {fileModel.status.ToString()}");
		}
	}
}
