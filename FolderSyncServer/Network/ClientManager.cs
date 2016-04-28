using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using FolderSyncServer;

namespace FolderSyncServer.Network
{
	class ClientManager
	{
		private Socket serverSocket = null;
		private List<EndPoint> clientList = new List<EndPoint>();
		private List<Tuple<EndPoint, byte[]>> dataList = new List<Tuple<EndPoint, byte[]>>();
		private byte[] _buffer = new byte[1025 * 500];
		private int _port;

		public List<Tuple<EndPoint, byte[]>> DataList
		{
			private set { this.dataList = value; }
			get { return (this.dataList); }
		}

		public ClientManager(int port)
		{
			this._port = port;
		}

		public void Start()
		{
			this.serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			//this.serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			this.serverSocket.Bind(new IPEndPoint(IPAddress.Parse("127.0.0.1"), this._port));

			EndPoint newClientEp = new IPEndPoint(IPAddress.Any, 0);
			this.serverSocket.BeginReceiveFrom(this._buffer, 0, this._buffer.Length, SocketFlags.None, ref newClientEp, DoReceiveFrom, newClientEp);
		}

		private void DoReceiveFrom(IAsyncResult iar)
		{
			var welcome = "Клиент успешно подключился!";
			byte[] data = Encoding.UTF8.GetBytes(welcome);
			this.serverSocket.Send(data, SocketFlags.None);
			try
			{
				EndPoint clientEP = new IPEndPoint(IPAddress.Any, 0);
				int dataLen = 0;
				//byte[] data = null;

				try
				{
					dataLen = this.serverSocket.EndReceiveFrom(iar, ref clientEP);
					data = new byte[dataLen];
					Array.Copy(this._buffer, data, dataLen);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}
				finally
				{
					EndPoint newClientEP = new IPEndPoint(IPAddress.Any, 0);
					this.serverSocket.BeginReceiveFrom(this._buffer, 0, this._buffer.Length, SocketFlags.None, ref newClientEP, DoReceiveFrom, newClientEP);
				}

				if (!this.clientList.Any(client => client.Equals(clientEP)))
					this.clientList.Add(clientEP);

				DataList.Add(Tuple.Create(clientEP, data));
			}
			catch (ObjectDisposedException)
			{
			}
		}

		public void SendTo(byte[] data, EndPoint clientEP)
		{
			try
			{
				this.serverSocket.SendTo(data, clientEP);
			}
			catch (System.Net.Sockets.SocketException)
			{
				this.clientList.Remove(clientEP);
			}
		}

		public void SendToAll(byte[] data)
		{
			foreach (var client in this.clientList)
			{
				this.SendTo(data, client);
			}
		}

		public void Stop()
		{
			this.serverSocket.Close();
			this.serverSocket = null;

			this.dataList.Clear();
			this.clientList.Clear();
		}
	}
}