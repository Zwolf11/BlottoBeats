using Networking;	// Contains protocols for communicating with the server
using SongData;		// Contains the definitions for song objects
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace BlottoBeats
{
    public static class Program
    {
        public static void Main()
        {
            // SERVER COMMUNICATION GUIDE
			// How to connect to the server
			string ip = "127.0.0.1";	// Replace with the IP of the server
			int port = 3000;			// Replace with the port of the server
			BBServerConnection server = new BBServerConnection(ip, port);

			// How to test the connection
			if (server.Test()) {
				Console.WriteLine("Connection is valid!");
			} else {
				Console.Error.WriteLine("Connection is not valid!");
			}
			
			// How to upload a song
			Song song = new Song();
			bool upOrDownVote = true;
			BBRequest request = new BBRequest(song, upOrDownVote);
			server.SendRequest(request);

			// How to request a list of songs
			SongParameters parametersToMatch = new SongParameters();
			int numberOfSongs = 5;
			request = new BBRequest(parametersToMatch, numberOfSongs);
			object reply = server.SendRequest(request);
			if (reply is BBRequest.ResponseSongs) {
				List<Song> songlist = ((BBRequest.ResponseSongs)reply).songs;
			} else {
				Console.Error.WriteLine("Reply was of wrong type!");
			}
			// END OF SERVER COMMUNICATION GUIDE

			Application.Run(new MainForm());
        }
    }
}
