using Networking;
using SongData;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BlottoBeatsServer {
	/// <summary>
	/// Basic TCP Server
	/// Uses ThreadPool to handle multiple connections at the same time.
	/// </summary>
	internal class Server {
		private TcpListener tcpListener;
		private Thread listenThread;
		
		private Database database;
		private string logFileLocation;
		private int logLevel;	// How verbose the logs should be.  Higher is more verbose.

		/// <summary>
		/// Starts up the server
		/// </summary>
		public static void Main() {
			// Create a new server that listens on port 3000;
			Server server = new Server(3000, new Database("Server=localhost;Port=3306;Database=songdatabase;Uid=root;password=joeswanson;"), "server.log", 3);
			//Server server = new Server(3000, new Database("Server=68.234.146.20;Port=3001;Database=songdatabase;Uid=BlottoServer;password=JJLrDtcrfvjym8gh1zUVklF19KDf1CTM;"), "server.log", 3);

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
		/// <param name="database">SQL Database string</param>
		/// <param name="logFileLocation">Location to save the log</param>
		/// <param name="logLevel">What level of logs to use</param>
		internal Server(int port, Database database, string logFileLocation, int logLevel) {
			this.database = database;
			this.logFileLocation = logFileLocation;
			this.logLevel = logLevel;

			tcpListener = new TcpListener(IPAddress.Any, port);
			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
		}

		/// <summary>
		/// Checks the health of the server
		/// </summary>
		/// <returns>Returns true if the server is alive, false otherwise</returns>
		internal bool IsAlive() {
			return listenThread.IsAlive;
		}

		/// <summary>
		/// Restarts the server thread
		/// </summary>
		internal void Restart() {
			Log("Restarting Server...", 0);
			
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
			Log("Server started", 0);

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
			

			Log("Remote client connected", address, 0);
			
			object message = Message.Recieve(networkStream);
			while (tcpClient.Connected && message != null) {
				
				if (message is string && (string)message == "Test") {

					// A test message was recieved.  Send a response back.
					Log("Recieved a connection test request", address, 1);
					Message.TestMsg(networkStream);

				} else if (message is BBMessage) {

					// A BBRequest was recieved.  Process the request
					BBMessage bbmessage = message as BBMessage;
					Log("Received a " + bbmessage.requestType + " request", address, 1);

					if (bbmessage.requestType is BBMessage.UpDownVote) {

						// Upload and vote on a song
						BBMessage.UpDownVote req = bbmessage.requestType as BBMessage.UpDownVote;

						Log("    " + (req.vote ? "Upvote" : "Downvote") + " Request", address, 2);
						Log("        ID - " + req.song.ID , address, 3);
						Log("     Genre - " + req.song.genre, address, 3);
						Log("     Tempo - " + req.song.tempo, address, 3);
						Log("      Seed - " + req.seed, address, 3);

						CompleteSongData response = database.VoteOnSong(req.seed, req.song, req.vote);
						Message.Send(networkStream, response);
						
						Log("    Response has ID of " + response.song.ID + " and score of " + response.score, address, 2);

					} else if (bbmessage.requestType is BBMessage.RequestScore) {

						// Request the score of a song
						BBMessage.RequestScore req = bbmessage.requestType as BBMessage.RequestScore;

						Log("        ID - " + req.song.ID, address, 3);
						Log("     Genre - " + req.song.genre, address, 3);
						Log("     Tempo - " + req.song.tempo, address, 3);
						Log("      Seed - " + req.seed, address, 3);

						CompleteSongData response = database.GetSongScore(req.seed, req.song);
						Message.Send(networkStream, response);

						Log("    Response has ID of " + response.song.ID + " and score of " + response.score, address, 2);

					} else if (bbmessage.requestType is BBMessage.RequestSongs) {

						// Request a list of songs
						BBMessage.RequestSongs req = bbmessage.requestType as BBMessage.RequestSongs;
						List<CompleteSongData> songList = database.GetSongList(req.parameters, req.num);
						Message.Send(networkStream, songList);
						Log("    Returned " + songList.Count + " songs", address, 2);

					} else {
						Log("ERROR: Unknown BBRequest type '" + bbmessage.GetType() + "'", address, 0);
					}

				} else {
					Log("ERROR: Unknown request type '" + message.GetType() + "'", address, 0);
					Log("    MORE INFO: " + message.ToString(), address, 0);
				}

				message = Message.Recieve(networkStream);
			}

			Log("Remote client disconnected", address, 0);

			tcpClient.Close();
		}

		/// <summary>
		/// Logs a message
		/// </summary>
		/// <param name="message">Message to log</param>
		private void Log(string message, int level) {
			if (logLevel >= level) {
				Console.WriteLine("<{0}> {1}", Timestamp(), message);
				using (System.IO.StreamWriter file = new System.IO.StreamWriter(logFileLocation, true)) {
					file.WriteLine("<{0}> {1}", Timestamp(), message);
				}
			}
		}

		/// <summary>
		/// Logs a message and the IP of the connected client
		/// </summary>
		/// <param name="message">Message to log</param>
		/// <param name="address">IP address to log</param>
		private void Log(string message, IPAddress address, int level) {
			Log(String.Format("{0} - {1}", address, message), level);
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
