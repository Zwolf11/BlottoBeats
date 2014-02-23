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
            Application.Run(new MainForm());

			/* SERVER COMMUNICATION GUIDE
			// How to connect to the server
			string ip = "127.0.0.1";	// Replace with the IP of the server
			int port = 3000;			// Replace with the port of the server
			BBServerConnection server = new BBServerConnection(ip, port);

			// How to test the connection
			if (server.Test()) {
				Console.WriteLine("Connection is valid!");
			} else {
				Console.WriteLine("Connection is not valid!");
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
			List<Song> songlist = (List<Song>)server.SendRequest(request);
			*/
        }
    }
}
