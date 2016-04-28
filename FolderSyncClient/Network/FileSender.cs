using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using FolderSyncClient.Models;

namespace FolderSyncClient.Network
{
	class FileSender
	{
		private byte[] _buffer;
		private List<byte[]> _chunks;

		public FileSender()
		{
			_chunks = new List<byte[]>();
		}

		public bool SendFile(Socket socket, FileModel file)
		{
			_buffer = File.ReadAllBytes(file.path);
			_chunks = new List<byte[]>();

			for (int i = 0; i < _buffer.Length; i++)
			{
				var buffer = new byte[1024 * 50 + 8];
				Buffer.BlockCopy(BitConverter.GetBytes(i), 0, buffer, 0, 4);
				Buffer.BlockCopy(_buffer, 0, buffer, 4, 50 * 1024);
				_chunks.Add(buffer);
			}

			_buffer = new byte[1024];

			var operation = Encoding.UTF8.GetBytes("Receive");
			Buffer.BlockCopy(BitConverter.GetBytes(operation.Length), 0, _buffer, 0, 4);
			Buffer.BlockCopy(operation, 0, _buffer, 4, operation.Length);
			Buffer.BlockCopy(BitConverter.GetBytes(_chunks.Count), 0, _buffer, 4 + operation.Length, 4);
			//TODO Send info | Catch exception

			foreach (var chunk in _chunks)
			{
				try
				{
					//TODO Send chunk
				}
				catch (Exception)
				{
					return false;
				}
			}

			return true;
		}
	}
}
