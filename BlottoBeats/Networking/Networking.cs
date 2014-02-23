using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using SongData;

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
		/// Sends a BBRequest to the server
		/// </summary>
		/// <param name="request">The BBRequest to send</param>
		/// <returns>If the reqeuest expects a response, returns the response</returns>
		public BBRequest.Request SendRequest(BBRequest request) {
			BBRequest.Request response = null;
			
			using (TcpClient client = new TcpClient()) {
				client.Connect(serverEndPoint);
				NetworkStream networkStream = client.GetStream();
				
				Message.Send(networkStream, request);
				if (request.ExpectsResponse()) {
					object reply = Message.Recieve(networkStream);

					if (reply == null) {
						Console.Error.WriteLine("BBRequest Error: Expected reply but recieved none");
					} else if (!(reply is BBRequest)) {
						Console.Error.WriteLine("BBRequest Error: Reply is of unknown object type '{0}'", reply.GetType().ToString());
					} else {
						response = ((BBRequest)reply).GetRequest();
					}
				}
			}

			return response;
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
	/// Request object for communication between the BlottoBeats server and client.
	/// </summary>
	[SerializableAttribute]
	public class BBRequest {
		private Request request;
		private bool expectsResponse;

		/// <summary>
		/// Sends an upload request with a single song and either an upvote or a downvote.
		/// The server will check the database for that song.  If the song exists, it will
		/// add the vote to it, otherwise it will add the song to the server and save it
		/// with the vote.
		/// </summary>
		/// <param name="song">Song to upload</param>
		/// <param name="upOrDownvote">Vote. True if an upvote, false otherwise.</param>
		public BBRequest(Song song, bool upOrDownvote) {
			request = new UpDownVote(song, upOrDownvote);
			expectsResponse = false;
		}

		/// <summary>
		/// Requests a list of songs that match the given parameters.
		/// </summary>
		/// <param name="parameters">Parameters to match</param>
		/// <param name="numberOfSongs">Number of songs to return</param>
		public BBRequest(SongParameters parameters, int numberOfSongs) {
			request = new RequestSongs(parameters, numberOfSongs);
			expectsResponse = true;
		}

		/// <summary>
		/// Sends a response to a REQUESTSONGS request containing the songs
		/// that were requested.
		/// </summary>
		/// <param name="songs"></param>
		public BBRequest(List<Song> songs) {
			request = new ResponseSongs(songs);
			expectsResponse = false;
		}
		
		/// <summary>
		/// Whether the request expects a response
		/// </summary>
		/// <returns></returns>
		public bool ExpectsResponse() {
			return expectsResponse;
		}

		/// <summary>
		/// Get the request object
		/// </summary>
		/// <returns>The requested object</returns>
		public Request GetRequest() {
			return request;
		}

		/// <summary>
		/// Generic request object
		/// </summary>
		[SerializableAttribute]
		public class Request { }

		/// <summary>
		/// Upload a song
		/// </summary>
		[SerializableAttribute]
		public class UpDownVote : Request {
			public Song song;
			public bool vote;

			public UpDownVote(Song song, bool vote) {
				this.song = song;
				this.vote = vote;
			}
		}

		/// <summary>
		/// Request a list of songs
		/// </summary>
		[SerializableAttribute]
		public class RequestSongs : Request {
			public SongParameters parameters;
			public int num;

			public RequestSongs(SongParameters parameters, int num) {
				this.parameters = parameters;
				this.num = num;
			}
		}

		/// <summary>
		/// Responds with a list of songs
		/// </summary>
		[SerializableAttribute]
		public class ResponseSongs : Request {
			public List<Song> songs;

			public ResponseSongs(List<Song> songs) {
				this.songs = songs;
			}
		}
	}

	/// <summary>
	/// Message helper class for to clog the tubes with.
	/// Use Message.Send() to send a message and Message.Receive() to receive it.
	/// Used to send and receive objects directly.
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
				

				if (dataLength == 0) {
					// A test message was sent.
					return "Test";
				} else {
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
				}
			} catch (Exception e) {
				Console.Error.WriteLine("A socket error has occured: " + e.ToString());
				return null;
			}
		}

		/// <summary>
		/// Tests a connection to see if it is valid
		/// </summary>
		/// <param name="stream">Stream to test</param>
		/// <returns>True if the connection is valid, false otherwise</returns>
		public static bool Test(NetworkStream stream) {
			Message.TestMsg(stream);
			object response = Message.Recieve(stream);

			return (response is string && response == "Test");
		}

		/// <summary>
		/// Sends a blank message with length header of zero.
		/// </summary>
		public static void TestMsg(NetworkStream stream) {
			byte[] zeros = BitConverter.GetBytes((Int32) 0);
			stream.Write(zeros, 0, sizeof(Int32));
			stream.Flush();
		}
	}
}
