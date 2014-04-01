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
		public string ip {
            get
            {
                return this.serverEndPoint.Address.ToString();
            }
            set
            {
                this.serverEndPoint.Address = IPAddress.Parse(value);
            }
		}

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
			serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), port);
		}

		/// <summary>
		/// Sends an authentication request to the server.  Returns a UserToken
		/// object if successful, or null otherwise.
		/// </summary>
		/// <param name="credentials">Credentials object that contains the username and password of the user.</param>
		/// <param name="register">True if registering a new user, false if verifying a current one.</param>
		/// <returns>UserToken if successful, null otherwise</returns>
		public UserToken Authenticate(Credentials credentials, bool register) {
			AuthRequest request = new AuthRequest(credentials, register);
			object reply;

			using (TcpClient client = new TcpClient()) {
				client.Connect(serverEndPoint);
				NetworkStream networkStream = client.GetStream();

				Message.Send(networkStream, request);
				reply = Message.Recieve(networkStream);
			}

			if (reply == null)
				throw new Exception("BBRequest Error: Expected reply but recieved none");//Console.Error.WriteLine("BBRequest Error: Expected reply but recieved none");
			else if (!(reply is AuthResponse))
				throw new Exception("BBRequest Error: Expected AuthResponse but recieved unknown response type");
			else
				return (reply as AuthResponse).token;
		}

		/// <summary>
		/// Sends a BBRequest to the server
		/// </summary>
		/// <param name="request">The BBRequest to send</param>
		/// <returns>If the reqeuest expects a response, returns the response</returns>
		public BBResponse SendRequest(BBRequest request) {
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
				return reply as BBResponse;
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
	public class BBRequest {
		public Request requestType { get; private set; }

		/// <summary>
		/// Sends an upload request with a single song and either an upvote or a downvote.
		/// The server will check the database for that song.  If the song exists, it will
		/// add the vote to it, otherwise it will add the song to the server and save it
		/// with the vote.
		/// 
		/// The response will contain a single SongParameters item with the song and it's score
		/// </summary>
		/// <param name="song">Song to upload</param>
		/// <param name="upOrDownvote">Vote. True if an upvote, false otherwise.</param>
		/// <param name="userInfo">The user authentication token</param>
		public BBRequest(SongParameters song, bool upOrDownvote, UserToken userInfo) {
			requestType = new UpDownVote(song, upOrDownvote, userInfo);
		}

		/// <summary>
		/// Sends a request for the score of a single song.
		/// 
		/// The response will contain a single SongParameters item with the song and it's score
		/// </summary>
		/// <param name="song">Song to check the score of</param>
		/// <param name="userInfo">The user authentication token</param>
		public BBRequest(SongParameters song, UserToken userInfo) {
			requestType = new RequestScore(song, userInfo);
		}

		/// <summary>
		/// Sends a request for a list of songs that match the given parameters.
		/// 
		/// The response will contain a list of songs that match the parameters.
		/// </summary>
		/// <param name="parameters">Parameters to match</param>
		/// <param name="numberOfSongs">Number of songs to return</param>
		/// <param name="userInfo">The user authentication token</param>
		public BBRequest(SongParameters parameters, int numberOfSongs, UserToken userInfo) {
			requestType = new RequestSongs(parameters, numberOfSongs, userInfo);
		}

		/// <summary>
		/// Generic request object
		/// </summary>
		[SerializableAttribute]
		public class Request {
			public UserToken userToken { get; private set; }

			protected Request(UserToken userInfo) {
				this.userToken = userInfo;
			}
		}

		/// <summary>
		/// Uploads a song with a vote
		/// </summary>
		[SerializableAttribute]
		public class UpDownVote : Request {
			public SongParameters song { get; private set; }
			public bool vote { get; private set; }

			public UpDownVote(SongParameters song, bool vote, UserToken userInfo) : base(userInfo) {
				this.song = song;
				this.vote = vote;
			}
		}

		/// <summary>
		/// Requests the score of a song
		/// </summary>
		[SerializableAttribute]
		public class RequestScore : Request {
			public SongParameters song { get; private set; }

			public RequestScore(SongParameters song, UserToken userInfo) : base(userInfo) {
				this.song = song;
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
	}

	/// <summary>
	/// Response object for communication between the BlottoBeats server and client.
	/// </summary>
	[SerializableAttribute]
	public class BBResponse {
		public Response responseType { get; private set; }

		public BBResponse() {
			responseType = new AuthFailed();
		}

		public BBResponse(string type, string message) {
			responseType = new Error(type, message);
		}

		public BBResponse(SongParameters song) {
			responseType = new SingleSong(song);
		}

		public BBResponse(List<SongParameters> songs) {
			responseType = new SongList(songs);
		}

		
		/// <summary>
		/// Base Response class
		/// </summary>
		[SerializableAttribute]
		public class Response { }

		/// <summary>
		/// Response for a failed authentication
		/// </summary>
		[SerializableAttribute]
		public class AuthFailed : Response { }

		/// <summary>
		/// Response for an error
		/// </summary>
		[SerializableAttribute]
		public class Error : Response {
			public string type { get; private set; }
			public string message { get; private set; }

			public Error(string type, string message) {
				this.message = message;
			}
		}

		/// <summary>
		/// Responds with a single song
		/// </summary>
		[SerializableAttribute]
		public class SingleSong : Response {
			public SongParameters song { get; private set; }

			public SingleSong(SongParameters song) {
				this.song = song;
			}
		}

		/// <summary>
		/// Responds with a list of songs
		/// </summary>
		[SerializableAttribute]
		public class SongList : Response {
			public List<SongParameters> songs { get; private set; }

			public SongList(List<SongParameters> songs) {
				this.songs = songs;
			}
		}
	}

	/// <summary>
	/// Request from the client to the server to register a new user, or authenticate an existing one.
	/// </summary>
	public class AuthRequest {
		public Credentials credentials { get; private set; }
		public bool register;

		/// <summary>
		/// Sends an authentication request to the server.
		/// </summary>
		/// <param name="credentials">The credentials to use</param>
		/// <param name="register">True if registering a new user</param>
		public AuthRequest(Credentials credentials, bool register) {
			this.credentials = credentials;
			this.register = register;
		}
	}

	/// <summary>
	/// Response from the server with an authentication token, or null if unsuccessful
	/// </summary>
	public class AuthResponse {
		public UserToken token { get; private set; }

		public AuthResponse(UserToken token) {
			this.token = token;
		}
	}
}
