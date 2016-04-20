using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace FolderSyncServer.Network
{
	class Connection
	{
		private byte[] _buffer, _backBuffer;
		private Socket _socket;

		private object _cleanUpLock = new object();
		private bool _cleanedUp;

		/// <summary>
		/// State of the connection.
		/// </summary>
		public ConnectionState State { get; protected set; }

		/// <summary>
		/// Remote address.
		/// </summary>
		public string Address { get; protected set; }

		/// <summary>
		/// Raised when connection is closed.
		/// </summary>
		public event EventHandler Closed;

		/// <summary>
		/// Connection's index on the connection manager's list.
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		/// Session key for this connection.
		/// </summary>
		public string SessionKey { get; set; }

		/// <summary>
		/// Creates new connection.
		/// </summary>
		public Connection()
		{
			_buffer = new byte[1024 * 500];
			_backBuffer = new byte[ushort.MaxValue];

			this.State = ConnectionState.Open;
			this.Address = "?:?";
		}

		/// <summary>
		/// Sets connection's socket once.
		/// </summary>
		/// <param name="socket"></param>
		/// <exception cref="InvalidOperationException">Thrown if socket was already set.</exception>
		public void SetSocket(Socket socket)
		{
			if (_socket != null)
				throw new InvalidOperationException("Socket is already set.");

			_socket = socket;
			this.Address = ((IPEndPoint)socket.RemoteEndPoint).ToString();
		}

		/// <summary>
		/// Closes the connection.
		/// </summary>
		public void Close()
		{
			if (this.State == ConnectionState.Closed)
			{
				Console.WriteLine("Attempted closing of an already closed connection.");
				return;
			}

			this.State = ConnectionState.Closed;

			try { _socket.Shutdown(SocketShutdown.Both); }
			catch { }
			try { _socket.Close(); }
			catch { }

			this.OnClosed();

			Console.WriteLine($"Closed connection from '{this.Address}'.");
		}

		/// <summary>
		/// Starts packet receiving.
		/// </summary>
		public void BeginReceive()
		{
			_socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnReceive, null);
		}

		/// <summary>
		/// Called when new data is available from socket.
		/// </summary>
		/// <param name="result"></param>
		private void OnReceive(IAsyncResult result)
		{
			try
			{
				var length = _socket.EndReceive(result);
				var read = 0;

				// Client disconnected
				if (length == 0)
				{
					this.State = ConnectionState.Closed;
					this.OnClosed();
					Console.WriteLine($"Connection was closed from '{this.Address}'.");
					return;
				}

				while (read < length)
				{
					var packetLength = BitConverter.ToUInt16(_buffer, read);
					if (packetLength > length)
					{
						Console.WriteLine(BitConverter.ToString(_buffer, read, length - read));
						throw new Exception("Packet length greater than buffer length (" + packetLength + " > " + length + ").");
					}

					// Read packet from buffer
					var packetBuffer = new byte[packetLength];
					Buffer.BlockCopy(_buffer, read + sizeof(short), packetBuffer, 0, packetLength);
					read += sizeof(short) + packetLength;
				}

				this.BeginReceive();
			}
			catch (SocketException)
			{
				this.State = ConnectionState.Closed;
				this.OnClosed();
				Console.WriteLine($"Lost connection from '{this.Address}'.");

			}
			catch (ObjectDisposedException)
			{
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error while receiving packet.");
			}
		}

		/// <summary>
		/// To be called when connection is closed, calls event
		/// and CleanUp.
		/// </summary>
		private void OnClosed()
		{
			var ev = this.Closed;
			if (ev != null)
				ev(this, null);

			lock (_cleanUpLock)
			{
				if (!_cleanedUp)
					this.CleanUp();
				else
					Console.WriteLine("Trying to clean already cleaned connection.");

				_cleanedUp = true;
			}
		}

		/// <summary>
		/// Called when the connection is closed.
		/// </summary>
		protected virtual void CleanUp()
		{
			Console.WriteLine("CLEAN UP");
		}

		/// <summary>
		/// Sends packet to client.
		/// </summary>
		/// <param name="packet"></param>
		public void Send(byte[] bytes)
		{
		}
	}

	public enum ConnectionState
	{
		Closed,
		Open,
	}
}
