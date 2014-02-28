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
                bool prevWasHalf = false;
                for (int j = 0; j < inGeneration.repeatEvery / 4; j++)
                {
                    int numChords = randomizer.Next(6) + 1;
                    int sumRhythm = 0;
                    String chord;
                    if (numChords == 1)
                    {
                        chord = "1";
                    }
                    if (numChords == 2)
                    {
                        if (j == 0 || prevWasHalf)
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(2);
                                switch (randOutput){
                                    case 0:
                                        chord = "15";
                                        break;
                                    case 1:
                                        chord = "1";
                                        break;
                                }
                            }
                            else
                            {
                                chord = "1";
                            }

                        }
                        else
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(9);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "16";
                                        break;
                                    case 1:
                                        chord = "56";
                                        break;
                                    case 2:
                                        chord = "76";
                                        break;
                                    case 3:
                                        chord = "15";
                                        break;
                                    case 4:
                                        chord = "45";
                                        break;
                                    case 5:
                                        chord = "25";
                                        break;
                                    case 6:
                                        chord = "51";
                                        break;
                                    case 7:
                                        chord = "41";
                                        break;
                                    case 8:
                                        chord = "71";
                                        break;
                                }                  
                            }
                            else
                            {
                                randOutput = randomizer.Next(3);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "51";
                                        break;
                                    case 1:
                                        chord = "41";
                                        break;
                                    case 2:
                                        chord = "71";
                                        break;
                                }
     
                            }

                        }
                    }
                    if (numChords == 3)
                    {
                        if (j == 0 || prevWasHalf)
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(6);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "145";
                                        break;
                                    case 1:
                                        chord = "125";
                                        break;
                                    case 2:
                                        chord = "156";
                                        break;
                                    case 3:
                                        chord = "151";
                                        break;
                                    case 4:
                                        chord = "171";
                                        break;
                                    case 5:
                                        chord = "141";
                                        break;
                                }
                            }
                            else
                            {
                                randOutput = randomizer.Next(3);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "151";
                                        break;
                                    case 1:
                                        chord = "171";
                                        break;
                                    case 2:
                                        chord = "141";
                                        break;
                                }
                            }

                        }
                        else
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(17);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "125";
                                        break;
                                    case 1:
                                        chord = "145";
                                        break;
                                    case 2:
                                        chord = "625";
                                        break;
                                    case 3:
                                        chord = "645";
                                        break;
                                    case 4:
                                        chord = "345";
                                        break;
                                    case 5:
                                        chord = "156";
                                        break;
                                    case 6:
                                        chord = "256";
                                        break;
                                    case 7:
                                        chord = "456";
                                        break;
                                    case 8:
                                        chord = "151";
                                        break;
                                    case 9:
                                        chord = "251";
                                        break;
                                    case 10:
                                        chord = "451";
                                        break;
                                    case 11:
                                        chord = "171";
                                        break;
                                    case 12:
                                        chord = "271";
                                        break;
                                    case 13:
                                        chord = "471";
                                        break;
                                    case 14:
                                        chord = "141";
                                        break;
                                    case 15:
                                        chord = "641";
                                        break;
                                    case 16:
                                        chord = "341";
                                        break;
                                }
                            }
                            else
                            {
                                randOutput = randomizer.Next(9);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "151";
                                        break;
                                    case 1:
                                        chord = "251";
                                        break;
                                    case 2:
                                        chord = "451";
                                        break;
                                    case 3:
                                        chord = "171";
                                        break;
                                    case 4:
                                        chord = "271";
                                        break;
                                    case 5:
                                        chord = "471";
                                        break;
                                    case 6:
                                        chord = "141";
                                        break;
                                    case 7:
                                        chord = "641";
                                        break;
                                    case 8:
                                        chord = "341";
                                        break;
                                }

                            }

                        }
                    }
                    if (numChords == 4)
                    {
                        if (j == 0 || prevWasHalf)
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(12);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "1625";
                                        break;
                                    case 1:
                                        chord = "1645";
                                        break;
                                    case 2:
                                        chord = "1425";
                                        break;
                                    case 3:
                                        chord = "1345";
                                        break;
                                    case 4:
                                        chord = "1256";
                                        break;
                                    case 5:
                                        chord = "1456";
                                        break;
                                    case 6:
                                        chord = "1641";
                                        break;
                                    case 7:
                                        chord = "1341";
                                        break;
                                    case 8:
                                        chord = "1271";
                                        break;
                                    case 9:
                                        chord = "1471";
                                        break;
                                    case 10:
                                        chord = "1251";
                                        break;
                                    case 11:
                                        chord = "1451";
                                        break;
                                }
                            }
                            else
                            {
                                 randOutput = randomizer.Next(6);
                                 switch (randOutput)
                                 {
                                     case 0:
                                         chord = "1641";
                                         break;
                                     case 1:
                                         chord = "1341";
                                         break;
                                     case 2:
                                         chord = "1271";
                                         break;
                                     case 3:
                                         chord = "1471";
                                         break;
                                     case 4:
                                         chord = "1251";
                                         break;
                                     case 5:
                                         chord = "1451";
                                         break;
                                 }
                            }

                        }
                        else
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(29);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "1625";
                                        break;
                                    case 1:
                                        chord = "1645";
                                        break;
                                    case 2:
                                        chord = "3625";
                                        break;
                                    case 3:
                                        chord = "3645";
                                        break;
                                    case 4:
                                        chord = "1425";
                                        break;
                                    case 5:
                                        chord = "1345";
                                        break;
                                    case 6:
                                        chord = "3425";
                                        break;
                                    case 7:
                                        chord = "6425";
                                        break;
                                    case 8:
                                        chord = "1256";
                                        break;
                                    case 9:
                                        chord = "1456";
                                        break;
                                    case 10:
                                        chord = "6256";
                                        break;
                                    case 11:
                                        chord = "6456";
                                        break;
                                    case 12:
                                        chord = "3456";
                                        break;
                                    case 13:
                                        chord = "4256";
                                        break;
                                    case 14:
                                        chord = "1641";
                                        break;
                                    case 15:
                                        chord = "3641";
                                        break;
                                    case 16:
                                        chord = "1341";
                                        break;
                                    case 17:
                                        chord = "1271";
                                        break;
                                    case 18:
                                        chord = "6271";
                                        break;
                                    case 19:
                                        chord = "1471";
                                        break;
                                    case 20:
                                        chord = "6471";
                                        break;
                                    case 21:
                                        chord = "3471";
                                        break;
                                    case 22:
                                        chord = "1251";
                                        break;
                                    case 23:
                                        chord = "6251";
                                        break;
                                    case 24:
                                        chord = "1451";
                                        break;
                                    case 25:
                                        chord = "6451";
                                        break;
                                    case 26:
                                        chord = "3451";
                                        break;
                                    case 27:
                                        chord = "4251";
                                        break;
                                    case 28:
                                        chord = "4271";
                                        break;
                                }
                            }
                            else
                            {
                                randOutput = randomizer.Next(15);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "1641";
                                        break;
                                    case 1:
                                        chord = "3641";
                                        break;
                                    case 2:
                                        chord = "1341";
                                        break;
                                    case 3:
                                        chord = "1271";
                                        break;
                                    case 4:
                                        chord = "6271";
                                        break;
                                    case 5:
                                        chord = "1471";
                                        break;
                                    case 6:
                                        chord = "6471";
                                        break;
                                    case 7:
                                        chord = "3471";
                                        break;
                                    case 8:
                                        chord = "1251";
                                        break;
                                    case 9:
                                        chord = "6251";
                                        break;
                                    case 10:
                                        chord = "1451";
                                        break;
                                    case 11:
                                        chord = "6451";
                                        break;
                                    case 12:
                                        chord = "3451";
                                        break;
                                    case 13:
                                        chord = "4251";
                                        break;
                                    case 14:
                                        chord = "4271";
                                        break;
                                }

                            }

                        }

                    }

                }


            }

            /*outputToMidi(output);*/
            


        }

        /*static void Main(string[] args)
        {
        }*/
    }
}
