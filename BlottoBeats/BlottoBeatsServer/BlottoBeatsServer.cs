using BlottoBeats.Library.Authentication;
using BlottoBeats.Library.Networking;
using BlottoBeats.Library.SongData;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace BlottoBeats.Server {
	/// <summary>
	/// Basic TCP Server
	/// Uses ThreadPool to handle multiple connections at the same time.
	/// </summary>
	internal class Server {
		private TcpListener tcpListener;
		private Thread listenThread;
		
		private Database database;
		private string logFileLocation;
		private int logLevel;	// How verbose the logs should be.  Higher is more verbose.

		/// <summary>
		/// Starts up the server
		/// </summary>
		public static void Main() {
			// Create a new server that listens on port 3000;
			//Database database = new Database("Server=localhost;Port=3306;Database=songdatabase;Uid=root;password=joeswanson;");
			//Database database = new Database("Server=68.234.183.70;Port=3001;Database=songdatabase;Uid=BlottoServer;password=JJLrDtcrfvjym8gh1zUVklF19KDf1CTM;");
			//Database database = new Database("Server=BlottoBeats.db.11772669.hostedresource.com;Port=3306;Database=BlottoBeats;Uid=BlottoBeats;password=JoeSwanson307!;");
			//Server server = new Server(3000, database, "server.log", 3);
			Database database = null;
			Server server = null;

			// Server startup screen
			Console.WriteLine("---------------------------------------------");
			Console.WriteLine("      BlottoBeats Server v1.0 (Stopped)");
			Console.WriteLine("---------------------------------------------");
			Console.WriteLine("Host ID: " + Properties.Settings.Default.hostID);
			Console.WriteLine("Port:    " + Properties.Settings.Default.port);
			Console.WriteLine();
			Console.WriteLine("Database Name: " + Properties.Settings.Default.databaseName);
			Console.WriteLine();
			Console.WriteLine("Username: " + Properties.Settings.Default.userID);
			Console.WriteLine("---------------------------------------------");
			Console.WriteLine("Type upstart <password> to log into the database and start the server.");
			Console.WriteLine("Type help or ? for more commands.");

			while (true) {
				CommandLine line = CommandLine.Prompt();

				switch (line.command.ToLower()) {
					// SERVER COMMANDS
					case "start":
						if (server != null && server.IsAlive())
							Console.WriteLine("ERROR: Can't start the server, the server is already started");
						else if (database == null || server == null)
							Console.WriteLine("ERROR: The update command needs to be run first");
						else
							server.Start();
						break;

					case "stop":
						if (server == null || !server.IsAlive())
							Console.WriteLine("ERROR: Can't stop the server, the server is already stopped");
						else
							server.Stop();
						break;

					case "restart":
						if (server == null || !server.IsAlive())
							Console.WriteLine("ERROR: Can't restart the server, the server is stopped");
						else
							server.Restart();
						break;

					case "update":
					case "updatedb":
					case "updatedatabase":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else if (server != null && server.IsAlive()) {
							Console.WriteLine("ERROR: Can't update the database if the server is running.  Stop the server first.");
						} else {
							database = new Database(Properties.Settings.Default.hostID,
								Properties.Settings.Default.port,
								Properties.Settings.Default.databaseName,
								Properties.Settings.Default.userID,
								line.args[0]);
							server = new Server(3000, database, "server.log", 3);
							Console.WriteLine("Database updated successfully.");
						}
						break;

					case "upstart":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else if (server != null && server.IsAlive()) {
							Console.WriteLine("ERROR: Can't update the database if the server is running.  Stop the server first.");
						} else {
							database = new Database(Properties.Settings.Default.hostID,
								Properties.Settings.Default.port,
								Properties.Settings.Default.databaseName,
								Properties.Settings.Default.userID,
								line.args[0]);
							server = new Server(3000, database, "server.log", 3);
							Console.WriteLine("Database updated successfully.");

							server.Start();
						}
						break;

					// DATABASE COMMANDS
					case "dbhost":
					case "dbhostid":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else {
							Properties.Settings.Default.hostID = line.args[0];
							Properties.Settings.Default.Save();
							Console.WriteLine("Setting is updated.  You need to run the update command in order for the changes to be applied to the server.");
						}
						break;

					case "dbport":
						int port;
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else if (int.TryParse(line.args[0], out port) && port > 1024 && port <= 65535) {
							Properties.Settings.Default.port = port;
							Properties.Settings.Default.Save();
							Console.WriteLine("Setting is updated.  You need to run the update command in order for the changes to be applied to the server.");
						} else {
							Console.WriteLine("ERROR: The argument must be an integer between 1024 and 65535");
						}
						break;

					case "database":
					case "databasename":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else {
							Properties.Settings.Default.databaseName = line.args[0];
							Properties.Settings.Default.Save();
							Console.WriteLine("Setting is updated.  You need to run the update command in order for the changes to be applied to the server.");
						}
						break;

					case "dbuser":
					case "dbuserid":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else {
							Properties.Settings.Default.userID = line.args[0];
							Properties.Settings.Default.Save();
							Console.WriteLine("Setting is updated.  You need to run the update command in order for the changes to be applied to the server.");
						}
						break;

					case "dbinfo":
						string health = "Stopped";
						if (server != null && server.IsAlive()) health = "Running";
						
						Console.WriteLine("---------------------------------------------");
						Console.WriteLine("      BlottoBeats Server v1.0 (" + health + ")");
						Console.WriteLine("---------------------------------------------");
						Console.WriteLine("Host ID: " + Properties.Settings.Default.hostID);
						Console.WriteLine("Port:    " + Properties.Settings.Default.port);
						Console.WriteLine();
						Console.WriteLine("Database Name: " + Properties.Settings.Default.databaseName);
						Console.WriteLine();
						Console.WriteLine("Username: " + Properties.Settings.Default.userID);
						Console.WriteLine("---------------------------------------------");
						break;

					// USER ACCOUNT COMMANDS
					case "newaccount":
						if (line.numArgs < 2) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires two arguments");
						} else {
							try {
								UserToken token = database.Authenticate(new Credentials(line.args[0], line.args[1]), true);
								if (token != null)
									Console.WriteLine("Registration Successful");
								else
									Console.WriteLine("Registration Failed");
							} catch (DatabaseException ex) {
								Console.WriteLine("DATABASE ERROR: Registration could not proceed");
								Console.WriteLine(ex.Message);
							}
						}
						break;

					case "deleteaccount":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else {
							Console.WriteLine(line.command + " not yet implimented");
						}
						break;

					case "resetpassword":
						if (line.numArgs < 2) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires two arguments");
						} else {
							Console.WriteLine(line.command + " not yet implimented");
						}
						break;

					case "refreshtoken":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else {
							try {
								if (database.RefreshToken(line.args[0]))
									Console.WriteLine("Refreshed token");
								else
									Console.WriteLine("User '" + line.args[0] + "' does not exist");
							} catch (DatabaseException ex) {
								Console.WriteLine("DATABASE ERROR: Refresh could not proceed");
								Console.WriteLine(ex.Message);
							}
						}
						break;

					case "whois":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else {
							try {
								int id;
								if (int.TryParse(line.args[0], out id)) {
									string name = database.GetUsername(id);
									if (name != null)
										Console.WriteLine("Username of user " + id + "is '" + name + "'");
									else
										Console.WriteLine("User " + id + " does not exist");
								} else {
									Console.WriteLine("ERROR: Argument 1 must be an integer");
								}
							} catch (DatabaseException ex) {
								Console.WriteLine("DATABASE ERROR: Refresh could not proceed");
								Console.WriteLine(ex.Message);
							}
						}
						break;

					// SONG COMMANDS
					case "newsong":
						if (line.numArgs < 3) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires three arguments");
						} else {
							try {
								int seed, tempo;
								if (line.numArgs < 4) {
									if (int.TryParse(line.args[0], out seed) && int.TryParse(line.args[1], out tempo)) {
										SongParameters song = new SongParameters(seed, tempo, line.args[2]);
										song = database.VoteOnSong(song, true);

										Console.WriteLine("Created new song with ID '" + song.ID + "' as anonymous");
									} else {
										Console.WriteLine("Arguments 1 and 2 must be integers");
									}
								} else {
									int userID;
									if (int.TryParse(line.args[0], out seed) && int.TryParse(line.args[1], out tempo) && int.TryParse(line.args[3], out userID)) {
										SongParameters song = new SongParameters(seed, tempo, line.args[2], userID);
										song = database.VoteOnSong(song, true);

										Console.WriteLine("Created new song with ID '" + song.ID + "' belonging to user " + userID);
									} else {
										Console.WriteLine("Arguments 1, 2, and 4 must be integers");
									}
								}
							} catch (DatabaseException ex) {
								Console.WriteLine("DATABASE ERROR: Could not create song");
								Console.WriteLine(ex.Message);
							}
						}
						break;

					case "deletesong":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else {
							Console.WriteLine(line.command + " not yet implimented");
						}
						break;

					case "setscore":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else {
							Console.WriteLine(line.command + " not yet implimented");
						}
						break;

					case "songinfo":
						if (line.numArgs < 1) {
							Console.WriteLine("ERROR: The command '" + line.command + "' requires an argument");
						} else {
							try {
								int id;
								if (int.TryParse(line.args[0], out id)) {
									if (database.SongExists(id)) {
										SongParameters song = database.GetSong(id);
									} else {
										Console.WriteLine("Song " + id + " does not exist");
									}
								} else {
									Console.WriteLine("ERROR: Argument 1 must be an integer");
								}
							} catch (DatabaseException ex) {
								Console.WriteLine("DATABASE ERROR: Could not get song info");
								Console.WriteLine(ex.Message);
							}
						}
						break;

					case "help":
					case "?":
						Console.WriteLine("-------------------------");
						Console.WriteLine("      COMMAND LIST");
						Console.WriteLine("-------------------------");
						Console.WriteLine("SERVER COMMANDS");
						Console.WriteLine("Start");
						Console.WriteLine("    Starts the server");
						Console.WriteLine("Stop");
						Console.WriteLine("    Stops the server");
						Console.WriteLine("Restart");
						Console.WriteLine("    Restarts the server");
						Console.WriteLine("Update <password>");
						Console.WriteLine("    Modifies the database according to the changes.  The server must be stopped.");
						Console.WriteLine("Upstart <password>");
						Console.WriteLine("    Modifies the database according to the changes and starts the server.  The server must be stopped.");
						Console.WriteLine();
						Console.WriteLine("DATABASE COMMANDS");
						Console.WriteLine("DBHost/DBHostID <new ID>");
						Console.WriteLine("    Changes the hostID of the database.  Requires an update to take effect.");
						Console.WriteLine("DBPort <new Port>");
						Console.WriteLine("    Changes the port of the database.  Requires an update to take effect.");
						Console.WriteLine("Database/DatabaseName <new name>");
						Console.WriteLine("    Changes the name of the database.  Requires an update to take effect.");
						Console.WriteLine("DBUser/DBUserID <new id>");
						Console.WriteLine("    Changes the userID to use with the database.  Requires an update to take effect.");
						Console.WriteLine("DBInfo");
						Console.WriteLine("    Displays information about the server.");
						Console.WriteLine();
						Console.WriteLine("USER ACCOUNT COMMANDS");
						Console.WriteLine("Newaccount <username> <password>");
						Console.WriteLine("    Creates a new user account with the given username and password.");
						Console.WriteLine("Deleteaccount <username>");
						Console.WriteLine("    Deletes a user account with the given username.");
						Console.WriteLine("Resetpassword <username> <password>");
						Console.WriteLine("    Resets the password of the given username to the given password.");
						Console.WriteLine("Refreshtoken <username>");
						Console.WriteLine("    Refreshes the token associated with the given user account.");
						Console.WriteLine("Whois <id>");
						Console.WriteLine("    Returns the username of the user with the given ID.");
						Console.WriteLine();
						Console.WriteLine("SONG COMMANDS");
						Console.WriteLine("Newsong <seed> <tempo> <genre> [<userID>]");
						Console.WriteLine("    Adds a new song to the database with the given seed, tempo, and genre, and optionally, userID.  Displays the ID of the newly-added song.");
						Console.WriteLine("Deletesong <id>");
						Console.WriteLine("    Removes the given song from the database.");
						Console.WriteLine("Setscore <id> <score>");
						Console.WriteLine("    Sets the score of a given song to the given score.");
						Console.WriteLine("Songinfo <id>");
						Console.WriteLine("    Gets all the info about the given song.");

						break;
					default:
						Console.WriteLine("'" + line.command + "' is not a valid command.  Type help or ? for a list of commands.");
						break;
				}
			}
		}

		/// <summary>
		/// Creates a new server object that listens on the specified port
		/// </summary>
		/// <param name="port">The port to listen on</param>
		/// <param name="database">SQL Database string</param>
		/// <param name="logFileLocation">Location to save the log</param>
		/// <param name="logLevel">What level of logs to use</param>
		internal Server(int port, Database database, string logFileLocation, int logLevel) {
			this.database = database;
			this.logFileLocation = logFileLocation;
			this.logLevel = logLevel;

			this.tcpListener = new TcpListener(IPAddress.Any, port);
		}

		/// <summary>
		/// Checks the health of the server
		/// </summary>
		/// <returns>Returns true if the server is alive, false otherwise</returns>
		internal bool IsAlive() {
			if (listenThread == null)
				return false;
			else
				return listenThread.IsAlive;
		}

		/// <summary>
		/// Starts the server thread.
		/// </summary>
		internal void Start() {
			Log("Server started", 0);

			listenThread = new Thread(new ThreadStart(ListenForClients));
			listenThread.Start();
		}

		/// <summary>
		/// Stops the server thread
		/// </summary>
		internal void Stop() {
			Log("Stopping Server", 0);

			listenThread.Abort();
			listenThread = null;
		}

		/// <summary>
		/// Restarts the server thread
		/// </summary>
		internal void Restart() {
			Log("Restarting Server...", 0);
			
			Stop();
			Start();
		}

		/// <summary>
		/// Main server thread function.
		/// Accepts clients over TCP and uses ThreadPool to support multiple connections at the same time.
		/// </summary>
		private void ListenForClients() {
			tcpListener.Start();
			
			while (true) {
				TcpClient client = tcpListener.AcceptTcpClient();
				ThreadPool.QueueUserWorkItem(new WaitCallback(HandleConnectionWithClient), client);
			}
		}
		
		/// <summary>
		/// Child thread function.
		/// Handles a connection with a single client.
		/// </summary>
		/// <param name="client">The TcpClient object</param>
		private void HandleConnectionWithClient(object client) {
			TcpClient tcpClient = (TcpClient)client;
			NetworkStream networkStream = tcpClient.GetStream();
			IPAddress address = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address;

			Log("Remote client connected", address, 0);
			
			object message = Message.Recieve(networkStream);
			while (tcpClient.Connected && message != null) {
				
				if (message is string && message as string == "Test") {

					// A test message was recieved.  Send a response back.
					Log("Recieved a connection test request", address, 1);
					Message.TestMsg(networkStream);

				} else if (message is AuthRequest) {

					// An AuthRequest was receieved.  Authenticate the user and reply with a UserToken.
					AuthRequest req = message as AuthRequest;
					Log("Recieved an authentication request", address, 1);

					UserToken token = null;
					if (req.register) {
						Log("    Registering New User: " + req.credentials.username, address, 2);

						try {
							token = database.Authenticate(req.credentials, true);
							if (token != null)
								Log("    Registration Successful", address, 2);
							else
								Log("    Registration Failed", address, 2);
						} catch (DatabaseException ex) {
							Log("DATABASE ERROR: Registration could not proceed", address, 0);
							Log(ex.Message, 0);
						}
						
					} else {
						Log("    Authenticating User: " + req.credentials.username, address, 2);

						try {
							token = database.Authenticate(req.credentials, false);
							if (token != null)
								Log("    Authentication Successful", address, 2);
							else
								Log("    Authentication Failed", address, 2);
						} catch (DatabaseException ex) {
							Log("DATABASE ERROR: Authentication could not proceed", address, 0);
							Log(ex.Message, 0);
						}
					}

					Message.Send(networkStream, new AuthResponse(token));
					
				} else if (message is TokenVerifyRequest) {
					
					// A TokenVerifyRequest was recieved.  Test the token for validity.
					Log("Recieved a token verification request", address, 1);
					TokenVerifyRequest req = message as TokenVerifyRequest;
					
					Log("    User: " + req.token.username, address, 2);
					Log("    Expires: " + req.token.expires, address, 2);
					Log("    Token: " + req.token.token, address, 2);

					try {

						bool valid;

						if (database.VerifyToken(req.token) != 0) {
							Log("    Verification Successful", address, 2);
							valid = true;
						} else {
							Log("    Verification Failed", address, 2);
							valid = false;
						}

						Message.Send(networkStream, new TokenVerifyResponse(valid));
					} catch (DatabaseException ex) {
						Log("DATABASE ERROR: Could not process request", address, 0);
						Log(ex.Message, 0);
						Message.Send(networkStream, new BBResponse("Database", "An unknown database error occured.  Could not process request."));
					}

				} else if (message is BBRequest) {

					try {
						// A BBRequest was recieved.  Process the request
						BBRequest bbmessage = message as BBRequest;
						Log("Received a " + bbmessage.requestType + " request", address, 1);

						// Authenticate the user
						int userID;
						if (bbmessage.requestType.userToken == null) {
							Log("    No user supplied.  Processing as anonymous.", address, 2);
							userID = -1;
						} else {
							Log("    Username: " + bbmessage.requestType.userToken.username, address, 2);
							Log("    Expires: " + bbmessage.requestType.userToken.expires, address, 2);
							Log("    Token: " + bbmessage.requestType.userToken.token, address, 2);
							userID = database.VerifyToken(bbmessage.requestType.userToken);
						}

						if (userID == 0) {
							Log("    User Authentication failed", address, 1);
							Message.Send(networkStream, new BBResponse());
						} else {
							if (bbmessage.requestType is BBRequest.UpDownVote) {

								// Upload and vote on a song
								BBRequest.UpDownVote req = bbmessage.requestType as BBRequest.UpDownVote;

								Log("    " + (req.vote ? "Upvote" : "Downvote") + " Request", address, 2);
								Log("        ID: " + req.song.ID, address, 3);
								Log("     Genre: " + req.song.genre, address, 3);
								Log("     Tempo: " + req.song.tempo, address, 3);
								Log("      Seed: " + req.song.seed, address, 3);

								SongParameters song = database.VoteOnSong(req.song, req.vote);
								Message.Send(networkStream, new BBResponse(song));

								Log("    Response has ID of " + song.ID + " and score of " + song.score, address, 2);

							} else if (bbmessage.requestType is BBRequest.RequestScore) {

								// Request the score of a song
								BBRequest.RequestScore req = bbmessage.requestType as BBRequest.RequestScore;

								Log("        ID: " + req.song.ID, address, 3);
								Log("     Genre: " + req.song.genre, address, 3);
								Log("     Tempo: " + req.song.tempo, address, 3);
								Log("      Seed: " + req.song.seed, address, 3);

								SongParameters song = database.RefreshSong(req.song);
								Message.Send(networkStream, new BBResponse(song));

								Log("    Response has ID of " + song.ID + " and score of " + song.score, address, 2);

							} else if (bbmessage.requestType is BBRequest.RequestSongs) {

								// Request a list of songs
								BBRequest.RequestSongs req = bbmessage.requestType as BBRequest.RequestSongs;

								List<SongParameters> songList = database.GetSongList(req.num);
								Message.Send(networkStream, new BBResponse(songList));

								Log("    Returned " + songList.Count + " songs", address, 2);

							} else {
								Log("BBREQUEST ERROR: Unknown BBRequest type '" + bbmessage.GetType() + "'", address, 0);
								Message.Send(networkStream, new BBResponse("BBRequest", "Unknown BBRequest type '" + bbmessage.GetType() + "'"));
							}
						}
					} catch (DatabaseException ex) {
						Log("DATABASE ERROR: Could not process request", address, 0);
						Log(ex.Message, 0);
						Message.Send(networkStream, new BBResponse("Database", "An unknown database error occured.  Could not process request."));
					} catch (System.IO.IOException ex) {
						Log("IO ERROR: Could not send response", address, 0);
						Log(ex.Message, 0);
					}

				} else {

					Log("Unknown request type '" + message.GetType() + "'", address, 0);

				}

				message = Message.Recieve(networkStream);
			}

			Log("Remote client disconnected", address, 0);

			tcpClient.Close();
		}

		/// <summary>
		/// Logs a message
		/// </summary>
		/// <param name="message">Message to log</param>
		private void Log(string message, int level) {
			if (logLevel >= level) {
				Console.WriteLine("<{0}> {1}", Timestamp(), message);
				lock (logFileLocation) {
					using (System.IO.StreamWriter file = new System.IO.StreamWriter(logFileLocation, true)) {
						file.WriteLine("<{0}> {1}", Timestamp(), message);
					}
				}
			}
		}

		/// <summary>
		/// Logs a message and the IP of the connected client
		/// </summary>
		/// <param name="message">Message to log</param>
		/// <param name="address">IP address to log</param>
		private void Log(string message, IPAddress address, int level) {
			Log(String.Format("{0} - {1}", address, message), level);
		}

		/// <summary>
		/// Gets the current time as a string
		/// </summary>
		/// <returns></returns>
		private string Timestamp() {
			return String.Format("{0:hh:mm:ss}", DateTime.Now);
		}
	}

	/// <summary>
	/// A very basic command-line parser.  Supports only single commands with an optional list of value.
	/// </summary>
	internal class CommandLine {
		internal string command { get; private set; }
		internal string[] args { get; private set; }
		internal int numArgs {
			get {
				if (args != null)
					return args.Length;
				else
					return 0;
			}
		}

		internal CommandLine(string[] tokens) {
			this.command = tokens[0];

			if (tokens.Length > 1) {
				this.args = new string[tokens.Length - 1];
				Array.ConstrainedCopy(tokens, 1, this.args, 0, this.args.Length);
			} else {
				this.args = null;
			}
			
		}

		internal static CommandLine Prompt() {
			Console.Write("> ");

			string[] tokens = Console.ReadLine().Split(new[] { ' ' });
			return new CommandLine(tokens);
		}
	}
}
