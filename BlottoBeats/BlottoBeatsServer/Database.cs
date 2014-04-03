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

		/// <summary>
		/// Loads a database from the given path
		/// </summary>
		internal Database(string connString) {
			this.connString = connString;
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
				song.ID = GetNextAvailableID("songs");
				song.score = (vote) ? 1 : -1;
				insertData(song);
			} else {
				updateScore(song.ID, vote);
				object score = returnItem(song.ID, "voteScore", "songs");
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
				object score = returnItem(song.ID, "voteScore", "songs");
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
					userID = GetID(credentials.username); // User was created
				else
					return null; // User was not created
			} else {
				userID = GetID(credentials.username);
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
			int userID = GetID(tokenToVerify.username);
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
				throw ex;	// Propagate the exception upwards after handling the finally block
			} finally {
				conn.Close();
			}
			
			return returnId;
		}

        /// <summary>
		/// Returns id of an item from the users table
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        private int GetID(string username)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = "Select idusers from users where username like '%" + username + "%'";
            int returnId = 0;

			try {
				conn.Open();
				MySqlDataReader reader = command.ExecuteReader();
				while (reader.Read())
				{
					returnId = (int)reader["idusers"];
				}
			} catch (Exception ex) {
				throw ex;	// Propagate the exception upwards after handling the finally block
			} finally {
				conn.Close();
			}

            return returnId;
        }


		/// <summary>
		/// Gets the next available ID for the server
		/// </summary>
		/// <returns>ID</returns>
		private int GetNextAvailableID(string table) {
			int testId = 1;

			while (returnItem(testId, "id"+table, table) != null) {
				testId += 1;
			}

			return testId;
		}

		private void insertData(SongParameters song) {
			MySqlConnection conn = new MySqlConnection(connString);
			MySqlCommand command = conn.CreateCommand();
			SQLNonQuery(conn, "Insert into uploadedsongs (iduploadedsongs,genre,songseed,tempo,voteScore) values('" + song.ID + "','" + song.genre + "','" + song.seed + "','" + song.tempo + "','" + song.score + "')");
		}

		private void updateScore(int id, bool vote) {
			MySqlConnection conn = new MySqlConnection(connString);
			MySqlCommand command = conn.CreateCommand();
			int scoreUpdate = (int)(returnItem(id, "voteScore", "iduploadedsongs"));

			if (vote == true) {
				scoreUpdate += 1;
			} else {
				scoreUpdate -= 1;
			}

			SQLNonQuery(conn, "Update uploadedsongs SET voteScore='" + scoreUpdate + "' WHERE iduploadedsongs='" + id + "'");
		}

		private object returnItem(int id, string col, string table) {
			MySqlConnection conn = new MySqlConnection(connString);
			MySqlCommand command = conn.CreateCommand();
			command.CommandText = "Select " + col + " from " + table + " where id" + table + "=" + id;
			object item = null;

			try {
				conn.Open();
				MySqlDataReader reader = command.ExecuteReader();
				while (reader.Read()) {
					item = reader[col];
				}
			} catch (Exception ex) {
				throw ex;	// Propagate the exception upwards after handling the finally block
			} finally {
				conn.Close();
			}

			return item;
		}

		private bool createUser(string username, string hash) {			
			// The token expiry date and string should both start null

			// This function should check to see if the username is already in use.
			// If so, it should not create a new entry in the table, and the function should return false.
			// If the username is not already in use, it should create a new entry in the user table and return true.
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();

            if (GetID(username) == 0)
            {
                int nextId = GetNextAvailableID("users");
                string token = null;
                String date = "1000-01-01 00:00:00";
                Console.WriteLine(nextId);
                SQLNonQuery(conn, "Insert into users (idusers,username,passwordHash,tokenExpire,tokenStr) values('" + nextId + "','" + username + "','" + hash + "','" + date + "','" + token + "')");
                return true;
            }

            return false;
		}

		private string getUserHash(int userID) {
            return (string)returnItem(userID, "passwordHash", "users");
		}

		private void storeUserToken(int userID, DateTime expires, string token) {
            const string FMT = "yyyy-MM-dd HH:mm:ss";
            string strDate = expires.ToString(FMT);
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
            SQLNonQuery(conn, "Update users SET tokenExpire='" + strDate + "' WHERE idusers='" + userID + "'");
			SQLNonQuery(conn, "Update users SET tokenStr='" + token + "' WHERE idusers='" + userID + "'");
		}

		private UserToken getUserToken(int userID) {
			string username = (string)returnItem(userID, "username", "users");
			DateTime expires = DateTime.Parse((string)returnItem(userID, "tokenExpire", "users"));
			string token = (string)returnItem(userID, "tokenStr", "users");
			return new UserToken(username, expires, token);
		}

		private void SQLNonQuery(MySqlConnection conn, string comString) {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = comString;
			try {
				conn.Open();
				command.ExecuteNonQuery();
			} catch (Exception ex) {
				throw ex;	// Propagate the exception upwards after handling the finally block
			} finally {
				conn.Close();
			}
		}
	}
}
