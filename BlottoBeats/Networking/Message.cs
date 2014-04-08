using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;

namespace BlottoBeats.Library.Networking {
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
			byte[] data = Pack(obj);
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

					return Unpack(data);
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
			byte[] zeros = BitConverter.GetBytes((Int32)0);
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
