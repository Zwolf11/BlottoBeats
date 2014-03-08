using SongData;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Networking {
	/// <summary>
	/// Server object used to store data about the server
	/// </summary>
	public class BBServerConnection {
		private IPEndPoint serverEndPoint;

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
		/// Sends an authentication request to the server.  Returns a UserToken
		/// object if successful, or null otherwise.
		/// </summary>
		/// <param name="username">Username to send</param>
		/// <param name="password">Passoword to send</param>
		/// <returns>UserToken if successful, null otherwise</returns>
		public UserToken Authenticate(string username, string password) {

			// TODO: Finish this
			// currently returns a newly-generated and completely arbitrary user token

			return new UserToken(username, UserToken.GetExpiration(), UserToken.GenerateToken());
		}

		/// <summary>
		/// Sends a BBRequest to the server
		/// </summary>
		/// <param name="request">The BBRequest to send</param>
		/// <returns>If the reqeuest expects a response, returns the response</returns>
		public BBMessage.Response SendRequest(BBMessage request) {
			object reply;
			
			using (TcpClient client = new TcpClient()) {
				client.Connect(serverEndPoint);
				NetworkStream networkStream = client.GetStream();

				Message.Send(networkStream, request);				
				reply = Message.Recieve(networkStream);
			}

			if (reply == null)
				throw new Exception("BBRequest Error: Expected reply but recieved none");//Console.Error.WriteLine("BBRequest Error: Expected reply but recieved none");
			else
				return reply as BBMessage.Response;
		}

		/// <summary>
		/// Tests the server to see if the connection is valid
		/// </summary>
		/// <returns>True if the connection is valid, false otherwise</returns>
		public bool Test() {
			bool result = false;
			
			using (TcpClient client = new TcpClient()) {
				client.Connect(serverEndPoint);
				NetworkStream networkStream = client.GetStream();

				result = Message.Test(networkStream);
			}

			return result;
		}
	}

	/// <summary>
	/// Request object for communication between the BlottoBeats client and server.
	/// </summary>
	[SerializableAttribute]
	public class BBMessage {
		public BBMessageObject requestType { get; private set; }

		/// <summary>
		/// Sends an upload request with a single song and either an upvote or a downvote.
		/// The server will check the database for that song.  If the song exists, it will
		/// add the vote to it, otherwise it will add the song to the server and save it
		/// with the vote.
		/// 
		/// The response will contain a single SongAndVoteData item with the song and it's score
		/// </summary>
		/// <param name="song">Song to upload</param>
		/// <param name="upOrDownvote">Vote. True if an upvote, false otherwise.</param>
		public BBMessage(int seed, SongParameters song, bool upOrDownvote, UserToken userInfo) {
			requestType = new UpDownVote(seed, song, upOrDownvote, userInfo);
		}

		/// <summary>
		/// Sends a request for the score of a single song.
		/// 
		/// The response will contain a single SongAndVoteData item with the song and it's score
		/// </summary>
		/// <param name="song">Song to check the score of</param>
		public BBMessage(int seed, SongParameters song, UserToken userInfo) {
			requestType = new RequestScore(seed, song, userInfo);
		}

		/// <summary>
		/// Sends a request for a list of songs that match the given parameters.
		/// 
		/// The response will contain a list of songs that match the parameters.
		/// </summary>
		/// <param name="parameters">Parameters to match</param>
		/// <param name="numberOfSongs">Number of songs to return</param>
		public BBMessage(SongParameters parameters, int numberOfSongs, UserToken userInfo) {
			requestType = new RequestSongs(parameters, numberOfSongs, userInfo);
		}

		/// <summary>
		/// Base message class
		/// </summary>
		public class BBMessageObject { }

		/// <summary>
		/// Generic request object
		/// </summary>
		[SerializableAttribute]
		public class Request : BBMessageObject {
			public UserToken userInfo { get; private set; }

			protected Request(UserToken userInfo) {
				this.userInfo = userInfo;
			}
		}

		/// <summary>
		/// Uploads a song with a vote
		/// </summary>
		[SerializableAttribute]
		public class UpDownVote : Request {
			public SongParameters song { get; private set; }
			public int seed { get; private set; }
			public bool vote { get; private set; }

			public UpDownVote(int seed, SongParameters song, bool vote, UserToken userInfo) : base(userInfo) {
				this.song = song;
				this.seed = seed;
				this.vote = vote;
			}
		}

		/// <summary>
		/// Requests the score of a song
		/// </summary>
		[SerializableAttribute]
		public class RequestScore : Request {
			public SongParameters song { get; private set; }
			public int seed { get; private set; }

			public RequestScore(int seed, SongParameters song, UserToken userInfo) : base(userInfo) {
				this.song = song;
				this.seed = seed;
			}
		}

		/// <summary>
		/// Request a list of songs
		/// </summary>
		[SerializableAttribute]
		public class RequestSongs : Request {
			public SongParameters parameters { get; private set; }
			public int num { get; private set; }

			public RequestSongs(SongParameters parameters, int num, UserToken userInfo) : base(userInfo) {
				this.parameters = parameters;
				this.num = num;
			}
		}

		// Base Response class
		public class Response : BBMessageObject { }

		/// <summary>
		/// Responds with a single song
		/// </summary>
		[SerializableAttribute]
		public class ResponseSong : Response {
			public CompleteSongData song { get; private set; }

			public ResponseSong(CompleteSongData song) {
				this.song = song;
			}
		}

		/// <summary>
		/// Responds with a list of songs
		/// </summary>
		[SerializableAttribute]
		public class ResponseSongs : Response {
			public List<CompleteSongData> songs { get; private set; }

			public ResponseSongs(List<CompleteSongData> songs) {
				this.songs = songs;
			}
		}
	}
}
