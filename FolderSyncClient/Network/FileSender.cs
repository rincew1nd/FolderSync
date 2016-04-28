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

		public void SendFile(Socket socket, FileModel file)
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

			var fileSendRequest = $"Receive file {_chunks.Count}";
			socket.s
		}
	}
}
