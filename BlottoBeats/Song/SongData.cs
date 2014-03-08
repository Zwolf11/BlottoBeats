using System;
using System.Collections.Generic;

namespace SongData {
	/// <summary>
	/// Song class definition; Each instance of Song will have a tempo/speed, key/mode, and song data consisting of the subclasses below.
	/// </summary>
    [SerializableAttribute]
    public class Song
    {
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

		/// <summary>
		/// Generates a SongParameters object with a default ID of -1
		/// </summary>
		/// <param name="tempo">Tempo of the song</param>
		/// <param name="genre">Genre of the song</param>
		public SongParameters(int tempo, string genre) {
			this.ID = -1;
			this.tempo = tempo;
			this.genre = genre;
		}

		/// <summary>
		/// Generates a SongParameters object with a given ID
		/// </summary>
		/// <param name="ID">ID of the song</param>
		/// <param name="tempo">Tempo of the song</param>
		/// <param name="genre">Genre of the song</param>
		public SongParameters(int ID, int tempo, string genre) {
			this.ID = ID;
			this.tempo = tempo;
			this.genre = genre;
		}
	}

	/// <summary>
	/// Contains a all the information required to construct a song.
	/// </summary>
	[SerializableAttribute]
	public class CompleteSongData {
		public int seed { get; private set; }
		public SongParameters song { get; private set; }
		public int score { get; private set; }

		public CompleteSongData(int seed, SongParameters song) {
			this.seed = seed;
			this.song = song;
			this.score = 0;
		}

		public CompleteSongData(int seed, SongParameters song, int score) {
			this.seed = seed;
			this.song = song;
			this.score = score;
		}
	}
}
