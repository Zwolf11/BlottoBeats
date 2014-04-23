using System;
using System.Security.Cryptography;

namespace BlottoBeats.Library.Authentication {
	/// <summary>
	/// Authorization token for a user
	/// </summary>
	[SerializableAttribute]
	public class UserToken {
		public string username { get; private set; }
		public DateTime expires { get; private set; }
		public string token { get; private set; }

		private static TimeSpan offset = new TimeSpan(30, 0, 0, 0);	// 30 day expiry on tokens

		public UserToken(String username, DateTime expires, string token) {
			this.username = username;
			this.expires = expires;
			this.token = token;
		}

		/// <summary>
		/// Verifies that two tokens are equal to each other and the "expires" date has not passed.
		/// Uses SlowEquals to prevent timing attacks.
		/// </summary>
		/// <param name="token">Token to compare</param>
		/// <returns>Whether the tokens match and are valid</returns>
		public bool Verify(UserToken token) {
			bool equal = (this.username.ToLower() == token.username.ToLower());

			equal &= (TrimMilliseconds(this.expires) == TrimMilliseconds(token.expires));
			equal &= (this.expires.CompareTo(DateTime.Now) > 0);
			equal &= PasswordHash.PasswordHash.SlowEquals(Convert.FromBase64String(this.token), Convert.FromBase64String(token.token));

			return equal;
		}

		/// <summary>
		/// Generates a new authentication token using a crytographically-secure random number generator
		/// </summary>
		/// <returns>The authentication token</returns>
		public static string GenerateToken() {
			RandomNumberGenerator rng = new RNGCryptoServiceProvider();
			byte[] tokenData = new byte[32];
			rng.GetBytes(tokenData);

			return Convert.ToBase64String(tokenData);
		}

		/// <summary>
		/// Returns a DateTime object that represents the moment the token expires
		/// </summary>
		/// <returns>DateTime object</returns>
		public static DateTime GetExpiration() {
			return DateTime.Now.Add(offset);
		}

		/// <summary>
		/// Because dateTimes are stupid
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static DateTime TrimMilliseconds(DateTime dt) {
			return new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second, 0);
		}
	}

	/// <summary>
	/// Login Credentials for a user
	/// </summary>
	[SerializableAttribute]
	public class Credentials {
		public string username { get; private set; }
		private string password { get; set; }

		public Credentials(string username, string password) {
			this.username = username;
			this.password = password;
		}

		/// <summary>
		/// Verifies that the specified password matches the given hash
		/// </summary>
		/// <param name="hash">Hash to check</param>
		/// <returns>True if they match, false otherwise</returns>
		public bool Verify(string hash) {
			return PasswordHash.PasswordHash.ValidatePassword(this.password, hash);
		}

		/// <summary>
		/// Generates a new hash for the password
		/// </summary>
		/// <returns>A new hash for the password</returns>
		public string GenerateHash() {
			return PasswordHash.PasswordHash.CreateHash(this.password);
		}
	}
}
