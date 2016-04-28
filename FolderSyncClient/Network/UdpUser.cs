using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FolderSyncClient.Network
{
	class UdpUser
	{
		private UdpClient _client;

		/// <summary>
		/// IP this manager listens on.
		/// </summary>
		public string Host { get; protected set; }

		/// <summary>
		/// Port this manager listens on.
		/// </summary>
		public int Port { get; protected set; }

		/// <summary>
		/// Address this manager listens on.
		/// </summary>
		public string Address => $"{this.Host}:{this.Port}";

		public UdpUser(string host, int port)
		{
			Host = host;
			Port = port;
			_client = new UdpClient(host, port);
		}

		public void ConnectTo(string hostname, int port)
		{
			_client.Connect(hostname, port);
		}

		public void Send(string message)
		{
			var datagram = Encoding.ASCII.GetBytes(message);
			_client.Send(datagram, datagram.Length);
		}

		public string Receive()
		{
			while (true)
			{
				try
				{
					var result = _client.ReceiveAsync().Result;
					return Encoding.ASCII.GetString(result.Buffer, 0, result.Buffer.Length);
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				}
			}
		}

	}
}
