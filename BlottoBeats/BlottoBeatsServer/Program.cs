using System;
using System.IO;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace BlottoBeatsServer {
	class Program {
		static void Main(string[] args) {
			Server server = new Server();
			ClientDemo client = new ClientDemo();
			
			client.ConnectToServer();
			client.SendMessageToServer("Hello, internet!");
			client.SendMessageToServer("TCP Connection reuse is fun and lazy!");
			client.DisconnectFromServer();

			client.ConnectToServer();
			client.SendMessageToServer("Multiple connections work");
			client.SendMessageToServer(71);
			int[] derpArray = { 4, 5, 2 };
			client.SendMessageToServer(derpArray);
			client.SendMessageToServer("Are you still there?");
			client.DisconnectFromServer();
		}
	}

	// ===================================================
	//  Basic TCP Server
	// ---------------------------------------------------
	//  Spawns a new thread for each connection to a
	//  client.
	// ===================================================
	class Server {
		private TcpListener tcpListener;
		private Thread listenThread;
		private static int PORT = 3000;

		public Server() {
			this.tcpListener = new TcpListener(IPAddress.Any, PORT);
			this.listenThread = new Thread(new ThreadStart(ListenForClients));
			this.listenThread.Start();
		}
		
		// Main server thread function.
		// Accepts clients and spawns threads to handle connections with them.
		private void ListenForClients() {
			this.tcpListener.Start();

			// TODO: Convert to using ThreadPool?
			/*
			public static void Main() {
				// Queue the task.
				ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadProc));
        
				Console.WriteLine("Main thread does some work, then sleeps.");
				// If you comment out the Sleep, the main thread exits before
				// the thread pool task runs.  The thread pool uses background
				// threads, which do not keep the application running.  (This
				// is a simple example of a race condition.)
				Thread.Sleep(1000);

				Console.WriteLine("Main thread exits.");
			}

			// This thread procedure performs the task.
			static void ThreadProc(Object stateInfo) {
				// No state object was passed to QueueUserWorkItem, so 
				// stateInfo is null.
				Console.WriteLine("Hello from the thread pool.");
			}
			*/
			while (true) {
				TcpClient client = this.tcpListener.AcceptTcpClient();

				Thread clientThread = new Thread(new ParameterizedThreadStart(HandleConnectionWithClient));
				clientThread.Start(client);
			}
		}
		
		// Child thread function.
		// Handles a connection with a single client.
		private void HandleConnectionWithClient(object client) {
			TcpClient tcpClient = (TcpClient)client;
			NetworkStream clientStream = tcpClient.GetStream();

			// TODO: tcpClient.Connected LIED to me!
			while (tcpClient.Connected) {
				object thingy = Message.Recieve(clientStream);

				// TODO: Actually do something useful.
				if (thingy is string) {
					Console.WriteLine(thingy);
				} else if (thingy == null) {
					Console.WriteLine("Can't find the thingy!");
				} else {
					Console.WriteLine("What is this I don't even...");
					Console.WriteLine("  " + thingy.ToString());
				}
			}

			tcpClient.Close();
		}

		// Takes a single object and converts it into a message which is sent to the client
		private void SendMessageToClient(NetworkStream clientStream, object obj) {
			Message.Send(clientStream, obj);
		}

		// Receieves a single message from the client and converts it into an object
		private object ReceieveMessageFromClient(NetworkStream clientStream) {
			return Message.Recieve(clientStream);
		}
	}

	// ===================================================
	//  Message helper object for to clog the tubes with
	// ===================================================
	public class Message {
		// Sends a single object over the specified stream
		public static void Send(NetworkStream stream, object obj) {
			byte[] data;
			byte[] dataLength;

			using (var memoryStream = new MemoryStream()) {
				(new BinaryFormatter()).Serialize(memoryStream, obj);
				data = memoryStream.ToArray();
			}
			dataLength = BitConverter.GetBytes((Int32) data.Length);

			if (BitConverter.IsLittleEndian) {
				// Use Big-Endian transmission
				Array.Reverse(data);
				Array.Reverse(dataLength);
			}

			stream.Write(dataLength, 0, sizeof(Int32));	//send length of message
			stream.Write(data, 0, data.Length);			//send message itself
			stream.Flush();
		}

		// Receieves a single object over the specified stream
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
					Console.Error.Write("Cannot receieve message: connection closed unexpectedly");
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
					Console.Error.Write("Cannot receieve message: connection closed unexpectedly");
					return null;
				}

				if (BitConverter.IsLittleEndian) Array.Reverse(data);

				using (var memoryStream = new MemoryStream(data)) {
					return (new BinaryFormatter()).Deserialize(memoryStream);
				}
			} catch (Exception e) {
				Console.Error.Write("A socket error has occured: " + e.ToString());
				return null;
			}
		}
	}

	// ===================================================
	//  A basic demo client to show how to send messages
	// ===================================================
	class ClientDemo {
		private TcpClient client;
		private IPEndPoint serverEndPoint;
		private NetworkStream clientStream;
		
		private static IPAddress IP = IPAddress.Parse("127.0.0.1"); // Defaults to localhost
		private static int PORT = 3000;								// Defaults to 3000
		
		public ClientDemo() {
			serverEndPoint = new IPEndPoint(IP, PORT);
		}

		// Initiates a connection to the server
		public void ConnectToServer() {
			if (client != null && client.Connected) {
				Console.Error.WriteLine("Can't connect to server as there is already an active connection!");
				return;
			}

			client = new TcpClient();
			client.Connect(serverEndPoint);
			clientStream = client.GetStream();
		}
		
		// Disconnects an active connection from the server
		public void DisconnectFromServer() {
			if (client != null && !client.Connected) {
				Console.Error.WriteLine("Can't disconnect from server as there is no active connection!");
				return;
			}

			client.Close();
			clientStream = null;
		}

		// Takes a single object and converts it to a message which is sent to the server
		public void SendMessageToServer(object obj) {
			if (!client.Connected) {
				Console.Error.WriteLine("Can't send a message with no connection to the server!");
				return;
			}

			Message.Send(clientStream, obj);
		}


		// Receieves a single message from the server and converts it to an object
		public object ReceiveMessageFromServer() {
			if (!client.Connected) {
				Console.Error.Write("Can't receive a message with no connection to the server!");
				return null;
			}

			return Message.Recieve(clientStream);
		}
	}
}
