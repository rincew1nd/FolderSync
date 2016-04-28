using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TestProject.http;

namespace TestProject
{
	class Program
	{
		static void Main(string[] args)
		{
			new Server(80);
		}
	}

	class TestSocket
	{
		public void Test()
		{
			EndPoint endPoint = new IPEndPoint(IPAddress.Any, 80);
			var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.Connected()
		}
	}
}