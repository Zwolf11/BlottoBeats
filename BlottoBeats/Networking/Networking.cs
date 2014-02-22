using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace Networking {
	/// <summary>
	/// Server object used to store data about the server
	/// </summary>
	public class BBServerConnection {
		private TcpClient client;
		private IPEndPoint serverEndPoint;
		private NetworkStream networkStream;

		/// <summary>
		/// Initializes a new connection to a BlottoBeats Server with a default
		/// ip address of localhost and a default port of 3000
		/// </summary>
		public BBServerConnection() {
			serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
		}

		/// <summary>
		/// Initializes a new connection to a BlottoBeats Server with the given IP address and port
		/// </summary>
		/// <param name="ipAddress">IP address of the server</param>
		/// <param name="port">Port to connect to the server with</param>
		public BBServerConnection(string ipAddress, int port) {
			serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
		}

		/// <summary>
		/// Initiates a connection to the server
		/// </summary>
		private void Connect() {
			client = new TcpClient();
			client.Connect(serverEndPoint);
			networkStream = client.GetStream();
		}

		/// <summary>
		/// Disconnects an active connection from the server
		/// </summary>
		private void Disconnect() {
			client.Close();
			client = null;
			networkStream = null;
		}

		/// <summary>
		/// Sends a BBRequest to the server
		/// </summary>
		/// <param name="request">The BBRequest to send</param>
		/// <returns>If the reqeuest expects a response, returns the response</returns>
		public object SendRequest(BBRequest request) {
			Connect();
			Message.Send(networkStream, request);
			if (request.ExpectsResponse())
				return Message.Recieve(networkStream);
			Disconnect();
			return null;
		}
	}

	/// <summary>
	/// Request object for communication between the BlottoBeats server and client.
	/// </summary>
	public class BBRequest {
		private Type type;
		private object thingy;
		private bool[] expectsResponse;

		

		public bool ExpectsResponse() {
			return false;
		}

		public enum Type {
			UPDOWNVOTE,
			REQUESTSONGS,
			RESPONSESONGS
		}
	}

	/// <summary>
	/// Message helper class for to clog the tubes with.
	/// Use Message.Send() to send a message and Message.Receive() to receive it.
	/// Used to send and receive objects directly
	/// </summary>
	public class Message {
		/// <summary>
		/// Sends a single object over the specified stream 
		/// </summary>
		/// <param name="stream">Stream to send the message over</param>
		/// <param name="obj">Object to send</param>
		public static void Send(NetworkStream stream, object obj) {
			byte[] data;
			byte[] dataLength;

			using (MemoryStream memoryStream = new MemoryStream()) {
				(new BinaryFormatter()).Serialize(memoryStream, obj);
				data = memoryStream.ToArray();
			}
			dataLength = BitConverter.GetBytes((Int32)data.Length);

			if (BitConverter.IsLittleEndian) {
				// Use Big-Endian transmission
				Array.Reverse(data);
				Array.Reverse(dataLength);
			}

			stream.Write(dataLength, 0, sizeof(Int32));	//send length of message
			stream.Write(data, 0, data.Length);			//send message itself
			stream.Flush();
		}

		/// <summary>
		/// Receieves a single object over the specified stream
		/// </summary>
		/// <param name="stream">Stream to receive the message from</param>
		/// <returns>Object received</returns>
		public static object Recieve(NetworkStream stream) {
			int dataLength;
			byte[] data = new byte[sizeof(Int32)];

			int bytesRead = 0;
			int totalBytesRead = 0;

			try {
				// Read the length integer
				do {
					bytesRead = stream.Read(data, totalBytesRead, (data.Length - totalBytesRead));
					totalBytesRead += bytesRead;
				} while (totalBytesRead < sizeof(Int32) && bytesRead != 0);

				if (totalBytesRead < sizeof(Int32)) {
					if (totalBytesRead != 0) Console.Error.WriteLine("Message Recieve Failed: connection closed unexpectedly");
					return null;
				}

				if (BitConverter.IsLittleEndian) Array.Reverse(data);

				dataLength = BitConverter.ToInt32(data, 0);
				data = new byte[dataLength];

				// Read data until the client disconnects
				totalBytesRead = 0;
				do {
					bytesRead = stream.Read(data, totalBytesRead, (dataLength - totalBytesRead));
					totalBytesRead += bytesRead;
				} while (totalBytesRead < dataLength && bytesRead != 0);

				if (totalBytesRead < dataLength) {
					Console.Error.WriteLine("Message Receive Failed: connection closed unexpectedly");
					return null;
				}

				if (BitConverter.IsLittleEndian) Array.Reverse(data);

				using (MemoryStream memoryStream = new MemoryStream(data)) {
					return (new BinaryFormatter()).Deserialize(memoryStream);
				}
			} catch (Exception e) {
				Console.Error.WriteLine("A socket error has occured: " + e.ToString());
				return null;
			}
		}
	}

	/*
	/// <summary>
	/// A basic demo client to show how to send messages to the server
	/// </summary>
	class ClientDemo {
		private TcpClient client;
		private IPEndPoint serverEndPoint;
		private NetworkStream clientStream;

		/// <summary>
		/// Initializes a new ClientDemo with a default ip address of localhost
		/// and a default port of 3000
		/// </summary>
		public ClientDemo() {
			serverEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 3000);
		}

		/// <summary>
		/// Initializes a new ClientDemo with the given IP address and port
		/// </summary>
		/// <param name="ipAddress">IP address of the server</param>
		/// <param name="port">Port to connect to the server with</param>
		public ClientDemo(string ipAddress, int port) {
			serverEndPoint = new IPEndPoint(IPAddress.Parse(ipAddress), port);
		}

		/// <summary>
		/// Initiates a connection to the server
		/// </summary>
		public void ConnectToServer() {
			if (client != null && client.Connected) {
				Console.Error.WriteLine("Can't connect to server as there is already an active connection!");
				return;
			}

			Console.WriteLine("<client> Initilizing connection...");

			client = new TcpClient();
			client.Connect(serverEndPoint);
			clientStream = client.GetStream();
		}

		/// <summary>
		/// Disconnects an active connection from the server
		/// </summary>
		public void DisconnectFromServer() {
			if (client != null && !client.Connected) {
				Console.Error.WriteLine("Can't disconnect from server as there is no active connection!");
				return;
			}

			Console.WriteLine("<client> Disconnected from server");

			client.Close();
			clientStream = null;
		}

		/// <summary>
		/// Takes a single object and converts it to a message which is sent to the server
		/// </summary>
		/// <param name="obj">The object to send</param>
		public void SendMessageToServer(object obj) {
			if (!client.Connected) {
				Console.Error.WriteLine("Can't send a message with no connection to the server!");
				return;
			}

			Console.WriteLine("<client> Sending message '" + obj.ToString() + "'");

			Message.Send(clientStream, obj);
		}


		/// <summary>
		/// Receieves a single message from the server and converts it to an object
		/// </summary>
		/// <returns>The object received</returns>
		public object ReceiveMessageFromServer() {
			if (!client.Connected) {
				Console.Error.WriteLine("Can't receive a message with no connection to the server!");
				return null;
			}

			return Message.Recieve(clientStream);
		}
	}
	*/
}
