using MySql.Data.MySqlClient;
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
	private class Server {
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
						database.VoteOnSong(req.seed, req.song, req.vote);
						SongAndVoteData response = database.GetSongScore(req.seed, req.song);
						Message.Send(networkStream, response);
						Log("  Song has ID of " + response.song.ID + " and score of " + response.score, address);

					} else if (bbrequest.requestType is BBRequest.RequestScore) {

						// Request the score of a song
						BBRequest.RequestScore req = (BBRequest.RequestScore)bbrequest.requestType;
						SongAndVoteData response = database.GetSongScore(req.seed, req.song);
						Message.Send(networkStream, response);
						Log("  Song has ID of " + response.song.ID + " and score of " + response.score, address);

					} else if (bbrequest.requestType is BBRequest.RequestSongs) {

						// Request a list of songs
						BBRequest.RequestSongs req = (BBRequest.RequestSongs)bbrequest.requestType;
						List<SongAndVoteData> songList = database.GetSongList(req.parameters, req.num);
						Message.Send(networkStream, songList);
						Log("  Returned " + songList.Count + " songs", address);

					} else {
						Log("ERROR: Unknown BBRequest type '" + bbrequest.GetType() + "'", address);
					}

				} else {
					Log("ERROR: Unknown request type '" + message.GetType() + "'", address);
					Log("  MORE INFO: " + message.ToString(), address);
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
        string connString;
		int nextID;

		/// <summary>
		/// Loads a database from the given path
		/// </summary>
		internal Database(string connString) {
            this.connString = connString;
			this.nextID = GetNextAvailableID(1);
		}

		/// <summary>
		/// Checks to see if a song is in the database already.  If it isn't, the song is
		/// added to the database with a single vote.  If it is, add the vote to that song.
		/// </summary>
		/// <param name="song">The song object to vote on</param>
		/// <param name="vote">The vote. True if upvote, false if downvote.</param>
		internal SongAndVoteData VoteOnSong(int seed, SongParameters song, bool vote) {
			if (song.ID == -1) song.ID = GetID(seed, song);	// Song has no ID.  Search the server
			if (song.ID == -1) {
				// Song still has no ID.  Insert it into the database
				song.ID = nextID;
				nextID = GetNextAvailableID(nextID);
				insertData(song.ID, song.genre, Message.Pack(song), (vote) ? 1 : -1);
			} else {
				updateScore(song.ID, vote);
			}

			return new SongAndVoteData(seed, song, (int)returnItem(song.ID, "voteScore"));
		}

		/// <summary>
		/// Returns the score of the given song.  If the song isn't in the database,
		/// returns zero.  Does not add the song to the database if it isn't already there.
		/// </summary>
		/// <param name="song">Song to check the score of</param>
		/// <returns>A SongAndVoteData item containing the song and it's score</returns>
		internal SongAndVoteData GetSongScore(int seed, SongParameters song) {
			if (song.ID == -1) song.ID = GetID(seed, song);	// Song has no ID.  Search the server
			if (song.ID == -1)
				return new SongAndVoteData(seed, song, 0);	// Song still has no ID. Return score of 0.
			else
				return new SongAndVoteData(seed, song, (int)returnItem(song.ID, "voteScore"));
		}

		/// <summary>
		/// Gets a list of songs that match the given SongParameters object
		/// </summary>
		/// <param name="songParameters">The parameters to match</param>
		/// <param name="numSongs">The maximum number of songs to return</param>
		/// <returns>The list of songs</returns>
		internal List<SongAndVoteData> GetSongList(SongParameters songParameters, int numSongs) {
			List<SongAndVoteData> list = new List<SongAndVoteData>();
			
			// TODO: Implement

			return list;
		}

		/// <summary>
		/// Searches the database for a song that matches the given song.
		/// If there is a match, returns the ID.  If not, returns -1
		/// </summary>
		/// <param name="song">Song to search the database for</param>
		/// <returns>ID of the song on the server</returns>
		private int GetID(int seed, SongParameters song) {
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "Select iduploadedsongs from uploadedsongs where genre like '%" + song.genre + "%' and songseed like '%" + seed + "%' and tempo like '%" + song.tempo + "%'";
            int returnId = -1;
            try
            {
                conn.Open();
				MySqlDataReader reader = command.ExecuteReader();
				while (reader.Read()) {
					returnId = (int)reader["iduploadedsongs"];
				}
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
			finally
			{
				conn.Close();
			}
            
            return returnId;
        }


		/// <summary>
		/// Gets the next available ID for the server
		/// </summary>
		/// <returns>ID</returns>
		private int GetNextAvailableID(int currID) {
            int testId = currID;

            while (returnItem(testId, "iduploadedsongs") != null)
            {
                testId += 1;
            }

            return testId;
		}

        private void insertData(int id, string genre, byte[] songData, int score)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "Insert into uploadedsongs (iduploadedsongs,genre,songseed,voteScore) values('" + id + "','" + genre + "','" + songData + "','" + score + "')";

			try
			{
				conn.Open();
				command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
			}
			finally
			{
				conn.Close();
			}
        }

        private void updateScore(int id, bool vote)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
			int scoreUpdate = (int)(returnItem(id, "voteScore"));

            if (vote == true)
            {
                scoreUpdate += 1;
            }
            else
            {
                scoreUpdate -= 1;
            }

			command.CommandText = "Update uploadedsongs SET voteScore='" + scoreUpdate + "' WHERE iduploadedsongs='" + id + "'";
			try
			{
				conn.Open();
				command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex.Message);
			}
			finally
			{
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
				MySqlDataReader reader = command.ExecuteReader();
				while (reader.Read()) {
					item = reader[col];
				}
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
			finally
			{
				conn.Close();
			}
            
			return item;
        }
	}
}
