using Networking;
using SongData;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;

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

		/// <summary>
		/// Starts up the server
		/// </summary>
		public static void Main() {
			// Create a new server that listens on port 3000;
            Server server = new Server(3000, new Database("Server=localhost;Port=3306;Database=songdatabase;Uid=root;password=joeswanson;"), "server.log");

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
		internal Server(int port, Database database, string logFileLocation) {
			this.database = database;
			this.logFileLocation = logFileLocation;

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
					BBRequest bbrequest = (BBRequest)message;
					Log("Received a " + bbrequest.requestType + " request", address);

					if (bbrequest.requestType is BBRequest.UpDownVote) {

						// Upload and vote on a song
						BBRequest.UpDownVote req = (BBRequest.UpDownVote)bbrequest.requestType;
						database.VoteOnSong(req.song, req.vote);
						SongAndVoteData response = database.GetSongScore(req.song);
						Message.Send(networkStream, response);

					} else if (bbrequest.requestType is BBRequest.RequestScore) {

						// Request the score of a song
						BBRequest.RequestScore req = (BBRequest.RequestScore)bbrequest.requestType;
						SongAndVoteData response = database.GetSongScore(req.song);
						Message.Send(networkStream, response);

					} else if (bbrequest.requestType is BBRequest.RequestSongs) {

						// Request a list of songs
						BBRequest.RequestSongs req = (BBRequest.RequestSongs)bbrequest.requestType;
						List<SongAndVoteData> songList = database.GetSongList(req.parameters, req.num);
						Message.Send(networkStream, songList);

					} else {
						Log("ERROR: Unknown BBRequest type '" + bbrequest.GetType() + "'", address);
					}

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
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(logFileLocation, true)) {
				file.WriteLine("<{0}> {1}", Timestamp(), message);
			}
		}

		/// <summary>
		/// Logs a message and the IP of the connected client
		/// </summary>
		/// <param name="message">Message to log</param>
		/// <param name="address">IP address to log</param>
		private void Log(string message, IPAddress address) {
			Console.WriteLine("<{0}> {2} - {1}", Timestamp(), message, address);
			using (System.IO.StreamWriter file = new System.IO.StreamWriter(logFileLocation, true)) {
				file.WriteLine("<{0}> {2} - {1}", Timestamp(), message, address);
			}
		}

		/// <summary>
		/// Gets the current time as a string
		/// </summary>
		/// <returns></returns>
		private string Timestamp() {
			return String.Format("{0:hh:mm:ss}", DateTime.Now);
		}
	}

	/// <summary>
	/// Handles all communication with the MySQL database
	/// </summary>
	internal class Database {
		// TODO - JOE: Add stuff
        string connString;

		/// <summary>
		/// Loads a database from the given path
		/// </summary>
		internal Database(string pathToDatabase) {
            connString = pathToDatabase;
		}

		/// <summary>
		/// Checks to see if a song is in the database already.  If it isn't, the song is
		/// added to the database with a single vote.  If it is, add the vote to that song.
		/// </summary>
		/// <param name="song">The song object to vote on</param>
		/// <param name="vote">The vote. True if upvote, false if downvote.</param>
		internal SongAndVoteData VoteOnSong(Song song, bool vote) {
			// TODO - JOE: Implement
			return new SongAndVoteData(song, 0);
		}

		/// <summary>
		/// Returns the score of the given song.  If the song isn't in the database,
		/// returns zero.
		/// </summary>
		/// <param name="song">Song to check the score of</param>
		/// <returns>A SongAndVoteData item containing the song and it's score</returns>
		internal SongAndVoteData GetSongScore(Song song) {
			
			return new SongAndVoteData(song, 0);
		}

		/// <summary>
		/// Gets a list of songs that match the given SongParameters object
		/// </summary>
		/// <param name="songParameters">The parameters to match</param>
		/// <param name="numSongs">The maximum number of songs to return</param>
		/// <returns>The list of songs</returns>
		internal List<SongAndVoteData> GetSongList(SongParameters songParameters, int numSongs) {
			// TODO - JOE: Implement
			return new List<SongAndVoteData>();
		}

        private void insertData(int id, string genre, byte[] songData, int score)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "Insert into uploadedsongs (iduploadedsongs,genre,songseed,voteScore) values('" + id + "','" + genre + "','" + songData + "','" + score + "')";
            conn.Open();
            command.ExecuteNonQuery();
            conn.Close();
        }

        private void updateScore(int id, bool vote)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
            int scoreUpdate;

            if (vote == true)
            {
                scoreUpdate = (int)(returnItem(id, "voteScore"));
                Console.WriteLine(scoreUpdate);
                scoreUpdate += 1;
                command.CommandText = "Update uploadedsongs SET voteScore='" + scoreUpdate + "' WHERE iduploadedsongs='" + id + "'";
                conn.Open();
                command.ExecuteNonQuery();
                conn.Close();
            }
            else
            {
                scoreUpdate = (int)(returnItem(id, "voteScore"));
                scoreUpdate -= 1;
                command.CommandText = "Update uploadedsongs SET voteScore='" + scoreUpdate + "' WHERE iduploadedsongs='" + id + "'";
                conn.Open();
                command.ExecuteNonQuery();
                conn.Close();
            }
        }

        private object returnItem(int id, string col)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "Select " + col + " from uploadedsongs where iduploadedsongs=" + id;
            object item = null;
            try
            {
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            MySqlDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                item = reader[col];
               
            }
            
            conn.Close();

            return item;
        }
        }
	}
}
