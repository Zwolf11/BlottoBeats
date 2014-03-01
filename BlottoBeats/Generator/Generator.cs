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
            int mode; // 0 = Major 1 = Minor
            String key;
            String timeSigPattern = ""; //Simple or Compound Meter
            int timeSigQuant = 0; // 2 = Duple, 3 = Triple, etc
            int numpatterns = 0;

            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" }; //array of all note values
   
            //Select Mode
            mode = randomizer.Next(2);
            
            //Select Key
            key = notes[randomizer.Next(12)];

            Song output = new Song(paramets.tempo, key);

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

            //TODO Actually figure out the rules for song patterns. It's currently a mess.
            numpatterns = randomizer.Next(4);

            for (int i = 0; i <= numpatterns; i++)
            {
                Song.SongSegment thisSection = new Song.SongSegment();
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
                    String chord="";
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
                    if (numChords == 5)
                    {
                        if (j == 0 || prevWasHalf)
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(17);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "13625";
                                        break;
                                    case 1:
                                        chord = "13645";
                                        break;
                                    case 2:
                                        chord = "13425";
                                        break;
                                    case 3:
                                        chord = "16425";
                                        break;
                                    case 4:
                                        chord = "16256";
                                        break;
                                    case 5:
                                        chord = "16456";
                                        break;
                                    case 6:
                                        chord = "13456";
                                        break;
                                    case 7:
                                        chord = "14256";
                                        break;
                                    case 8:
                                        chord = "13641";
                                        break;
                                    case 9:
                                        chord = "16271";
                                        break;
                                    case 10:
                                        chord = "16471";
                                        break;
                                    case 11:
                                        chord = "13471";
                                        break;
                                    case 12:
                                        chord = "16251";
                                        break;
                                    case 13:
                                        chord = "16451";
                                        break;
                                    case 14:
                                        chord = "13451";
                                        break;
                                    case 15:
                                        chord = "14521";
                                        break;
                                    case 16:
                                        chord = "14271";
                                        break;
                                }
                            }
                            else
                            {
                                 randOutput = randomizer.Next(9);
                                 switch (randOutput)
                                 {
                                     case 0:
                                         chord = "13641";
                                         break;
                                     case 1:
                                         chord = "16271";
                                         break;
                                     case 2:
                                         chord = "16471";
                                         break;
                                     case 3:
                                         chord = "13471";
                                         break;
                                     case 4:
                                         chord = "16251";
                                         break;
                                     case 5:
                                         chord = "16451";
                                         break;
                                     case 6:
                                         chord = "13451";
                                         break;
                                     case 7:
                                         chord = "14521";
                                         break;
                                     case 8:
                                         chord = "14271";
                                         break;
                                 }
                            }

                        }
                        else
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(30);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "13625";
                                        break;
                                    case 1:
                                        chord = "13645";
                                        break;
                                    case 2:
                                        chord = "13425";
                                        break;
                                    case 3:
                                        chord = "16425";
                                        break;
                                    case 4:
                                        chord = "36425";
                                        break;
                                    case 5:
                                        chord = "16256";
                                        break;
                                    case 6:
                                        chord = "36256";
                                        break;
                                    case 7:
                                        chord = "16456";
                                        break;
                                    case 8:
                                        chord = "36456";
                                        break;
                                    case 9:
                                        chord = "13456";
                                        break;
                                    case 10:
                                        chord = "14256";
                                        break;
                                    case 11:
                                        chord = "64256";
                                        break;
                                    case 12:
                                        chord = "34256";
                                        break;
                                    case 13:
                                        chord = "13641";
                                        break;
                                    case 14:
                                        chord = "16271";
                                        break;
                                    case 15:
                                        chord = "36271";
                                        break;
                                    case 16:
                                        chord = "16471";
                                        break;
                                    case 17:
                                        chord = "36471";
                                        break;
                                    case 18:
                                        chord = "13471";
                                        break;
                                    case 19:
                                        chord = "16251";
                                        break;
                                    case 20:
                                        chord = "36251";
                                        break;
                                    case 21:
                                        chord = "16451";
                                        break;
                                    case 22:
                                        chord = "36451";
                                        break;
                                    case 23:
                                        chord = "13451";
                                        break;
                                    case 24:
                                        chord = "14251";
                                        break;
                                    case 25:
                                        chord = "34251";
                                        break;
                                    case 26:
                                        chord = "64251";
                                        break;
                                    case 27:
                                        chord = "14271";
                                        break;
                                    case 28:
                                        chord = "34271";
                                        break;
                                    case 29:
                                        chord = "64271";
                                        break;
                                }
                            }
                            else
                            {
                                randOutput = randomizer.Next(17);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "13641";
                                        break;
                                    case 1:
                                        chord = "16271";
                                        break;
                                    case 2:
                                        chord = "36271";
                                        break;
                                    case 3:
                                        chord = "16471";
                                        break;
                                    case 4:
                                        chord = "36471";
                                        break;
                                    case 5:
                                        chord = "13471";
                                        break;
                                    case 6:
                                        chord = "16251";
                                        break;
                                    case 7:
                                        chord = "36251";
                                        break;
                                    case 8:
                                        chord = "16451";
                                        break;
                                    case 9:
                                        chord = "36451";
                                        break;
                                    case 10:
                                        chord = "13451";
                                        break;
                                    case 11:
                                        chord = "14251";
                                        break;
                                    case 12:
                                        chord = "34251";
                                        break;
                                    case 13:
                                        chord = "64251";
                                        break;
                                    case 14:
                                        chord = "14271";
                                        break;
                                    case 15:
                                        chord = "34271";
                                        break;
                                    case 16:
                                        chord = "64271";
                                        break;
                                }

                            }

                        }

                    }
                    if (numChords == 6)
                    {
                        if (j == 0 || prevWasHalf)
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(13);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "136425";
                                        break;
                                    case 1:
                                        chord = "136256";
                                        break;
                                    case 2:
                                        chord = "136456";
                                        break;
                                    case 3:
                                        chord = "164256";
                                        break;
                                    case 4:
                                        chord = "134256";
                                        break;
                                    case 5:
                                        chord = "136271";
                                        break;
                                    case 6:
                                        chord = "136471";
                                        break;
                                    case 7:
                                        chord = "136251";
                                        break;
                                    case 8:
                                        chord = "136451";
                                        break;
                                    case 9:
                                        chord = "134251";
                                        break;
                                    case 10:
                                        chord = "164251";
                                        break;
                                    case 11:
                                        chord = "134271";
                                        break;
                                    case 12:
                                        chord = "164271";
                                        break;
                                }
                            }
                            else
                            {
                                randOutput = randomizer.Next(8);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "136271";
                                        break;
                                    case 1:
                                        chord = "136471";
                                        break;
                                    case 2:
                                        chord = "136251";
                                        break;
                                    case 3:
                                        chord = "136451";
                                        break;
                                    case 4:
                                        chord = "134251";
                                        break;
                                    case 5:
                                        chord = "164251";
                                        break;
                                    case 6:
                                        chord = "134271";
                                        break;
                                    case 7:
                                        chord = "164271";
                                        break;
                                }
                            }

                        }
                        else
                        {
                            if (j != (inGeneration.repeatEvery / 4) - 1)
                            {
                                randOutput = randomizer.Next(16);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "136425";
                                        break;
                                    case 1:
                                        chord = "136256";
                                        break;
                                    case 2:
                                        chord = "136456";
                                        break;
                                    case 3:
                                        chord = "164256";
                                        break;
                                    case 4:
                                        chord = "364256";
                                        break;
                                    case 5:
                                        chord = "134256";
                                        break;
                                    case 6:
                                        chord = "136271";
                                        break;
                                    case 7:
                                        chord = "136471";
                                        break;
                                    case 8:
                                        chord = "136251";
                                        break;
                                    case 9:
                                        chord = "136451";
                                        break;
                                    case 10:
                                        chord = "134251";
                                        break;
                                    case 11:
                                        chord = "164251";
                                        break;
                                    case 12:
                                        chord = "364251";
                                        break;
                                    case 13:
                                        chord = "134271";
                                        break;
                                    case 14:
                                        chord = "164271";
                                        break;
                                    case 15:
                                        chord = "364271";
                                        break;
                                }
                            }
                            else
                            {
                                randOutput = randomizer.Next(10);
                                switch (randOutput)
                                {
                                    case 0:
                                        chord = "136271";
                                        break;
                                    case 1:
                                        chord = "136471";
                                        break;
                                    case 2:
                                        chord = "136251";
                                        break;
                                    case 3:
                                        chord = "136451";
                                        break;
                                    case 4:
                                        chord = "134251";
                                        break;
                                    case 5:
                                        chord = "164251";
                                        break;
                                    case 6:
                                        chord = "364251";
                                        break;
                                    case 7:
                                        chord = "134271";
                                        break;
                                    case 8:
                                        chord = "164271";
                                        break;
                                    case 9:
                                        chord = "364271";
                                        break;
                                }

                            }

                        }

                    }

                    int sumRhythm = 0;
                    numChords = chord.Length;
                    int rhythm=0;

                    //TODO Add rules for generating rhythm
                    for (int count = 0; count < numChords; count++)
                    {
                       if (timeSigPattern.Equals("Simple")){
                           if (timeSigQuant == 2)
                           {


                           }
                           if (timeSigQuant == 3)
                           {


                           }
                           if (timeSigQuant == 4)
                           {


                           }


                       }
                       else{
                           if (timeSigQuant == 2)
                           {


                           }
                           if (timeSigQuant == 3)
                           {


                           }
                           if (timeSigQuant == 4)
                           {


                           }
                       }

                       thisSection.chordPattern.Add(generateChord(mode, key, chord[count], rhythm));
                    }
                    if (chord[chord.Length - 1] == '5')
                    {
                        prevWasHalf = true;
                    }
                }


            }



            /*outputToMidi(output);*/
            


        }


        //TODO define chord voicing in minor mode
        private Song.Chord generateChord(int mode, String key, char chord, int length)
        {
            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            String[] keySig= new String[7];
            String[] noteNames = new String[4];
            int chordNumIndex = int.Parse(new String(chord, 1)) - 1;
            int keynum = Array.IndexOf(notes, key);
            //generate chord in major mode
            if (mode == 0)
            {
                keySig[0] = notes[keynum];
                keySig[1] = notes[(keynum + 2)%12];
                keySig[2] = notes[(keynum + 4) % 12];
                keySig[3] = notes[(keynum + 5) % 12];
                keySig[4] = notes[(keynum + 7) % 12];
                keySig[5] = notes[(keynum + 9) % 12];
                keySig[6] = notes[(keynum + 11) % 12];

                noteNames[0] = keySig[chordNumIndex] + "1";
                noteNames[1] = keySig[chordNumIndex] + "4";
                noteNames[2] = keySig[(chordNumIndex + 2)%8] + "4";
                noteNames[3] = keySig[(chordNumIndex + 4)%8] + "4";
                return new Song.Chord(noteNames, length);
            }


            //generate chord in minor mode
            if (mode == 1)
            {
                keySig[0] = notes[keynum];
                keySig[1] = notes[(keynum + 2) % 12];
                keySig[2] = notes[(keynum + 3) % 12];
                keySig[3] = notes[(keynum + 5) % 12];
                keySig[4] = notes[(keynum + 7) % 12];
                keySig[5] = notes[(keynum + 8) % 12];
                keySig[6] = notes[(keynum + 10) % 12];


            }
            return null;

        }

        /*static void Main(string[] args)
        {
        }*/
    }
}
