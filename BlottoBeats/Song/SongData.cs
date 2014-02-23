using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongData {
	/// <summary>
	/// TODO - MICHAEL/AUSTIN: Documentation
	/// </summary>
	[SerializableAttribute]
    public class Song {
		// TODO - MICHAEL/AUSTIN: ADD STUFF
    }

	/// <summary>
	/// TODO - MICHAEL/AUSTIN: Documentation
	/// </summary>
	[SerializableAttribute]
	public class SongParameters {
		// TODO - MICHAEL/AUSTIN: ADD STUFF
	}

	/// <summary>
	/// Contains a single song and the vote data for that song
	/// </summary>
	[SerializableAttribute]
	public class SongAndVoteData {
		public Song song { get; private set; }
		public int score { get; private set; }

		public SongAndVoteData(Song song, int score) {
			this.song = song;
			this.score = score;
		}
	}

	// TODO - MICHAEL/AUSTIN: Add any other song information necessary
}
