using Networking;
using SongData;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BlottoBeatsServer {
	/// <summary>
	/// Basic TCP Server
	/// Uses ThreadPool to handle multiple connections at the same time.
	/// </summary>
	class Server {
		private TcpListener tcpListener;
		private Thread listenThread;
		
		/// <summary>
		/// Starts up the server
		/// </summary>
		public static void main() {
			// Create a new server that listens on port 3000;
			Server server = new Server(3000);

			// Checks the state of the server every 5 seconds
			// If the server thread has died, restart it
			while (true) {
				if (!server.IsAlive()) server.Restart();
				Thread.Sleep(5000);
			}
		}

		/// <summary>
		/// Creates a new server object that listens on the specified port
		/// </summary>
		/// <param name="port">The port to listen on</param>
		public Server(int port) {
			tcpListener = new TcpListener(IPAddress.Any, port);
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
		}
		
		/// <summary>
		/// Main server thread function.
		/// Accepts clients over TCP and uses ThreadPool to support multiple connections at the same time.
		/// </summary>
		private void ListenForClients() {
			tcpListener.Start();

			while (true) {
				TcpClient client = tcpListener.AcceptTcpClient();
				ThreadPool.QueueUserWorkItem(new WaitCallback(HandleConnectionWithClient), client);
			}
		}
		
		/// <summary>
		/// Child thread function.
		/// Handles a connection with a single client.
		/// </summary>
		/// <param name="client">The TcpClient object</param>
		private void HandleConnectionWithClient(object client) {
			TcpClient tcpClient = (TcpClient)client;
			NetworkStream networkStream = tcpClient.GetStream();

			Console.WriteLine("<server> Received connection from client...");

			object message = Message.Recieve(networkStream);
			while (tcpClient.Connected && message != null) {
				// TODO: Actually do something useful.
				if (message is string && (string)message == "Test") {
					// A test message was recieved.  Send a response back.
					Message.TestMsg(networkStream);
				} else if (message is BBRequest) {
					Console.WriteLine("<server> Received a " + BBRequest.getType() + " request");
				} else {
					Console.WriteLine("<Server> What is this I don't even...");
					Console.WriteLine("<Server>  " + message.ToString());
				}

				message = Message.Recieve(networkStream);
			}

			Console.WriteLine("<server> Client disconnected");

			tcpClient.Close();
		}

		/// <summary>
		/// Checks the health of the server
		/// </summary>
		/// <returns>Returns true if the server is alive, false otherwise</returns>
		public bool IsAlive() {
			return listenThread.IsAlive;
		}

		/// <summary>
		/// Restarts the server thread
		/// </summary>
		public void Restart() {
			listenThread.Abort();
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
		}
	}
}
