using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace FolderSyncServer
{
	class FileReceiver
	{

		public FileReceiver()
		{
			
		}

		public void Receive(EndPoint endPoint, byte[] info)
		{
			//TODO open connection

			var stringLength = BitConverter.ToInt32(info, 0);
			var operation = Encoding.UTF8.GetString(info, 4, stringLength);

			switch (operation)
			{
				case "Recieve":
					ReceiveFile(endPoint, BitConverter.ToInt32(info, 4 + stringLength));
					break;
				case "Test":
					Console.WriteLine("Test");
					break;
				default:
					Console.WriteLine("Unknown operation");
					break;
			}
		}

		public byte[] ReceiveFile(EndPoint endPoint, int chunksCount)
		{
			var buffer = new byte[1];//TODO temprorary variable while no network code
			var chunks = new Dictionary<int, byte[]>();

			while (true)
			{
				//TODO Receive chunk
				var chunk = new byte[1024*50];
				var chunkNumber = BitConverter.ToInt32(buffer, 0);
				Buffer.BlockCopy(buffer, 4, chunk, 0, 1024*50);

				chunks.Add(chunkNumber, chunk);
			}

			//TODO Send missing chunks
			//for (int i = 0; i < chunksCount; i++)
			//	if (!chunks.ContainsKey(i))

			return chunks.Values.SelectMany(z => z).ToArray();
		}
	}
}
