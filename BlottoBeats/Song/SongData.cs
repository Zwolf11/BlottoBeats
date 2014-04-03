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
        private string genre;
        public List<SongSegment> songData { get; private set; }

        public Song(int temp, string ky, string gen)
        {
            tempo = temp;
            key = ky;
            genre = gen;
            songData = new List<SongSegment>();
        }

        public void addSegment(SongSegment x){
            songData.Add(x);
        }

        [SerializableAttribute]
        public class Chord
        {
            public HashSet<Note> chordVoice { get; private set; }
            public int chordVal { get; private set; }
            public Chord(String[] notes, int rhythm, int chordNum)
            {
                chordVoice = new HashSet<Note>();
                chordVal = chordNum;
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
            public List<Note> melodicLine { get; private set; }
            public Melody()
            {

                melodicLine = new List<Note>();
            }
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
        
        public string Genre 
        {
            get
            {
                return this.genre;
            }
            set
            {
                this.genre = value;
            }
        }
    }

	/// <summary>
	/// Contains a all the information required to construct a song.
	/// </summary>
	[SerializableAttribute]
	public class SongParameters {
		public int ID { get; set; }		// Used to index the songs on the server.  Newly generated songs should have an ID of -1.
		public int seed { get; set; }
		public int tempo { get; set; }
        public string genre { get; set; }
		public int score { get; set; }

		/// <summary>
		/// Creates a new SongParameters object with a default ID of -1 and a score of 0
		/// </summary>
		/// <param name="seed">Seed of the song</param>
		/// <param name="tempo">Tempo of the song</param>
		/// <param name="genre">Genre of the song</param>
		public SongParameters(int seed, int tempo, string genre) {
			this.ID = -1;
			this.score = 0; 
			this.seed = seed;
			this.tempo = tempo;
			this.genre = genre;
		}

		/// <summary>
		/// Constructs a SongParameters object with the given data
		/// </summary>
		/// <param name="ID">ID of the song</param>
		/// <param name="score">Score of the song</param>
		/// <param name="seed">Seed of the song</param>
		/// <param name="tempo">Tempo of the song</param>
		/// <param name="genre">Genre of the song</param>
		public SongParameters(int ID, int score, int seed, int tempo, string genre) {
			this.ID = ID;
			this.score = score; 
			this.seed = seed;
			this.tempo = tempo;
			this.genre = genre;
		}
	}
}
