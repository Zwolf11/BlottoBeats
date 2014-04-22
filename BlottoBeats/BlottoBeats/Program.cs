using System;
using System.Windows.Forms;

namespace BlottoBeats.Client
{
    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            //Generator Debug Code
           /*SongParameters input = new SongParameters(-1, 100, "Nope");
            int seed = -100000;
            Generator.Generator.generate(seed, input);*/

            /*// SERVER COMMUNICATION GUIDE
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
			int seed = 9001;
			SongParameters song = new SongParameters();
			bool upOrDownVote = true;

			BBRequest request = new BBRequest(seed, song, upOrDownVote);
			object reply = server.SendRequest(request);
			if (reply is SongAndVoteData) {
				SongAndVoteData songScore = (SongAndVoteData)reply;
				Console.WriteLine("Vote Successful. Song score is {0}", songScore.score);
			} else {
				Console.Error.WriteLine("Reply was of wrong type!");
			}

			// How to check the score of a song
			request = new BBRequest(seed, song);
			reply = server.SendRequest(request);
			if (reply is SongAndVoteData) {
				SongAndVoteData songScore = (SongAndVoteData)reply;
				Console.WriteLine("Song score is {0}", songScore.score);
			} else {
				Console.Error.WriteLine("Reply was of wrong type!");
			}

			// How to request a list of songs
			SongParameters parametersToMatch = new SongParameters();
			int numberOfSongs = 5;
			request = new BBRequest(parametersToMatch, numberOfSongs);
			reply = server.SendRequest(request);
			if (reply is List<SongAndVoteData>) {
				List<SongAndVoteData> songList = (List<SongAndVoteData>)reply;
				Console.WriteLine("Request Successful. Returned {0} songs.", songList.Count);
			} else {
				Console.Error.WriteLine("Reply was of wrong type!");
			}
			// END OF SERVER COMMUNICATION GUIDE*/

			Application.Run(new MainForm());
        }
    }
}
