using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FolderSyncClient.Files;
using FolderSyncClient.Models;

namespace FolderSyncClient
{
	class FolderSyncClient
	{
		public static readonly FolderSyncClient Instance = new FolderSyncClient();

		//private Client _client;
		private FileWatcher _fileWatcher;
		public void Run()
		{
			_fileWatcher = new FileWatcher("E:\\test");
			//_client = new Client("127.0.0.1", 10432);
			_fileWatcher.OnFilesChanged += PrintAllChanges;
			Console.Read();
		}

		public void PrintAllChanges(List<FileModel> files)
		{
			foreach (var fileModel in files)
				Console.WriteLine($"Name {fileModel.name}, path {fileModel.path}, status {fileModel.status.ToString()}");
		}
	}
}
