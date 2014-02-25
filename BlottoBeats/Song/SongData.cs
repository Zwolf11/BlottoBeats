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
        private int tempo;
        private String key; //Sharped notation is always used over flatted
        private List<SongSegment> songData;

        class Chord
        {
            HashSet<Note> chordVoice;
        }

        class Note
        {
            String noteValue;
            int length;
        }

        class Melody
        {
            List<Note> melodicLine;
        }

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

        public List<SongSegment> SongData
        {
            get
            {
                return this.songData;
            }
            set
            {
                this.songData = value;
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
