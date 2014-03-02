using System;
using System.Collections.Generic;

namespace SongData {
	/// <summary>
	/// Song class definition; Each instance of Song will have a tempo/speed, key/mode, and song data consisting of the subclasses below.
	/// </summary>
    [SerializableAttribute]
    public class Song
    {
		public int ID { get; set; }		// Used to index the songs on the server.  Newly generated songs should have an ID of -1.

        private int tempo;
        private string key; //Sharped notation is always used over flatted
        public List<SongSegment> songData { get; private set; }

        public Song(int temp, string ky)
        {
            tempo = temp;
            key = ky;
            songData = new List<SongSegment>();
        }

        public void addSegment(SongSegment x){
            songData.Add(x);
        }

        [SerializableAttribute]
        public class Chord
        {
            public HashSet<Note> chordVoice { get; private set; }
            public Chord(String[] notes, int rhythm)
            {
                chordVoice = new HashSet<Note>();
                int len = notes.Length;
                for (int i = 0; i < len; i++)
                {
                    chordVoice.Add(new Note(notes[i], rhythm));

                }

            }
        }

        [SerializableAttribute]
        public class Note
        {
            public String noteValue { get; private set; }
            public int length { get; private set; }

            public Note(String val, int len)
            {
                noteValue = val;
                length = len;
            }
        }

        [SerializableAttribute]
        public class Melody
        {
            List<Note> melodicLine;
        }

        [SerializableAttribute]
        public class SongSegment
        {
            public List<Melody> melodies { get; private set; }
            public List<Chord> chordPattern { get; private set; }

            public SongSegment()
            {
                chordPattern = new List<Chord>();
                melodies = new List<Melody>();
            }
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
		public int ID { get; set; }		// Used to index the songs on the server.  Newly generated songs should have an ID of -1.
        public int tempo { get; set; }
        public string genre { get; set; }
	}

	/// <summary>
	/// Contains a single set of song parameters and the vote data for that song
	/// </summary>
	[SerializableAttribute]
	public class SongAndVoteData {
		public int seed { get; private set; }
		public SongParameters song { get; private set; }
		public int score { get; private set; }

		public SongAndVoteData(int seed, SongParameters song, int score) {
			this.seed = seed;
			this.song = song;
			this.score = score;
		}
	}

	// TODO - MICHAEL/AUSTIN: Add any other song information necessary
}
