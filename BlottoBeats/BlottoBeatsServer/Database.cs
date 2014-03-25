using MySql.Data.MySqlClient;
using Networking;
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
		internal SongParameters VoteOnSong(SongParameters song, bool vote) {
			if (song.ID == -1) song.ID = GetID(song);	// Song has no ID.  Search the server
			if (song.ID == -1) {
				// Song is not in the database.  Insert it into the database.
				song.ID = nextID;
				song.score = (vote) ? 1 : -1;
				insertData(song);
				nextID = GetNextAvailableID(nextID);
			} else {
				updateScore(song.ID, vote);
				object score = returnItem(song.ID, "voteScore");
				if (score != null && score is int)
					song.score = (int)score;
				else
					throw new Exception("Database Error: Invalid data type returned");
			}

			return song;
		}

		/// <summary>
		/// Returns the score of the given song.  If the song isn't in the database,
		/// returns zero.  Does not add the song to the database if it isn't already there.
		/// </summary>
		/// <param name="song">Song to check the score of</param>
		/// <returns>A SongAndVoteData item containing the song and it's score</returns>
		internal SongParameters GetSongScore(SongParameters song) {
			if (song.ID == -1) song.ID = GetID(song);	// Song has no ID.  Search the server
			if (song.ID == -1) {
				// Song is not in the database. Return score of 0.
				song.score = 0;
			} else {
				object score = returnItem(song.ID, "voteScore");
				if (score != null && score is int)
					song.score = (int)score;
				else
					throw new Exception("Database Error: Invalid data type returned");
			}

			return song;
		}

		/// <summary>
		/// Gets a list of songs that match the given SongParameters object
		/// </summary>
		/// <param name="songParameters">The parameters to match</param>
		/// <param name="numSongs">The maximum number of songs to return</param>
		/// <returns>The list of songs</returns>
		internal List<SongParameters> GetSongList(SongParameters songParameters, int numSongs) {
			List<SongParameters> list = new List<SongParameters>();

			// TODO: Implement

			return list;
		}


		/// <summary>
		/// Attemts to register a new user or authenticate an existing user with the given credentials
		/// </summary>
		/// <param name="credentials">User credentials to authenticate</param>
		/// <param name="register">True if registering a new user, false otherwise</param>
		/// <returns>UserToken if successful, null otherwise</returns>
		internal UserToken Authenticate(Credentials credentials, bool register) {
			int userID;
			if (register) {
				// Register a new user
				if (createUser(credentials.username, credentials.GenerateHash()))
					userID = getUserID(credentials.username); // User was created
				else
					return null; // User was not created
			} else {
				userID = getUserID(credentials.username);
				string hash = getUserHash(userID);

				if (hash != null && !credentials.Verify(hash))
					return null; // Credentials were invalid
			}

			// Generate a new authentication token.
			DateTime expiry = UserToken.GetExpiration();
			string token = UserToken.GenerateToken();

			storeUserToken(userID, expiry, token);
			return new UserToken(credentials.username, expiry, token);
		}

		/// <summary>
		/// Verifies a user token
		/// </summary>
		/// <param name="tokenToVerify">The token to verify</param>
		/// <returns>The ID of the user if authentication was successful, 0 otherwise</returns>
		internal int VerifyToken(UserToken tokenToVerify) {
			int userID = getUserID(tokenToVerify.username);
			UserToken userToken = getUserToken(userID);

			if (userToken.Verify(tokenToVerify))
				return userID;
			else
				return 0;
		}

		/// <summary>
		/// Searches the database for a song that matches the given song.
		/// If there is a match, returns the ID.  If not, returns -1
		/// </summary>
		/// <param name="song">Song to search the database for</param>
		/// <returns>ID of the song on the server</returns>
		private int GetID(SongParameters song) {
			MySqlConnection conn = new MySqlConnection(connString);
			MySqlCommand command = conn.CreateCommand();
			command.CommandText = "Select iduploadedsongs from uploadedsongs where genre like '%" + song.genre + "%' and songseed like '%" + song.seed + "%' and tempo like '%" + song.tempo + "%'";
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

		private void insertData(SongParameters song) {
			MySqlConnection conn = new MySqlConnection(connString);
			MySqlCommand command = conn.CreateCommand();
			command.CommandText = "Insert into uploadedsongs (iduploadedsongs,genre,songseed,tempo,voteScore) values('" + song.ID + "','" + song.genre + "','" + song.seed + "','" + song.tempo + "','" + song.score + "')";

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

		private bool createUser(string username, string hash) {
			// TODO: Joe, write this method
			// The user needs these fields:
			//   User ID (int)
			//   Username (string)
			//   Password Hash (string)
			//   Token Expiry Date (DateTime object - can be converted to a string or whatever, as long as it's recoverable)
			//   Token String (string)
			// The token expiry date and string should both start null

			// This function should check to see if the username is already in use.
			// If so, it should not create a new entry in the table, and the function should return false.
			// If the username is not already in use, it should create a new entry in the user table and return true.
			return false;
		}

		private int getUserID(string username) {
			// TODO: Joe, write this method
			// This function should return the ID of a user based on the username if they exist in the table, and 0 otherwise.

			return 0;
		}

		private string getUserHash(int userID) {
			// TODO: Joe, write this method
			// This function should get the password hash of the specified user.

			return null;
		}

		private void storeUserToken(int userID, DateTime expires, string token) {
			// TODO: Joe, write this method
			// This function should store both the expires date and the token string in the specified user's entry
		}

		private UserToken getUserToken(int userID) {
			// TODO: Joe, write this method
			// This function should return a UserToken object with the username, expiry date, and token string initialized with the values in the user's entry

			string username = null;
			DateTime expires = new DateTime();
			string token = null;
			return new UserToken(username, expires, token);
		}
	}
}
