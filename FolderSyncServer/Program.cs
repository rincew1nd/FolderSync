using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FolderSyncServer.Network;

namespace FolderSyncServer
{
    class Program
    {
        static void Main(string[] args)
        {
			int recv;
			byte[] data = new byte[1024];
			IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 9050);
			Socket SrvSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			SrvSock.Bind(ipep);
			Console.WriteLine("Ожидаем соединения с клиентом...");
			IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
			EndPoint Remote = (EndPoint)(sender);
			
			recv = SrvSock.ReceiveFrom(data, ref Remote);
			
			Console.Write("Сообщение получено от {0}:", Remote.ToString());
			Console.WriteLine(Encoding.UTF8.GetString(data, 0, recv));
			
			string welcome = "Подключение к серверу успешно!";
			data = Encoding.UTF8.GetBytes(welcome);
			SrvSock.SendTo(data, data.Length, SocketFlags.None, Remote);
			
			while (true)
			{
				data = new byte[1024];
				recv = SrvSock.ReceiveFrom(data, ref Remote);
				string str = Encoding.UTF8.GetString(data, 0, recv);
				
				if (str == "exit") break;

				Console.WriteLine("Получили данные: " + str);
				
				data = Encoding.UTF8.GetBytes(str);
				SrvSock.SendTo(data, data.Length, SocketFlags.None, Remote);
			}
		}
	}
}
