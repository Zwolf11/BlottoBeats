using BlottoBeats.Library.Authentication;
using BlottoBeats.Library.SongData;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;


namespace BlottoBeats.Server {
	/// <summary>
	/// Handles all communication with the MySQL database
	/// </summary>
	internal class Database {
		string connString;

		/// <summary>
		/// Loads a database from the given connection string
		/// </summary>
		internal Database(string hostID, int port, string databaseName, string userID, string password) {
			// No sanitization because I like to live on the edge
			this.connString = String.Format("Server={0};Port={1};Database={2};Uid={3};password={4}", hostID, port, databaseName, userID, password);
		}

		/// <summary>
		/// Loads a database from the given connection string
		/// </summary>
		internal Database(string connString) {
			this.connString = connString;
		}

		/// <summary>
		/// Tests the connection to the database
		/// </summary>
		/// <returns>1 if connected, 0 if invalid connection string, -1 if cannot connect to host, -2 if access denied</returns>
		internal int TestConnection() {
			int isConn = 0;
			MySqlConnection conn = null;
			try {
				conn = new MySqlConnection(connString);
				conn.Open();
				isConn = 1;
			} catch (ArgumentException) {
				throw new DatabaseException("Invalid connection string");
			} catch (MySqlException ex) {
				switch (ex.Number) {
					case 1042: // Unable to connect to any of the specified MySQL hosts (Check Server,Port)
						isConn = -1;
						break;
					case 0: // Access denied (Check DB name,username,password)
						isConn = -2;
						break;
					default:
						break;
				}
			} finally {
				conn.Close();
			}
			return isConn;
		}

		/// <summary>
		/// Checks to see if a song is in the database already.  If it isn't, the song is
		/// added to the database with a single vote.  If it is, add the vote to that song.
		/// </summary>
		/// <param name="song">The song object to vote on</param>
		/// <param name="vote">The vote. True if upvote, false if downvote.</param>
		/// <param name="userID">The ID of the user who voted</param>
		internal SongParameters VoteOnSong(SongParameters song, bool vote, int userID) {
			if (song.ID == -1) song.ID = GetID(song);	// Song has no ID.  Search the server
			if (song.ID == -1) {
				// Song is not in the database.  Insert it into the database.
				song.ID = GetNextAvailableID("uploadedsongs");
				song.score = (vote) ? 1 : -1;
				song.userID = userID;
				insertData(song);
                addVoteToUserTable(userID, song.ID, vote);
			} else {
                int voteAmount = checkAndChangeVote(userID, song.ID, vote);
				updateScore(song.ID, voteAmount);

				object score = returnItem(song.ID, "voteScore", "uploadedsongs");
				if (score != null && score is int)
					song.score = (int)score;
				else
					throw new DatabaseException("DATABASE ERROR: Invalid data type returned");
			}

			return song;
		}

		/// <summary>
		/// Checks to see if a song with the given ID exists in the database.
		/// </summary>
		/// <param name="id">The ID to check</param>
		/// <returns>True if the song exists, false otherwise</returns>
		internal bool SongExists(int id) {
			// TODO: We can probably do this better
			object obj = null;
			try {
				obj = returnItem(id, "voteScore", "uploadedsongs");
			} catch (DatabaseException) {
				return false;
			}

			return (obj != null);
		}

		/// <summary>
		/// Returns the song with the given ID
		/// </summary>
		/// <param name="id">The ID of the song to get</param>
		/// <returns>A SongParamteres object that represents the song</returns>
		internal SongParameters GetSong(int id) {
			if (SongExists(id)) {
				
                int score = 0;
                int seed = 0;
                int tempo = 0;
                string genre = "";
                int userID = 0;
                MySqlConnection conn = new MySqlConnection(connString);
                MySqlCommand command = conn.CreateCommand();

                command.CommandText = "Select genre,songseed,tempo,voteScore,idusers from uploadedsongs where iduploadedsongs = " + id;
                try
                {
                    conn.Open();
                    MySqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        genre = (string)reader["genre"];
                        seed = (int)reader["songseed"];
                        tempo = (int)reader["tempo"];
                        score = (int)reader["voteScore"];
                        userID = (int)reader["idusers"];
                    }
                }
                catch (MySqlException ex)
                {
                    throw new DatabaseException("SQL Exception: " + ex.Message, ex);	// Propagate the exception upwards after handling the finally block
                }
                finally
                {
                    conn.Close();
                }

                return new SongParameters((int)id, (int)score, (int)seed, (int)tempo, (string)genre, (int)userID);

			} else {
				return null;
			}
		}

		/// <summary>
		/// Returns the given song with updated ID, score and user data.
		///  Does not add the song to the database if it isn't already there.
		/// </summary>
		/// <param name="song">Song to refresh</param>
		/// <returns>A SongParameters object that represents the song</returns>
		internal SongParameters RefreshSong(SongParameters song) {
			if (song.ID == -1) song.ID = GetID(song);	// Song has no ID.  Search the server
			if (song.ID == -1) {
				// Song is not in the database. Return score of 0 and userID of -1.
				song.score = 0;
				song.userID = -1;
			} else {
				object score = returnItem(song.ID, "voteScore", "uploadedsongs");
				if (score != null && score is int)
					song.score = (int)score;
				else
					throw new DatabaseException("DATABASE ERROR: Invalid data type returned");

				object userID = returnItem(song.ID, "idusers", "uploadedsongs");
				if (userID != null && userID is int)
					song.userID = (int)userID;
				else
					throw new DatabaseException("DATABASE ERROR: Invalid data type returned");
			}

			return song;
		}

		/// <summary>
		/// Gets a list of songs that match the given SongParameters object
		/// </summary>
		/// <param name="numSongs">The number of songs</param>
		/// <param name="seed">(optional) the seed to filter by</param>
		/// <param name="tempo">(optional) the tempo to filter by</param>
		/// <param name="genre">(optional) the genre to filter by</param>
		/// <param name="userID">(optional) the user ID to filter by</param>
		/// <returns></returns>
		internal List<SongParameters> GetSongList(int numSongs, string genre, string username) {
            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();

			if (genre != null) {
				command.CommandText = "Select iduploadedsongs from uploadedsongs where genre like '%" + genre + "%' order by voteScore desc limit " + numSongs;
            } else if (username != null) {
				command.CommandText = "Select iduploadedsongs from uploadedsongs where idusers like '%" + GetID(username) + "%' order by voteScore desc limit " + numSongs;
			} else {
				command.CommandText = "Select iduploadedsongs from uploadedsongs order by voteScore desc limit " + numSongs;
            }
            
            int score = 0;
           
            int[] idArray = new int[numSongs];
            int i = -1;
            
			try {
				conn.Open();
				MySqlDataReader reader = command.ExecuteReader();
				while (reader.Read()) {
					i++;
					score = (int)reader["iduploadedsongs"];
					idArray[i] = score;
				}
			} catch (MySqlException ex) {
				throw new DatabaseException("SQL Exception: " + ex.Message, ex);	// Propagate the exception upwards after handling the finally block
			} finally {
				conn.Close();
			}

            List<SongParameters> list = new List<SongParameters>();
            
            for (int j = 0; j<numSongs; j++) {
                
                int tempId = idArray[j];
                if (tempId == 0)
                {
                    break;
                }
				
				SongParameters song = GetSong(tempId);
                
                list.Add(song);
                
            }

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
				if (userID == 0)
					return null;

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
		/// Refreshes the user token of a user, effectively forcing them to login again.
		/// </summary>
		/// <param name="username">The user to refresh the token of</param>
		internal bool RefreshToken(string username) {
			int userID = GetID(username);
			if (userID == 0) return false;

			DateTime expiry = UserToken.GetExpiration();
			string token = UserToken.GenerateToken();

			storeUserToken(userID, expiry, token);
			return true;
		}

		/// <summary>
		/// Verifies a user token
		/// </summary>
		/// <param name="tokenToVerify">The token to verify</param>
		/// <returns>The ID of the user if authentication was successful, 0 otherwise</returns>
		internal int VerifyToken(UserToken tokenToVerify) {
			int userID = GetID(tokenToVerify.username);

			if (userID != 0 && getUserToken(userID).Verify(tokenToVerify))
				return userID;
			else
				return 0;
		}

		/// <summary>
		/// Returns the username of a user with the given ID
		/// </summary>
		/// <param name="id">User id to get the username of</param>
		/// <returns>The username of the user</returns>
		internal string GetUsername(int id) {
			return returnItem(id, "username", "users") as string;
		}

		/// <summary>
		/// Searches the database for a song that matches the given song.
		/// If there is a match, returns the ID.  If not, returns -1
		/// </summary>
		/// <param name="song">Song to search the database for</param>
		/// <returns>ID of the song on the server</returns>
		internal int GetID(SongParameters song) {
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
			} catch (MySqlException ex) {
				throw new DatabaseException("SQL Exception: " + ex.Message, ex);	// Propagate the exception upwards after handling the finally block
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
        internal int GetID(string username)
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
			} catch (MySqlException ex) {
				throw new DatabaseException("SQL Exception: " + ex.Message, ex);	// Propagate the exception upwards after handling the finally block
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
			SQLNonQuery(conn, "Insert into uploadedsongs (iduploadedsongs,genre,songseed,tempo,voteScore, idusers) values('" + song.ID + "','" + song.genre + "','" + song.seed + "','" + song.tempo + "','" + song.score + "','" + song.userID + "')");
		}

        private void addVoteToUserTable(int userID, int songID, bool vote)
        {
            string userTable = "user" + userID;
            int voteNum;
            if (vote == true)
            {
                voteNum = 1;
            }
            else
            {
                voteNum = -1;
            }

            MySqlConnection conn = new MySqlConnection(connString);
            MySqlCommand command = conn.CreateCommand();
            SQLNonQuery(conn, "Insert into user" + userID + " (iduser"+userID+",songID,voteUpOrDown) values('" + GetNextAvailableID(userTable) + "','" + songID + "','" + voteNum + "')");

        }

        private int checkAndChangeVote(int userID, int songID, bool vote)
        {
            int currentVote;
           

            MySqlConnection connect = new MySqlConnection(connString);
            MySqlCommand command = connect.CreateCommand();
            command.CommandText = "Select iduser" + userID + " from user" + userID + " where songID like '%" + songID + "%'";
            int returnId = 0;

            try
            {
                connect.Open();
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    returnId = (int)reader["iduser"+userID];
                }
            }
            catch (MySqlException ex)
            {
                throw new DatabaseException("SQL Exception: " + ex.Message, ex);	// Propagate the exception upwards after handling the finally block
            }
            finally
            {
                connect.Close();
            }

            string userTable = "user" + userID;
            if (returnItem(returnId, "voteUpOrDown", userTable) == null)
            {
                addVoteToUserTable(userID, songID, vote);
                if (vote == true)
                {
                    return 1;
                }
                else
                {
                    return -1;
                }
            }
            else
            {
               currentVote = (int)returnItem(returnId, "voteUpOrDown", userTable);
            }
            

            if ((currentVote == 1) && (vote == true)) {
                return 0;       //user already upvoted
            }
            else if ((currentVote == 1) && (vote == false))     //user changes upvote to downvote, returns true to show that vote was changed
            {
                MySqlConnection conn = new MySqlConnection(connString);
                SQLNonQuery(conn, "Update " + userTable + " SET voteUpOrDown= '-1' WHERE songID='" + songID + "'");
                return -2;
            }
            else if ((currentVote == -1) && (vote == false))
            {
                return 0;      //user already downvoted
                
            }
            else if ((currentVote == -1) && (vote == true))     //user changes downvote to upvote, returns true to show that vote was changed
            {
                MySqlConnection conn = new MySqlConnection(connString);
                SQLNonQuery(conn, "Update " + userTable + " SET voteUpOrDown= '1' WHERE songID='" + songID + "'");
                return 2;
            }
            else
            {
				addVoteToUserTable(userID, songID, vote);
				return vote ? 1 : -1;
            }

            
        }

		private void updateScore(int id, int voteAmount) {
			MySqlConnection conn = new MySqlConnection(connString);
			MySqlCommand command = conn.CreateCommand();
			int scoreUpdate = (int)(returnItem(id, "voteScore", "uploadedsongs"));

			scoreUpdate += voteAmount;

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
			} catch (MySqlException ex) {
				throw new DatabaseException("SQL Exception: " + ex.Message, ex);	// Propagate the exception upwards after handling the finally block
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
                SQLNonQuery(conn, "Insert into users (idusers,username,passwordHash,tokenExpire,tokenStr) values('" + nextId + "','" + username + "','" + hash + "','" + date + "','" + token + "')");
                createUserTable(nextId);
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
            SQLNonQuery(conn, "Update users SET tokenExpire='" + strDate + "' WHERE idusers='" + userID + "'");
			SQLNonQuery(conn, "Update users SET tokenStr='" + token + "' WHERE idusers='" + userID + "'");
		}

		private UserToken getUserToken(int userID) {
			string username = (string)returnItem(userID, "username", "users");
			DateTime expires = (DateTime)returnItem(userID, "tokenExpire", "users");
			string token = (string)returnItem(userID, "tokenStr", "users");
			return new UserToken(username, expires, token);
		}

        internal void deleteUser(int userID)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            SQLNonQuery(conn, "Delete from users where idusers = " + userID);
            SQLNonQuery(conn, "Drop table user" + userID);
        }

        internal void deleteSong(int songID)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            SQLNonQuery(conn, "Delete from uploadedsongs where iduploadedsongs = " + songID);
        }

        internal void changePassword(int userID, string hash)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            SQLNonQuery(conn, "Update users set passwordHash = '" + hash + "' where idusers = " + userID);
            SQLNonQuery(conn, "Update users set tokenStr = ' ' where idusers = " + userID);
        }

        internal void changeVoteScore(int songID, int newScore)
        {
            MySqlConnection conn = new MySqlConnection(connString);
            SQLNonQuery(conn, "Update uploadedsongs set voteScore = " + newScore + " where iduploadedsongs = " + songID);
        }

        private void createUserTable(int userID)
        {
            string tableName = "user" + userID;
           
            MySqlConnection conn = new MySqlConnection(connString);
            SQLNonQuery(conn, "Create table " + tableName +  " (id" + tableName + " INT NOT NULL, songID INT, voteUpOrDown INT, PRIMARY KEY(id" + tableName + "))");
        }

		/// <summary>
		/// Performs an SQL NonQuery to the database
		/// </summary>
		/// <param name="conn">the connection SQL connection</param>
		/// <param name="comString">the command string</param>
		internal void SQLNonQuery(MySqlConnection conn, string comString) {
            MySqlCommand command = conn.CreateCommand();
            command.CommandText = comString;
			try {
				conn.Open();
				command.ExecuteNonQuery();
			} catch (MySqlException ex) {
				throw new DatabaseException("SQL Exception: " + ex.Message, ex);	// Propagate the exception upwards after handling the finally block
			} finally {
				conn.Close();
			}
		}
	}


	[SerializableAttribute]
	public class DatabaseException : Exception {
		public DatabaseException() : base() { }
		public DatabaseException(string message) : base(message) { }
		public DatabaseException(string message, Exception inner) : base(message, inner) { }

		// A constructor is needed for serialization when an 
		// exception propagates from a remoting server to the client.  
		protected DatabaseException(SerializationInfo info, StreamingContext context) { }
	}
}
