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
		public object SendRequest(BBRequest request) {
			object reply;
			
			using (TcpClient client = new TcpClient()) {
				client.Connect(serverEndPoint);
				NetworkStream networkStream = client.GetStream();

				Message.Send(networkStream, request);				
				reply = Message.Recieve(networkStream);
			}

			if (reply == null) Console.Error.WriteLine("BBRequest Error: Expected reply but recieved none");
			return reply;
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
		public Request requestType { get; private set; }

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
		public BBRequest(Song song, bool upOrDownvote) {
			requestType = new UpDownVote(song, upOrDownvote);
		}

		/// <summary>
		/// Sends a request for the score of a single song.
		/// 
		/// The response will contain a single SongAndVoteData item with the song and it's score
		/// </summary>
		/// <param name="song">Song to check the score of</param>
		public BBRequest(Song song) {
			requestType = new RequestScore(song);
		}

		/// <summary>
		/// Sends a request for a list of songs that match the given parameters.
		/// 
		/// The response will contain a list of songs that match the parameters.
		/// </summary>
		/// <param name="parameters">Parameters to match</param>
		/// <param name="numberOfSongs">Number of songs to return</param>
		public BBRequest(SongParameters parameters, int numberOfSongs) {
			requestType = new RequestSongs(parameters, numberOfSongs);
		}

		/// <summary>
		/// Generic request object
		/// </summary>
		[SerializableAttribute]
		public class Request { }

		/// <summary>
		/// Uploads a song with a vote
		/// </summary>
		[SerializableAttribute]
		public class UpDownVote : Request {
			public Song song { get; private set; }
			public bool vote { get; private set; }

			public UpDownVote(Song song, bool vote) {
				this.song = song;
				this.vote = vote;
			}
		}

		/// <summary>
		/// Requests the score of a song
		/// </summary>
		[SerializableAttribute]
		public class RequestScore : Request {
			public Song song { get; private set; }

			public RequestScore(Song song) {
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

			public RequestSongs(SongParameters parameters, int num) {
				this.parameters = parameters;
				this.num = num;
			}
		}

		/// <summary>
		/// Responds with a single song
		/// </summary>
		[SerializableAttribute]
		public class ResponseSong : Request {
			public SongAndVoteData song { get; private set; }

			public ResponseSong(SongAndVoteData song) {
				this.song = song;
			}
		}

		/// <summary>
		/// Responds with a list of songs
		/// </summary>
		[SerializableAttribute]
		public class ResponseSongs : Request {
			public List<SongAndVoteData> songs { get; private set; }

			public ResponseSongs(List<SongAndVoteData> songs) {
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
			SendBytes(stream, Pack(obj));
		}

		/// <summary>
		/// Sends a prepackaged byte array over the specified stream
		/// </summary>
		/// <param name="stream"></param>
		public static void SendBytes(NetworkStream stream, byte[] data) {
			byte[] dataLength = BitConverter.GetBytes((Int32)data.Length);

			if (BitConverter.IsLittleEndian) Array.Reverse(dataLength);

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
			return Unpack(RecieveBytes(stream));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="stream"></param>
		/// <returns></returns>
		public static byte[] RecieveBytes(NetworkStream stream) {
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
					return Pack("Test"); // *shrug* eh, it works
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

					return data;
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

			return (response is string && "Test" == (string)response);
		}

		/// <summary>
		/// Sends a blank message with length header of zero.
		/// </summary>
		public static void TestMsg(NetworkStream stream) {
			byte[] zeros = BitConverter.GetBytes((Int32) 0);
			stream.Write(zeros, 0, sizeof(Int32));
			stream.Flush();
		}

		/// <summary>
		/// Packs an object into a big-endian byte array
		/// </summary>
		/// <param name="obj">Object to pack</param>
		/// <returns>Big-endian byte representation of the object</returns>
		public static byte[] Pack(object obj) {
			byte[] data;

			using (MemoryStream memoryStream = new MemoryStream()) {
				(new BinaryFormatter()).Serialize(memoryStream, obj);
				data = memoryStream.ToArray();
			}

			if (BitConverter.IsLittleEndian) Array.Reverse(data);
			return data;
		}

		/// <summary>
		/// Unpacks a big-endian byte array into an object
		/// </summary>
		/// <param name="arr">Byte array to unpack</param>
		/// <returns>Object that was unpacked</returns>
		public static object Unpack(byte[] data) {
			object obj;

			if (BitConverter.IsLittleEndian) Array.Reverse(data);
			using (MemoryStream memoryStream = new MemoryStream(data)) {
				obj = (new BinaryFormatter()).Deserialize(memoryStream);
			}

			return obj;
		}
	}
}
