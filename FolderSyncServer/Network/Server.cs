//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading;
//using FolderSync.Models;
//
//namespace FolderSyncSe
//{
//    class Server
//    {
//        private readonly TcpListener _tcpListener;
//        private List<ClientModel> _tcpClients;
//
//        public Server()
//        {
//            try
//            {
//                _tcpListener = new TcpListener(new IPEndPoint(IPAddress.Any, 10432));
//                _tcpListener.Start();
//                _tcpClients = new List<ClientModel>();
//                _tcpListener.BeginAcceptSocket(AcceptCallback, null);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex.Message);
//            }
//        }
//
//        private void AcceptCallback(IAsyncResult AR)
//        {
//            try
//            {
//                _tcpListener.BeginAcceptSocket(AcceptCallback, null);
//
//                var tcpClient = _tcpListener.EndAcceptTcpClient(AR);
//                var username = new BinaryReader(tcpClient.GetStream(), Encoding.Unicode).ReadString();
//	            Console.WriteLine($"{username} connected to server!");
//
//                _tcpClients.Add(
//                    new ClientModel() {
//                        _tcpClient = tcpClient,
//                        _username = username
//                    }
//                );
//            }
//            catch (Exception)
//            {
//                Console.WriteLine("Client disconnected!");
//            }
//        }
//
//        //public void SaveFile(IAsyncResult asyncResult)
//        //{
//        //    Console.WriteLine("Waiting for client...");
//        //    tcpListener.BeginAcceptTcpClient(SaveFile, null);
//        //    
//        //    using (var tcpClient = tcpListener.EndAcceptTcpClient(asyncResult))
//        //    using (var networkStream = tcpClient.GetStream())
//        //    using (var binaryReader = new BinaryReader(networkStream, Encoding.UTF8))
//        //    {
//        //        var fileName = binaryReader.ReadString();
//        //        var length = binaryReader.ReadInt64();
//        //
//        //        var mib = length / 1024.0 / 1024.0;
//        //        Console.WriteLine("Receiving '{0}' ({1:N1} MiB)", fileName, mib);
//        //
//        //        var stopwatch = Stopwatch.StartNew();
//        //
//        //        var fullFilePath = Path.Combine(Path.GetTempPath(), fileName);
//        //        using (var fileStream = File.Create(fullFilePath))
//        //            networkStream.CopyTo(fileStream);
//        //
//        //        var elapsed = stopwatch.Elapsed;
//        //
//        //        Console.WriteLine("Received in {0} ({1:N1} MiB/sec)",
//        //            elapsed, mib / elapsed.TotalSeconds);
//        //    }
//        //}
//    }
//}
//