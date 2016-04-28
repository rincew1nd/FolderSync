using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
			var path = "E:\\test";

			// Init of main things
			_fileWatcher = new FileWatcher(path);
			_folderChanger = new FolderChanger(path);
			_updClient = new UdpUser("127.0.0.1", 2001);

			// Nonify FileChanger about changed files in folder
			_fileWatcher.OnFilesChanged += _folderChanger.CheckFilesInQuery;
			_folderChanger.OnFileSend += _updClient.Send;

			{
				byte[] data = new byte[1024];
				string input, stringData;

				UdpClient server = new UdpClient("127.0.0.1", 9050);
				IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
				
				string welcome = "Клиент успешно подключился!";
				data = Encoding.UTF8.GetBytes(welcome);
				server.Send(data, data.Length);
				
				data = server.Receive(ref sender);
				
				Console.Write("Сообщение принято от {0}:", sender.ToString());
				stringData = Encoding.UTF8.GetString(data, 0, data.Length);
				Console.WriteLine(stringData);
				
				while (true)
				{
					data = new byte[1024];
					Console.Write("\r\n>");
					input = Console.ReadLine();
					
					data = Encoding.UTF8.GetBytes(input);
					server.Send(data, data.Length);
					
					if (input == "exit") break;

					data = server.Receive(ref sender);
					stringData = Encoding.UTF8.GetString(data, 0, data.Length);
					Console.Write("<");
					Console.WriteLine(stringData);
				}
				Console.WriteLine("Остановка клиента...");
				server.Close();
			}

			Console.Read();
		}

		public void PrintAllChanges(List<FileModel> files)
		{
			foreach (var fileModel in files)
				Console.WriteLine($"Name {fileModel.name}, path {fileModel.path}, status {fileModel.status.ToString()}");
		}
	}
}
