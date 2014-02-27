using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongData {
	/// <summary>
	/// Song class definition; Each instance of Song will have a tempo/speed, key/mode, and song data consisting of the subclasses below.
	/// </summary>
    [SerializableAttribute]
    public class Song
    {
		public int ID { get; set; }		// Used to index the songs on the server.  Newly generated songs should have an ID of -1.
		public string genre { get; private set; }

        private int tempo;
        private String key; //Sharped notation is always used over flatted
        private List<SongSegment> songData;

        [SerializableAttribute]
        class Chord
        {
            HashSet<Note> chordVoice;
        }

        [SerializableAttribute]
        class Note
        {
            String noteValue;
            int length;
        }

        [SerializableAttribute]
        class Melody
        {
            List<Note> melodicLine;
        }

        [SerializableAttribute]
        class SongSegment
        {
            List<Melody> melodies;
            List<Chord> chordPattern;
        }

        public int Tempo
        {
            get
            {
                return this.tempo;
            }
            set
            {
                this.tempo = value;
            }
        }
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
