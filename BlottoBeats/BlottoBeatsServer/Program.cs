using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using Networking;

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
			NetworkStream clientStream = tcpClient.GetStream();

			Console.WriteLine("<server> Received connection from client...");

			object thingy = Message.Recieve(clientStream);
			while (tcpClient.Connected && thingy != null) {
				// TODO: Actually do something useful.
				if (thingy is string) {
					Console.WriteLine("<server> Message from client: '" + thingy + "'");
				} else if (thingy is int && (int)thingy == 418) {
					Console.WriteLine("<server> HTTP: 418 error");
					Message.Send(clientStream, "I'm a teapot");
				} else {
					Console.WriteLine("<Server> What is this I don't even...");
					Console.WriteLine("<Server>  " + thingy.ToString());
				}

				thingy = Message.Recieve(clientStream);
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
