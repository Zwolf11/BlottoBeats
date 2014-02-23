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
	public class Server {
		private TcpListener tcpListener;
		private Thread listenThread;

		/// <summary>
		/// Starts up the server
		/// </summary>
		public static void Main() {
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
			Log("Restarting Server...");
			
			listenThread.Abort();
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
		}

		/// <summary>
		/// Main server thread function.
		/// Accepts clients over TCP and uses ThreadPool to support multiple connections at the same time.
		/// </summary>
		private void ListenForClients() {
			tcpListener.Start();
			Log("Server started");

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
			IPAddress address = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;

			Log("Remote client connected", address);
			
			object message = Message.Recieve(networkStream);
			while (tcpClient.Connected && message != null) {
				
				if (message is string && (string)message == "Test") {
					// A test message was recieved.  Send a response back.
					Log("Recieved a connection test request", address);
					Message.TestMsg(networkStream);
				} else if (message is BBRequest) {
					// A BBRequest was recieved.  Process the request
					BBRequest request = (BBRequest)message;
					Log("Received a " + request.GetRequest() + " request", address);

					// TODO: Process request

				} else {
					Log("ERROR: Unknown request type '" + message.GetType() + "'", address);
				}

				message = Message.Recieve(networkStream);
			}

			Log("Remote client disconnected", address);

			tcpClient.Close();
		}

		/// <summary>
		/// Logs a message
		/// </summary>
		/// <param name="message">Message to log</param>
		private void Log(string message) { 
			Console.WriteLine("<{0}> {1}", Timestamp(), message);
		}

		/// <summary>
		/// Logs a message and the IP of the connected client
		/// </summary>
		/// <param name="message">Message to log</param>
		/// <param name="address">IP address to log</param>
		private void Log(string message, IPAddress address) {
			Console.WriteLine("<{0}> {2} - {1}", Timestamp(), message, address);
		}

		/// <summary>
		/// Gets the current time as a string
		/// </summary>
		/// <returns></returns>
		private string Timestamp() {
			return String.Format("{0:hh:mm:ss}", DateTime.Now);
		}
	}
}
