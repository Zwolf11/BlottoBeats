using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SongData;

namespace Generator
{
    public class Generator
    {
        internal class SongPattern
        {
            public int totalNumMeasures { get; private set; }
            public int repeatEvery{get; private set;}
            public int numRepetitions{get; private set;}

            public SongPattern(int numMeas, int repeatEv)
            {
                totalNumMeasures = numMeas;
                repeatEvery = repeatEv;
                numRepetitions = numMeas / repeatEv;
            }

        }
        public void generate(int seed, SongParameters paramets)
        {
            Random randomizer = new Random(seed);
            int mode;
            String key;
            String timeSigPattern; //Simple or Compound Meter
            int timeSigQuant; // 2 = Duple, 3 = Triple, etc
            int numpatterns;
            List<SongPattern> patterns;
            Song output;

            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" }; //array of all note values
   
            //Select Mode
            mode = randomizer.Next(2);
            
            //Select Key
            key = notes[randomizer.Next(12)];


            int randOutput = randomizer.Next(2);
            switch (randOutput){
                case 0:
                    timeSigPattern = "Simple";
                    break;
                case 1:
                    timeSigPattern = "Compound";
                    break;
            }

            timeSigQuant = randomizer.Next(3) + 1;

            numpatterns = randomizer.Next(4);

            for (int i = 0; i <= numpatterns; i++)
            {
                randOutput = randomizer.Next(8) + 1;
                int measures = randOutput * 4;
                int rep = 0;
                while(rep == 0 || (measures%rep!= 0)){
                    rep = (randomizer.Next(8) + 1) * 4;
                }
                SongPattern inGeneration = new SongPattern(measures, rep);

                for (int j = 0; j < inGeneration.repeatEvery / 4; j++)
                {
                    int numChords = randomizer.Next(6) + 1;
                    /* Select Chords, Select Rhythm, Voice Them, add them to song*/
                }


            }

            /*outputToMidi(output);*/
            


        }

        /*static void Main(string[] args)
        {
        }*/
    }
}
