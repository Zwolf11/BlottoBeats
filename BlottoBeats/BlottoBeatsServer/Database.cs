using MySql.Data.MySqlClient;
using SongData;
using System;
using System.Collections.Generic;

namespace BlottoBeatsServer {
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
		internal CompleteSongData VoteOnSong(int seed, SongParameters song, bool vote) {
			if (song.ID == -1) song.ID = GetID(seed, song);	// Song has no ID.  Search the server
			if (song.ID == -1) {
				// Song is not in the database.  Insert it into the database.
				song.ID = nextID;
				nextID = GetNextAvailableID(nextID);
				insertData(seed, song, (vote) ? 1 : -1);
				return new CompleteSongData(seed, song, (vote) ? 1 : -1);
			} else {
				updateScore(song.ID, vote);
				int score = (int)returnItem(song.ID, "voteScore");
				return new CompleteSongData(seed, song, score);
			}
		}

		/// <summary>
		/// Returns the score of the given song.  If the song isn't in the database,
		/// returns zero.  Does not add the song to the database if it isn't already there.
		/// </summary>
		/// <param name="song">Song to check the score of</param>
		/// <returns>A SongAndVoteData item containing the song and it's score</returns>
		internal CompleteSongData GetSongScore(int seed, SongParameters song) {
			if (song.ID == -1) song.ID = GetID(seed, song);	// Song has no ID.  Search the server
			if (song.ID == -1) {
				// Song is not in the database. Return score of 0.
				return new CompleteSongData(seed, song, 0);
			} else {
				object score = returnItem(song.ID, "voteScore");
				if (score != null && score is int)
					return new CompleteSongData(seed, song, (int)score);
				else
					return new CompleteSongData(seed, song, 0);
			}
		}

		/// <summary>
		/// Gets a list of songs that match the given SongParameters object
		/// </summary>
		/// <param name="songParameters">The parameters to match</param>
		/// <param name="numSongs">The maximum number of songs to return</param>
		/// <returns>The list of songs</returns>
		internal List<CompleteSongData> GetSongList(SongParameters songParameters, int numSongs) {
			List<CompleteSongData> list = new List<CompleteSongData>();

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
			try {
				conn.Open();
				MySqlDataReader reader = command.ExecuteReader();
				while (reader.Read()) {
					returnId = (int)reader["iduploadedsongs"];
				}
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			} finally {
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

			while (returnItem(testId, "iduploadedsongs") != null) {
				testId += 1;
			}

			return testId;
		}

		private void insertData(int seed, SongParameters song, int score) {
			MySqlConnection conn = new MySqlConnection(connString);
			MySqlCommand command = conn.CreateCommand();
			command.CommandText = "Insert into uploadedsongs (iduploadedsongs,genre,songseed,tempo,voteScore) values('" + song.ID + "','" + song.genre + "','" + seed + "','" + song.tempo + "','" + score + "')";

			try {
				conn.Open();
				command.ExecuteNonQuery();
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			} finally {
				conn.Close();
			}

		}

		private void updateScore(int id, bool vote) {
			MySqlConnection conn = new MySqlConnection(connString);
			MySqlCommand command = conn.CreateCommand();
			int scoreUpdate = (int)(returnItem(id, "voteScore"));

			if (vote == true) {
				scoreUpdate += 1;
			} else {
				scoreUpdate -= 1;
			}

			command.CommandText = "Update uploadedsongs SET voteScore='" + scoreUpdate + "' WHERE iduploadedsongs='" + id + "'";
			try {
				conn.Open();
				command.ExecuteNonQuery();
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			} finally {
				conn.Close();
			}
		}

		private object returnItem(int id, string col) {
			MySqlConnection conn = new MySqlConnection(connString);
			MySqlCommand command = conn.CreateCommand();
			command.CommandText = "Select " + col + " from uploadedsongs where iduploadedsongs=" + id;
			object item = null;

			try {
				conn.Open();
				MySqlDataReader reader = command.ExecuteReader();
				while (reader.Read()) {
					item = reader[col];
				}
			} catch (Exception ex) {
				Console.Error.WriteLine(ex.Message);
			} finally {
				conn.Close();
			}

			return item;
		}
	}
}
