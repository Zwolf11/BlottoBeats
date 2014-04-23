using BlottoBeats.Library.SongData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BlottoBeats.Client
{
    public class Generator
    {
        internal class SongPattern
        {
            public int totalNumMeasures { get; private set; }
            public int repeatEvery { get; private set; }
            public int numRepetitions { get; private set; }

            public SongPattern(int numMeas, int repeatEv)
            {
                totalNumMeasures = numMeas;
                repeatEvery = repeatEv;
                numRepetitions = numMeas / repeatEv;
            }

        }

        public Generator()
        {

        }
        public double generate_4Chord(SongParameters paramets)
        {
            Random randomizer = new Random(paramets.seed);
            int mode = 0; // 0 = Major 1 = Minor
            String key;
            String gen;
            Song.SongSegment[] thisSection = new Song.SongSegment[3];
            String timeSigPattern = ""; //Simple or Compound Meter
            int timeSigQuant = 0; // 2 = Duple, 3 = Triple, etc

            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            

            //Select Key
            key = notes[randomizer.Next(12)];

            //Now also sets the genre
            Song output = new Song(paramets.tempo, key, paramets.genre);
            Console.Out.WriteLine(paramets.genre);

            int randOutput = randomizer.Next(2);
            switch (randOutput)
            {
                case 0:
                    timeSigPattern = "Simple";
                    break;
                case 1:
                    timeSigPattern = "Compound";
                    break;
            }

            timeSigQuant = randomizer.Next(3) + 2;

            for (int i = 0; i < 3; i++)
            {
                if (i != 2)
                    thisSection[i] = new Song.SongSegment();
                else
                    thisSection[i] = new Song.SongSegment(thisSection[0].chordPattern, thisSection[0].melodies, thisSection[0].melodies[0]);

                String chordProg="";

                int subdiv=0;

                int measureLen = 0;
                if (timeSigPattern.Equals("Simple"))
                {
                    measureLen += 4;
                }
                else
                    measureLen += 6;
                measureLen *= timeSigQuant;

                if (timeSigQuant == 2)
                {
                    randOutput = randomizer.Next(2) + 1;
                    subdiv = measureLen / randOutput;
                }
                else if (timeSigQuant == 3)
                {
                    randOutput = randomizer.Next(2) + 1;
                    if (randOutput == 2)
                        randOutput = 3;
                    subdiv = measureLen / randOutput;
                }
                else
                {
                    randOutput = randomizer.Next(3) + 1;
                    if (randOutput == 3)
                        randOutput = 4;
                    subdiv = measureLen / randOutput;
                }
                int repPerMeasure = measureLen / subdiv;

                int randout = randomizer.Next(5);
                switch (randout)
                {
                    case 0: 
                        chordProg = "6415";
                        break;
                    case 1:
                        chordProg = "1564";
                        break;
                    case 2:
                        chordProg = "1264";
                        break;
                    case 3:
                        chordProg = "1254";
                        break;
                    case 4:
                        chordProg = "4156";
                        break;

                }
                //Write chord progression
                if (i != 2)
                {

                    for (int j = 0; j < 4; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            for (int l = 0; l < repPerMeasure; l++)
                            {
                                thisSection[i].chordPattern.Add(generateChord(mode, key, chordProg[k], subdiv));
                            }
                                
                        }
                    }
                }
                //Write voice line
                composeMelody(thisSection[i], randomizer, key, mode, timeSigPattern, timeSigQuant);

                //Write bass line
                if (i != 2)
                {
                    Song.Melody thisMelody = new Song.Melody();
                    for (int j = 0; j < 4; j++)
                    {
                        for (int k = 0; k < 4; k++)
                        {
                            for (int l = 0; l < repPerMeasure; l++)
                            {
                               thisMelody.melodicLine.Add(thisSection[i].chordPattern[(j*4*repPerMeasure)+(k*repPerMeasure)+l].chordVoice.First());
                            }

                        }
                    }
                    thisSection[i].melodies.Add(thisMelody);

                }

                //Fix ordering of voice and bass line after insertion in bridge
                if (i == 2)
                {
                    Song.Melody tmp = thisSection[i].melodies[0];
                    thisSection[i].melodies[0] = thisSection[i].melodies[1];
                    thisSection[i].melodies[1] = tmp;
                }
            }

            output.addSegment(thisSection[0]);
            output.addSegment(thisSection[1]);
            output.addSegment(thisSection[0]);
            output.addSegment(thisSection[1]);
            output.addSegment(thisSection[2]);
            output.addSegment(thisSection[1]);

            BlottoBeats.MidiOut.MidiOut outgoing = new BlottoBeats.MidiOut.MidiOut();
            double songLen = outgoing.outputToMidi(output);

            return songLen;
        }
        public double generate_TwelveTone(SongParameters paramets)
        {
            const int NUMTONEROWS = 2;
            Random randomizer = new Random(paramets.seed);
            String gen;
            Song.SongSegment thisSection = new Song.SongSegment();
            String[,] toneRows = new String[NUMTONEROWS,12];
            String timeSigPattern = ""; //Simple or Compound Meter
            int timeSigQuant = 0; // 2 = Duple, 3 = Triple, etc

            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };

            //Set genre
            gen = paramets.genre;

            //Now also sets the genre
            Song output = new Song(paramets.tempo, "A", paramets.genre);
            Console.Out.WriteLine(paramets.genre);

            int randOutput = randomizer.Next(2);
            switch (randOutput)
            {
                case 0:
                    timeSigPattern = "Simple";
                    break;
                case 1:
                    timeSigPattern = "Compound";
                    break;
            }

            timeSigQuant = randomizer.Next(3) + 2;

            //Randomize toneRows
            for (int i = 0; i < NUMTONEROWS; i++)
            {
                int[] selected = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                for (int j = 0; j<12; j++){
                    int pos=0;
                    do{
                        pos = randomizer.Next(12);
                    }while(selected[pos]==1);

                    selected[pos] = 1;
                    toneRows[i, j] = notes[pos];
                }
            }
            //Select quantity of phrases (8-32)
            int phrases = randomizer.Next(25) + 8;

            //Make the actual twelve-tone composition
            for (int i = 0; i < NUMTONEROWS; i++)
            {
                thisSection.melodies.Add(new Song.Melody());
                for (int j = 0; j < phrases; j++)
                {
                    int rhythmSum = 0;
                    //Define measureLen
                    int measureLen=0;
                    if (timeSigPattern.Equals("Simple"))
                    {
                        measureLen += 4;
                    }
                    else
                        measureLen += 6;
                    measureLen *= timeSigQuant;

                    int prevNoteVal = 0;
                    int farthestPos = 0;
                    while (rhythmSum < measureLen * 4)
                    {
                        int noteVal;
                        int noteRhythm=0;
                        int remainderOfMeasure = measureLen - (rhythmSum % measureLen);
                        int measure = (rhythmSum / measureLen) + 1;
                        int measureBound = 0;
                        if (measure == 1)
                        {
                            measureBound = 2;
                        }
                        if (measure == 2)
                        {
                            measureBound = 4;
                        }
                        if (measure == 3)
                        {
                            measureBound = 6;
                        }
                        if (measure == 4)
                        {
                            measureBound = 12;
                        }


                        if (prevNoteVal == 0)
                        {
                            noteVal = prevNoteVal + 1;

                        }
                        else if (remainderOfMeasure - (measureBound - prevNoteVal) <= 1)
                        {
                            if (remainderOfMeasure - (measureBound - prevNoteVal) == 0)
                            {
                                noteVal = prevNoteVal + 1;
                            }
                            else
                            {
                                int randOut = randomizer.Next(2);
                                noteVal = prevNoteVal + randOut;

                            }
                        }
                        else
                        {
                            if (farthestPos == prevNoteVal)
                            {
                                int randOut = randomizer.Next(3) - 1;
                                noteVal = prevNoteVal + randOut;
                            }
                            else
                            {
                                int randOut = randomizer.Next(2);
                                noteVal = prevNoteVal + randOut;
                            }
                            
                        }

                        if (measure == 1)
                        {
                            if (noteVal > 4)
                                noteVal = 4;
                        }
                        if (measure == 2)
                        {
                            if (noteVal > 9)
                                noteVal = 9;
                        }
                        if (measure == 3)
                        {
                            if (noteVal > 11)
                                noteVal = 11;
                        }
                        if (noteVal > 12)
                            noteVal = 12;
                        if (noteVal < 1)
                            noteVal = 1;

                        int maxlen = Math.Min((remainderOfMeasure - (measureBound - noteVal)), remainderOfMeasure);
                        noteRhythm = randomizer.Next(maxlen) + 1;
                        String noteString = toneRows[i, noteVal-1];
                        if (i == 0)
                        {
                            noteString += "5";
                        }
                        if (i == 1)
                        {
                            noteString += "3";
                        }
                        thisSection.melodies[i].melodicLine.Add(new Song.Note(noteString, noteRhythm));

                        rhythmSum += noteRhythm;
                        prevNoteVal = noteVal;
                        if (farthestPos < noteVal)
                        {
                            farthestPos = noteVal;
                        }

                    }

                }

            }
                

            output.addSegment(thisSection);
            BlottoBeats.MidiOut.MidiOut outgoing = new BlottoBeats.MidiOut.MidiOut();
            double songLen = outgoing.outputToMidi(output);

            return songLen;

        }

        //NOTE: CURRENTLY ASSUMING VALID/NON-NULL INPUT!!!!! (will crash with invalid input) 
        //TODO (soon, but not priority) check for validity of input
        public double generate(SongParameters paramets)
        {
            Random randomizer = new Random(paramets.seed);
            int mode; // 0 = Major 1 = Minor
            String key;
            String gen;
            String timeSigPattern = ""; //Simple or Compound Meter
            int timeSigQuant = 0; // 2 = Duple, 3 = Triple, etc
            int numpatterns = 0;

            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" }; //array of all note values

            //Select Mode
            mode = randomizer.Next(2);

            //Select Key
            key = notes[randomizer.Next(12)];

            //Set genre
            gen = paramets.genre;
            if (gen == "Classical")
                return generate_Classical(paramets);
            if (gen == "Twelve-tone")
                return generate_TwelveTone(paramets);
            if (gen == "Jazz")
                return generate_Jazz(paramets);
            if (gen == "4-Chord Pop/Rock")
                return generate_4Chord(paramets);

            //Now also sets the genre
            Song output = new Song(paramets.tempo, key, paramets.genre);
            Console.Out.WriteLine(key);
            Console.Out.WriteLine(paramets.genre);

            int randOutput = randomizer.Next(2);
            switch (randOutput)
            {
                case 0:
                    timeSigPattern = "Simple";
                    break;
                case 1:
                    timeSigPattern = "Compound";
                    break;
            }

            timeSigQuant = randomizer.Next(3) + 2;

            Console.Out.WriteLine(timeSigPattern + " " + timeSigQuant);
            //numpatterns is a value between 2 and 6
            numpatterns = randomizer.Next(4) + 2;
            List<Song.SongSegment> patterns = new List<Song.SongSegment>();

            for (int i = 0; i < numpatterns; i++)
            {
                Song.SongSegment thisSection = new Song.SongSegment();
                randOutput = randomizer.Next(8) + 1;
                int measures = randOutput * 4;
                int rep = 0;
                while (rep == 0 || (measures % rep != 0))
                {
                    rep = (randomizer.Next(8) + 1) * 4;
                }
                SongPattern inGeneration = new SongPattern(measures, rep);
                bool prevWasHalf = false;
                for (int j = 0; j < inGeneration.repeatEvery / 4; j++)
                {
                    int numChords = randomizer.Next(6) + 1;
                    String chord = "";
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
                                switch (randOutput)
                                {
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
                    int rhythm = 0;
                    int measureLen = 0;

                    //TODO Add rules for generating rhythm
                    for (int count = 0; count < numChords; count++)
                    {
                        if (timeSigPattern.Equals("Simple"))
                        {
                            if (timeSigQuant == 2)
                            {
                                measureLen = 4;
                            }
                            if (timeSigQuant == 3)
                            {
                                measureLen = 6;

                            }
                            if (timeSigQuant == 4)
                            {
                                measureLen = 8;

                            }
                            if (count == numChords - 6)
                            {
                                rhythm = randomizer.Next(measureLen) + 1;
                            }
                            if (count == numChords - 5)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= measureLen && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && rhythm + sumRhythm % measureLen != 0));

                                }
                            }
                            if (count == numChords - 4)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 3)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 2)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 1)
                            {
                                rhythm = 4 * measureLen - sumRhythm;

                            }
                        }
                        else
                        {
                            if (timeSigQuant == 2)
                            {
                                measureLen = 6;

                            }
                            if (timeSigQuant == 3)
                            {
                                measureLen = 9;

                            }
                            if (timeSigQuant == 4)
                            {
                                measureLen = 12;

                            }
                            if (count == numChords - 6)
                            {
                                rhythm = randomizer.Next(measureLen) + 1;
                            }
                            if (count == numChords - 5)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= measureLen && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && rhythm + sumRhythm % measureLen != 0));

                                }
                            }
                            if (count == numChords - 4)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 3)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 2)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 1)
                            {
                                rhythm = 4 * measureLen - sumRhythm;

                            }
                        }

                        thisSection.chordPattern.Add(generateChord(mode, key, chord[count], 2 * rhythm));
                        sumRhythm += rhythm;
                    }
                    if (chord[chord.Length - 1] == '5')
                    {
                        prevWasHalf = true;
                    }
                }
                composeMelody(thisSection, randomizer, key, mode, timeSigPattern, timeSigQuant);

                patterns.Add(thisSection);
            }

            const int MAXNUMSECTIONS = 8;

            //totalSections is a random number between 1 and 8
            int totalSections = randomizer.Next(MAXNUMSECTIONS) + 1;
            //numReps denotes how many time any section can be repeated in a song denoted by (the total number of sections - the number of available patterns)
            int numReps = totalSections - numpatterns;
            //if the random value says to utilize fewer sections than have been generated, simply make the song the list of all generated sections
            if (numReps < 1)
            {
                for (int i = 0; i < numpatterns; i++)
                {
                    output.addSegment(patterns[i]);
                }
            }

            //otherwise make numReps number of repetitions in the production of the song
            else
            {
                //denotes the number value of the furthest section placed into the song
                int patNum = -1;
                //denotes the number value of the preceding section placed into the song
                int prevSec = -1;
                for (int i = 0; i < totalSections; i++)
                {
                    //if you can't repeat anymore fill out the list
                    if (numReps < 1)
                    {
                        patNum++;
                        output.addSegment(patterns[patNum]);
                        prevSec = patNum;
                    }
                    // if you can repeat
                    else
                    {
                        //and you've already gone through the list, your only option is to repeat
                        if (patNum + 1 == numpatterns)
                        {

                            do
                            {
                                randOutput = randomizer.Next(numpatterns);
                            } while (randOutput == prevSec);

                            numReps--;
                            output.addSegment(patterns[randOutput]);
                            prevSec = randOutput;
                        }
                        //if you haven't gotten all the way through the list, you can keep traversing or repeat
                        else
                        {
                            randOutput = randomizer.Next(2);
                            if (prevSec == 0 || randOutput == 0)
                            {
                                patNum++;
                                output.addSegment(patterns[patNum]);
                                prevSec = patNum;

                            }
                            else
                            {
                                do
                                {
                                    randOutput = randomizer.Next(patNum + 1);
                                } while (randOutput == prevSec);

                                numReps--;
                                output.addSegment(patterns[randOutput]);
                                prevSec = randOutput;

                            }


                        }
                    }


                }


            }



            BlottoBeats.MidiOut.MidiOut outgoing = new BlottoBeats.MidiOut.MidiOut();
            double songLen = outgoing.outputToMidi(output);

            return songLen;
        }


        public double generate_Classical(SongParameters paramets)
        {
            Random randomizer = new Random(paramets.seed);
            int mode; // 0 = Major 1 = Minor
            String key;
            String gen;
            String timeSigPattern = ""; //Simple or Compound Meter
            int timeSigQuant = 0; // 2 = Duple, 3 = Triple, etc
            int numpatterns = 0;

            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" }; //array of all note values

            //Select Mode
            mode = randomizer.Next(2);

            //Select Key
            key = notes[randomizer.Next(12)];

            //Set genre
            gen = paramets.genre;

            //Now also sets the genre
            Song output = new Song(paramets.tempo, key, paramets.genre);
            Console.Out.WriteLine(key);
            Console.Out.WriteLine(paramets.genre);

            int randOutput = randomizer.Next(2);
            switch (randOutput)
            {
                case 0:
                    timeSigPattern = "Simple";
                    break;
                case 1:
                    timeSigPattern = "Compound";
                    break;
            }

            timeSigQuant = randomizer.Next(3) + 2;

            Console.Out.WriteLine(timeSigPattern + " " + timeSigQuant);
            //numpatterns is a value between 2 and 6
            numpatterns = randomizer.Next(4) + 2;
            List<Song.SongSegment> patterns = new List<Song.SongSegment>();

            for (int i = 0; i < numpatterns; i++)
            {
                Song.SongSegment thisSection = new Song.SongSegment();
                randOutput = randomizer.Next(8) + 1;
                int measures = randOutput;
                int rep = 0;
                while (rep == 0 || (measures % rep != 0))
                {
                    rep = (randomizer.Next(8) + 1);
                }
                SongPattern inGeneration = new SongPattern(measures, rep);
                bool prevWasHalf = false;
                for (int j = 0; j < inGeneration.repeatEvery; j++)
                {
                    int numChords = 4;
                    String chord = "";
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
                                switch (randOutput)
                                {
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
                                    case 6:
                                        chord = "251";
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
                            if (j != (inGeneration.repeatEvery) - 1)
                            {
                                randOutput = randomizer.Next(20);
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
                                    case 12:
                                        chord = "6251";
                                        break;
                                    case 13:
                                        chord = "4251";
                                        break;
                                    case 14:
                                        chord = "4271";
                                        break;
                                    case 15:
                                        chord = "2715";
                                        break;
                                    case 16:
                                        chord = "2515";
                                        break;
                                    case 17:
                                        chord = "3451";
                                        break;
                                    case 18:
                                        chord = "3471";
                                        break;
                                    case 19:
                                        chord = "3425";
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
                    int rhythm = 0;
                    int measureLen = 0;

                    //TODO Add rules for generating rhythm
                    for (int count = 0; count < numChords; count++)
                    {
                        if (timeSigPattern.Equals("Simple"))
                        {
                            if (timeSigQuant == 2)
                            {
                                measureLen = 4;
                            }
                            if (timeSigQuant == 3)
                            {
                                measureLen = 6;

                            }
                            if (timeSigQuant == 4)
                            {
                                measureLen = 8;

                            }
                            if (count == numChords - 6)
                            {
                                rhythm = randomizer.Next(measureLen) + 1;
                            }
                            if (count == numChords - 5)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= measureLen && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && rhythm + sumRhythm % measureLen != 0));

                                }
                            }
                            if (count == numChords - 4)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 3)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 2)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 1)
                            {
                                rhythm = 4 * measureLen - sumRhythm;

                            }
                        }
                        else
                        {
                            if (timeSigQuant == 2)
                            {
                                measureLen = 6;

                            }
                            if (timeSigQuant == 3)
                            {
                                measureLen = 9;

                            }
                            if (timeSigQuant == 4)
                            {
                                measureLen = 12;

                            }
                            if (count == numChords - 6)
                            {
                                rhythm = randomizer.Next(measureLen) + 1;
                            }
                            if (count == numChords - 5)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= measureLen && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && rhythm + sumRhythm % measureLen != 0));

                                }
                            }
                            if (count == numChords - 4)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 3)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 2)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 1)
                            {
                                rhythm = 4 * measureLen - sumRhythm;

                            }
                        }

                        thisSection.chordPattern.Add(generateChord(mode, key, chord[count], 2 * rhythm));
                        sumRhythm += rhythm;
                    }
                    if (chord[chord.Length - 1] == '5')
                    {
                        prevWasHalf = true;
                    }
                }
                composeClassicalMelody(thisSection, randomizer, key, mode, timeSigPattern, timeSigQuant, paramets.tempo);

                patterns.Add(thisSection);
            }

            const int MAXNUMSECTIONS = 8;

            //totalSections is a random number between 1 and 8
            int totalSections = randomizer.Next(MAXNUMSECTIONS) + 1;
            //numReps denotes how many time any section can be repeated in a song denoted by (the total number of sections - the number of available patterns)
            int numReps = totalSections - numpatterns;
            //if the random value says to utilize fewer sections than have been generated, simply make the song the list of all generated sections
            if (numReps < 1)
            {
                for (int i = 0; i < numpatterns; i++)
                {
                    output.addSegment(patterns[i]);
                }
            }

            //otherwise make numReps number of repetitions in the production of the song
            else
            {
                //denotes the number value of the furthest section placed into the song
                int patNum = -1;
                //denotes the number value of the preceding section placed into the song
                int prevSec = -1;
                for (int i = 0; i < totalSections; i++)
                {
                    //if you can't repeat anymore fill out the list
                    if (numReps < 1)
                    {
                        patNum++;
                        output.addSegment(patterns[patNum]);
                        prevSec = patNum;
                    }
                    // if you can repeat
                    else
                    {
                        //and you've already gone through the list, your only option is to repeat
                        if (patNum + 1 == numpatterns)
                        {

                            do
                            {
                                randOutput = randomizer.Next(numpatterns);
                            } while (randOutput == prevSec);

                            numReps--;
                            output.addSegment(patterns[randOutput]);
                            prevSec = randOutput;
                        }
                        //if you haven't gotten all the way through the list, you can keep traversing or repeat
                        else
                        {
                            randOutput = randomizer.Next(2);
                            if (prevSec == 0 || randOutput == 0)
                            {
                                patNum++;
                                output.addSegment(patterns[patNum]);
                                prevSec = patNum;

                            }
                            else
                            {
                                do
                                {
                                    randOutput = randomizer.Next(patNum + 1);
                                } while (randOutput == prevSec);

                                numReps--;
                                output.addSegment(patterns[randOutput]);
                                prevSec = randOutput;

                            }


                        }
                    }


                }


            }

            BlottoBeats.MidiOut.MidiOut outgoing = new BlottoBeats.MidiOut.MidiOut();
            double songLen = outgoing.outputToMidi(output);

            return songLen;
        }

        public double generate_Jazz(SongParameters paramets)
        {
            Random randomizer = new Random(paramets.seed);
            int mode; // 0 = Major 1 = Minor
            String key;
            String gen;
            String timeSigPattern = ""; //Simple or Compound Meter
            int timeSigQuant = 0; // 2 = Duple, 3 = Triple, etc
            int numpatterns = 0;

            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" }; //array of all note values

            //Select Mode
            mode = randomizer.Next(2);

            //Select Key
            key = notes[randomizer.Next(12)];

            //Set genre
            gen = paramets.genre;

            //Now also sets the genre
            Song output = new Song(paramets.tempo, key, paramets.genre);
            Console.Out.WriteLine(key);
            Console.Out.WriteLine(paramets.genre);

            int randOutput = randomizer.Next(2);
            switch (randOutput)
            {
                case 0:
                    timeSigPattern = "Simple";
                    break;
                case 1:
                    timeSigPattern = "Compound";
                    break;
            }

            timeSigQuant = randomizer.Next(3) + 2;

            Console.Out.WriteLine(timeSigPattern + " " + timeSigQuant);
            //numpatterns is a value between 2 and 6
            numpatterns = randomizer.Next(4) + 2;
            List<Song.SongSegment> patterns = new List<Song.SongSegment>();

            for (int i = 0; i < numpatterns; i++)
            {
                Song.SongSegment thisSection = new Song.SongSegment();
                randOutput = randomizer.Next(8) + 1;
                int measures = randOutput * 4;
                int rep = 0;
                while (rep == 0 || (measures % rep != 0))
                {
                    rep = (randomizer.Next(8) + 1) * 4;
                }
                SongPattern inGeneration = new SongPattern(measures, rep);
                bool prevWasHalf = false;
                for (int j = 0; j < inGeneration.repeatEvery / 4; j++)
                {
                    int numChords = randomizer.Next(6) + 1;
                    String chord = "";
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
                                switch (randOutput)
                                {
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
                    int rhythm = 0;
                    int measureLen = 0;

                    //TODO Add rules for generating rhythm
                    for (int count = 0; count < numChords; count++)
                    {
                        if (timeSigPattern.Equals("Simple"))
                        {
                            if (timeSigQuant == 2)
                            {
                                measureLen = 4;
                            }
                            if (timeSigQuant == 3)
                            {
                                measureLen = 6;

                            }
                            if (timeSigQuant == 4)
                            {
                                measureLen = 8;

                            }
                            if (count == numChords - 6)
                            {
                                rhythm = randomizer.Next(measureLen) + 1;
                            }
                            if (count == numChords - 5)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= measureLen && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && rhythm + sumRhythm % measureLen != 0));

                                }
                            }
                            if (count == numChords - 4)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 3)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 2)
                            {
                                if (sumRhythm % 2 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 2 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 1)
                            {
                                rhythm = 4 * measureLen - sumRhythm;

                            }
                        }
                        else
                        {
                            if (timeSigQuant == 2)
                            {
                                measureLen = 6;

                            }
                            if (timeSigQuant == 3)
                            {
                                measureLen = 9;

                            }
                            if (timeSigQuant == 4)
                            {
                                measureLen = 12;

                            }
                            if (count == numChords - 6)
                            {
                                rhythm = randomizer.Next(measureLen) + 1;
                            }
                            if (count == numChords - 5)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= measureLen && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(2 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && rhythm + sumRhythm % measureLen != 0));

                                }
                            }
                            if (count == numChords - 4)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 3)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(3 * measureLen - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 2)
                            {
                                if (sumRhythm % 3 != 0)
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0) || (rhythm + sumRhythm <= (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % 3 != 0));

                                }
                                else
                                {
                                    do
                                    {
                                        rhythm = randomizer.Next(4 * measureLen - 1 - sumRhythm) + 1;
                                    } while ((rhythm + sumRhythm > (sumRhythm + (measureLen - sumRhythm % measureLen)) && (rhythm + sumRhythm) % measureLen != 0));

                                }

                            }
                            if (count == numChords - 1)
                            {
                                rhythm = 4 * measureLen - sumRhythm;

                            }
                        }

                        thisSection.chordPattern.Add(generateJazzChord(mode, key, chord[count], 2 * rhythm));
                        sumRhythm += rhythm;
                    }
                    /*if (chord[chord.Length - 1] == '5')
                    {
                        prevWasHalf = true;
                    }*/
                }
                composeJazzMelody(thisSection, randomizer, key, mode, timeSigPattern, timeSigQuant, paramets.tempo);

                patterns.Add(thisSection);
            }

            const int MAXNUMSECTIONS = 8;

            //totalSections is a random number between 1 and 8
            int totalSections = randomizer.Next(MAXNUMSECTIONS) + 1;
            //numReps denotes how many time any section can be repeated in a song denoted by (the total number of sections - the number of available patterns)
            int numReps = totalSections - numpatterns;
            //if the random value says to utilize fewer sections than have been generated, simply make the song the list of all generated sections
            if (numReps < 1)
            {
                for (int i = 0; i < numpatterns; i++)
                {
                    output.addSegment(patterns[i]);
                }
            }

            //otherwise make numReps number of repetitions in the production of the song
            else
            {
                //denotes the number value of the furthest section placed into the song
                int patNum = -1;
                //denotes the number value of the preceding section placed into the song
                int prevSec = -1;
                for (int i = 0; i < totalSections; i++)
                {
                    //if you can't repeat anymore fill out the list
                    if (numReps < 1)
                    {
                        patNum++;
                        output.addSegment(patterns[patNum]);
                        prevSec = patNum;
                    }
                    // if you can repeat
                    else
                    {
                        //and you've already gone through the list, your only option is to repeat
                        if (patNum + 1 == numpatterns)
                        {

                            do
                            {
                                randOutput = randomizer.Next(numpatterns);
                            } while (randOutput == prevSec);

                            numReps--;
                            output.addSegment(patterns[randOutput]);
                            prevSec = randOutput;
                        }
                        //if you haven't gotten all the way through the list, you can keep traversing or repeat
                        else
                        {
                            randOutput = randomizer.Next(2);
                            if (prevSec == 0 || randOutput == 0)
                            {
                                patNum++;
                                output.addSegment(patterns[patNum]);
                                prevSec = patNum;

                            }
                            else
                            {
                                do
                                {
                                    randOutput = randomizer.Next(patNum + 1);
                                } while (randOutput == prevSec);

                                numReps--;
                                output.addSegment(patterns[randOutput]);
                                prevSec = randOutput;

                            }


                        }
                    }


                }


            }



            BlottoBeats.MidiOut.MidiOut outgoing = new BlottoBeats.MidiOut.MidiOut();
            double songLen = outgoing.outputToMidi(output);

            return songLen;
        }


        private static Song.Chord generateChord(int mode, String key, char chord, int length)
        {
            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            String[] keySig = new String[7];
            String[] noteNames = new String[4];
            int chordNumIndex = int.Parse(new String(chord, 1)) - 1;
            int keynum = Array.IndexOf(notes, key);
            //generate chord in major mode
            if (mode == 0)
            {
                keySig[0] = notes[keynum];
                keySig[1] = notes[(keynum + 2) % 12];
                keySig[2] = notes[(keynum + 4) % 12];
                keySig[3] = notes[(keynum + 5) % 12];
                keySig[4] = notes[(keynum + 7) % 12];
                keySig[5] = notes[(keynum + 9) % 12];
                keySig[6] = notes[(keynum + 11) % 12];

                noteNames[0] = keySig[chordNumIndex] + "3";
                noteNames[1] = keySig[chordNumIndex] + "4";
                noteNames[2] = keySig[(chordNumIndex + 2) % 7] + "4";
                noteNames[3] = keySig[(chordNumIndex + 4) % 7] + "4";
                Console.Out.WriteLine(chordNumIndex + 1 + " " + length);
                return new Song.Chord(noteNames, length, chordNumIndex + 1);
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

                //if not a dominant function
                if (chordNumIndex != 4 && chordNumIndex != 6)
                {
                    noteNames[0] = keySig[chordNumIndex] + "3";
                    noteNames[1] = keySig[chordNumIndex] + "4";
                    noteNames[2] = keySig[(chordNumIndex + 2) % 7] + "4";
                    noteNames[3] = keySig[(chordNumIndex + 4) % 7] + "4";
                }
                //if dominant
                if (chordNumIndex == 4)
                {
                    noteNames[0] = keySig[chordNumIndex] + "3";
                    noteNames[1] = keySig[chordNumIndex] + "4";
                    noteNames[3] = keySig[(chordNumIndex + 4) % 7] + "4";
                    //2nd note of the triad is raised a half step
                    noteNames[2] = notes[(Array.IndexOf(notes, keySig[(chordNumIndex + 2) % 7]) + 1) % 12] + "4";

                }
                //if leading tone
                if (chordNumIndex == 6)
                {
                    //root of the triad is raised a half step
                    noteNames[0] = notes[(Array.IndexOf(notes, keySig[(chordNumIndex)]) + 1) % 12] + "3";
                    noteNames[1] = notes[(Array.IndexOf(notes, keySig[(chordNumIndex)]) + 1) % 12] + "4";
                    noteNames[2] = keySig[(chordNumIndex + 2) % 7] + "4";
                    noteNames[3] = keySig[(chordNumIndex + 4) % 7] + "4";

                }

                Console.Out.WriteLine(chordNumIndex + 1 + " " + length);
                return new Song.Chord(noteNames, length, chordNumIndex + 1);
            }
            return null;

        }

        private static Song.Chord generateJazzChord(int mode, String key, char chord, int length)
        {
            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            String[] keySig = new String[7];
            String[] noteNames = new String[4];
            int chordNumIndex = int.Parse(new String(chord, 1)) - 1;
            int keynum = Array.IndexOf(notes, key);
            //generate chord in major mode
            if (mode == 0)
            {
                //TEST THESE
                keySig[0] = notes[keynum];
                keySig[1] = notes[(keynum + 2) % 12];
                keySig[2] = notes[(keynum + 4) % 12];
                keySig[3] = notes[(keynum + 5) % 12];
                keySig[4] = notes[(keynum + 7) % 12];
                keySig[5] = notes[(keynum + 9) % 12];
                keySig[6] = notes[(keynum + 11) % 12];

                noteNames[0] = keySig[chordNumIndex] + "2";
                noteNames[1] = keySig[chordNumIndex] + "4";
                noteNames[2] = keySig[(chordNumIndex + 2) % 7] + "4";
                noteNames[3] = keySig[(chordNumIndex + 6) % 7] + "4";
                Console.Out.WriteLine(chordNumIndex + 1 + " " + length);
                return new Song.Chord(noteNames, length, chordNumIndex + 1);
            }


            //generate chord in minor mode
            if (mode == 1)
            {
                //TEST THESE
                keySig[0] = notes[keynum];
                keySig[1] = notes[(keynum + 2) % 12];
                keySig[2] = notes[(keynum + 3) % 12];
                keySig[3] = notes[(keynum + 5) % 12];
                keySig[4] = notes[(keynum + 7) % 12];
                keySig[5] = notes[(keynum + 8) % 12];
                keySig[6] = notes[(keynum + 10) % 12];

                //if not a dominant function
                if (chordNumIndex != 4 && chordNumIndex != 6)
                {
                    noteNames[0] = keySig[chordNumIndex] + "2";
                    noteNames[1] = keySig[chordNumIndex] + "4";
                    noteNames[2] = keySig[(chordNumIndex + 2) % 7] + "4";
                    noteNames[3] = keySig[(chordNumIndex + 6) % 7] + "4";
                }
                //if dominant
                if (chordNumIndex == 4)
                {
                    noteNames[0] = keySig[chordNumIndex] + "2";
                    noteNames[1] = keySig[chordNumIndex] + "4";
                    noteNames[3] = keySig[(chordNumIndex + 6) % 7] + "4";
                    //2nd note of the triad is raised a half step
                    //noteNames[2] = notes[(Array.IndexOf(notes, keySig[(chordNumIndex + 2) % 7]) + 1) % 12] + "4";
                    noteNames[2] = notes[(Array.IndexOf(notes, keySig[(chordNumIndex + 2) % 7]) + 1) % 12] + "4";

                }
                //if leading tone
                if (chordNumIndex == 6)
                {
                    //root of the triad is raised a half step
                    noteNames[0] = notes[(Array.IndexOf(notes, keySig[(chordNumIndex)]) + 1) % 12] + "2";
                    noteNames[1] = notes[(Array.IndexOf(notes, keySig[(chordNumIndex)]) + 1) % 12] + "4";
                    noteNames[2] = keySig[(chordNumIndex + 2) % 7] + "4";
                    noteNames[3] = keySig[(chordNumIndex + 6) % 7] + "4";

                }

                Console.Out.WriteLine(chordNumIndex + 1 + " " + length);
                return new Song.Chord(noteNames, length, chordNumIndex + 1);
            }
            return null;

        }

        /*static void Main(string[] args)
        {
            SongParameters input = new SongParameters();
            input.tempo = 100;
            input.genre = "Nope";
            input.ID = -1;
            int seed = 3000;
            generate(seed, input);
        }*/
        //composes a melody for a SongSegment. This SongSegment is assumed to not already have a melody
        void composeMelody(Song.SongSegment thisSection, Random randomizer, String key, int mode, String timeSigPattern, int timeSigQuant)
        {
            //TODO check for bad input
            int indexer = thisSection.melodies.Count;
            int chordLength;
            int currentSum = 0;
            String prevNoteVal = "";
            String noteVal = "";
            int noteRhythm = 0;
            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            String[] keySig = new String[7];
            String[] notearray = new String[14];
            int keynum = Array.IndexOf(notes, key);
            thisSection.melodies.Add(new Song.Melody());

            if (mode == 0)
            {
                keySig[0] = notes[keynum];
                keySig[1] = notes[(keynum + 2) % 12];
                keySig[2] = notes[(keynum + 4) % 12];
                keySig[3] = notes[(keynum + 5) % 12];
                keySig[4] = notes[(keynum + 7) % 12];
                keySig[5] = notes[(keynum + 9) % 12];
                keySig[6] = notes[(keynum + 11) % 12];

            }
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

            int octave = 4;
            String lastNote = "";
            String nextNote = "";
            int thisChord = 0;
            int nextChord = 0;
            for (int i = 0; i < 14; i++)
            {
                nextNote = keySig[i % 7];
                if (!lastNote.Equals("") && !nextNote.Equals("") && lastNote[0] < 'C' && nextNote[0] >= 'C')
                {
                    octave++;
                }
                notearray[i] = nextNote + octave.ToString();
                lastNote = nextNote;
            }
            int totalSectionSum = 0;
            int measureLen = 0;
            if (timeSigPattern.Equals("Simple"))
            {
                measureLen += 4;
            }
            else
                measureLen += 6;
            measureLen *= timeSigQuant;
            for (int i = 0; i < thisSection.chordPattern.Count; i++)
            {
                chordLength = thisSection.chordPattern[i].chordVoice.First().length;
                currentSum = 0;
                while (currentSum < chordLength)
                {

                    //Randomly pick a length within the current measure that doesn't overlap chords
                    int maxVal = Math.Min(measureLen - (totalSectionSum % measureLen), chordLength - currentSum);
                    //If a melody falls onto the last chord of a segment just make the rhythm of note #1 the length of the chord
                    if (i == thisSection.chordPattern.Count - 1)
                    {
                        noteRhythm = chordLength;
                    }
                    else
                    {
                        int[] rhythmWeight = new int[maxVal];
                        for (int j = 0; j < maxVal; j++)
                        {
                            int beatSize = measureLen / timeSigQuant;
                            if (j > beatSize)
                            {
                                rhythmWeight[j] = 4;

                            }
                            else
                            {
                                rhythmWeight[j] = 1;

                            }
                            if (maxVal < beatSize)
                            {
                                rhythmWeight[maxVal - 1] = 10;

                            }
                            if (j + 1 % beatSize == 0)
                            {
                                rhythmWeight[j] *= 4;

                            }
                            if (j + 1 == beatSize)
                            {
                                rhythmWeight[j] *= 4;

                            }

                        }
                        int sumRythWeights = 0;
                        for (int k = 0; k < maxVal; k++)
                        {
                            sumRythWeights += rhythmWeight[k];
                        }

                        int randOutput = randomizer.Next(sumRythWeights);
                        sumRythWeights = 0;
                        for (int k = 0; k < maxVal; k++)
                        {
                            sumRythWeights += rhythmWeight[k];
                            if (randOutput < sumRythWeights)
                            {
                                noteRhythm = k + 1;
                                break;
                            }

                        }
                    }



                    //Define noteValue for each note
                    int[] noteWeights = new int[14];
                    if (prevNoteVal.Equals(""))
                    {
                        for (int j = 0; j < 14; j++)
                        {
                            noteWeights[j] = 1;

                        }

                    }
                    else
                    {
                        int index = Array.IndexOf(notearray, prevNoteVal);
                        int difference = 0;
                        for (int j = 0; j < 14; j++)
                        {
                            difference = index - j;
                            difference = Math.Abs(difference);
                            switch (difference)
                            {
                                case 0:
                                    noteWeights[j] = 16;
                                    break;
                                case 1:
                                    noteWeights[j] = 14;
                                    break;
                                case 2:
                                    noteWeights[j] = 10;
                                    break;
                                case 3:
                                    noteWeights[j] = 3;
                                    break;
                                case 4:
                                    noteWeights[j] = 4;
                                    break;
                                case 5:
                                    noteWeights[j] = 3;
                                    break;
                                case 6:
                                    noteWeights[j] = 2;
                                    break;
                                case 7:
                                    noteWeights[j] = 4;
                                    break;
                                case 8:
                                    noteWeights[j] = 1;
                                    break;
                                case 9:
                                    noteWeights[j] = 2;
                                    break;
                                case 10:
                                    noteWeights[j] = 1;
                                    break;
                                case 11:
                                    noteWeights[j] = 1;
                                    break;
                                case 12:
                                    noteWeights[j] = 1;
                                    break;
                                case 13:
                                    noteWeights[j] = 1;
                                    break;


                            }


                        }

                    }

                    //Defines the current chord and the upcoming chord for usage below
                    thisChord = thisSection.chordPattern[i].chordVal;
                    if (i < thisSection.chordPattern.Count - 1)
                    {
                        nextChord = thisSection.chordPattern[i + 1].chordVal;

                    }
                    else
                    {
                        nextChord = 0;

                    }

                    if (currentSum == 0)
                    {
                        //If first and last note of the chord
                        if (currentSum + noteRhythm == chordLength)
                        {
                            //if chord is last chord weight towards chord
                            if (nextChord == 0)
                            {
                                int indexerForChordVals = thisChord - 1;
                                for (int j = 0; j < 6; j++)
                                {
                                    noteWeights[indexerForChordVals] *= 2;
                                    if (j == 2)
                                        indexerForChordVals += 3;
                                    else
                                        indexerForChordVals += 2;
                                    indexerForChordVals %= 14;
                                }

                            }
                            //Otherwise weight towards notes that lead into the next chord
                            if (nextChord == 1)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[6] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[13] *= 3;
                            }
                            if (nextChord == 2)
                            {
                                noteWeights[0] *= 3;
                                noteWeights[2] *= 3;
                                noteWeights[4] *= 3;
                                noteWeights[7] *= 3;
                                noteWeights[9] *= 3;
                                noteWeights[11] *= 3;
                            }
                            if (nextChord == 3)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[5] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[12] *= 3;
                                noteWeights[0] *= 3;
                                noteWeights[7] *= 3;
                            }
                            if (nextChord == 4)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[2] *= 3;
                                noteWeights[4] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[9] *= 3;
                                noteWeights[11] *= 3;
                            }
                            if (nextChord == 5)
                            {
                                noteWeights[0] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[5] *= 3;
                                noteWeights[7] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[12] *= 3;
                            }
                            if (nextChord == 6)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[6] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[13] *= 3;
                            }
                            if (nextChord == 7)
                            {
                                noteWeights[0] *= 3;
                                noteWeights[2] *= 3;
                                noteWeights[5] *= 3;
                                noteWeights[7] *= 3;
                                noteWeights[9] *= 3;
                                noteWeights[12] *= 3;
                            }


                        }
                        if (thisChord == 1)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[2] *= 2;
                            noteWeights[4] *= 1;
                            noteWeights[7] *= 3;
                            noteWeights[9] *= 2;
                            noteWeights[11] *= 1;
                        }
                        if (thisChord == 2)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 1;
                            noteWeights[5] *= 2;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 1;
                            noteWeights[12] *= 2;
                        }
                        if (thisChord == 3)
                        {
                            noteWeights[2] *= 3;
                            noteWeights[4] *= 2;
                            noteWeights[6] *= 1;
                            noteWeights[9] *= 3;
                            noteWeights[11] *= 2;
                            noteWeights[13] *= 1;
                        }
                        if (thisChord == 4)
                        {
                            noteWeights[3] *= 3;
                            noteWeights[5] *= 1;
                            noteWeights[7] *= 2;
                            noteWeights[10] *= 3;
                            noteWeights[12] *= 1;
                            noteWeights[0] *= 2;
                        }
                        if (thisChord == 5)
                        {
                            noteWeights[4] *= 1;
                            noteWeights[6] *= 2;
                            noteWeights[8] *= 3;
                            noteWeights[11] *= 1;
                            noteWeights[13] *= 2;
                            noteWeights[1] *= 3;
                        }
                        if (thisChord == 6)
                        {
                            noteWeights[5] *= 2;
                            noteWeights[7] *= 1;
                            noteWeights[9] *= 3;
                            noteWeights[12] *= 2;
                            noteWeights[0] *= 1;
                            noteWeights[2] *= 3;
                        }
                        if (thisChord == 7)
                        {
                            noteWeights[6] *= 2;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 1;
                            noteWeights[13] *= 2;
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 1;
                        }
                        //if the note is not a chord tone, set the weight to be 0
                        for (int j = 0; j < 14; j++)
                        {
                            int thisChordIndex = thisChord - 1;
                            if (j != thisChordIndex && j != thisChordIndex + 2 && j != thisChordIndex + 4 && j != thisChordIndex + 7 && j != (thisChordIndex + 9) % 13 && j != (thisChordIndex + 11) % 13)
                            {
                                noteWeights[j] = 0;
                            }
                        }


                    }
                    else if (currentSum + noteRhythm == chordLength)
                    {

                        //weight towards notes that lead into the next chord
                        if (nextChord == 1)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[6] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[13] *= 3;
                        }
                        if (nextChord == 2)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[2] *= 3;
                            noteWeights[4] *= 3;
                            noteWeights[7] *= 3;
                            noteWeights[9] *= 3;
                            noteWeights[11] *= 3;
                        }
                        if (nextChord == 3)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[5] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[12] *= 3;
                            noteWeights[0] *= 3;
                            noteWeights[7] *= 3;
                        }
                        if (nextChord == 4)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[2] *= 3;
                            noteWeights[4] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[9] *= 3;
                            noteWeights[11] *= 3;
                        }
                        if (nextChord == 5)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[5] *= 3;
                            noteWeights[7] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[12] *= 3;
                        }
                        if (nextChord == 6)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[6] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[13] *= 3;
                        }
                        if (nextChord == 7)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[2] *= 3;
                            noteWeights[5] *= 3;
                            noteWeights[7] *= 3;
                            noteWeights[9] *= 3;
                            noteWeights[12] *= 3;
                        }
                    }
                    //If a note isn't first or last of a chord. Double it's weighting towards chord tones.
                    else
                    {
                        int indexerForChordVals = thisChord - 1;
                        for (int j = 0; j < 6; j++)
                        {
                            noteWeights[indexerForChordVals] *= 2;
                            if (j == 2)
                                indexerForChordVals += 3;
                            else
                                indexerForChordVals += 2;
                            indexerForChordVals %= 14;
                        }

                    }
                    //Selects a note randomly based upon weighting
                    int sumWeights = 0;
                    for (int j = 0; j < 14; j++)
                    {
                        sumWeights += noteWeights[j];
                    }

                    int randOut = randomizer.Next(sumWeights);
                    sumWeights = 0;
                    for (int j = 0; j < 14; j++)
                    {
                        sumWeights += noteWeights[j];
                        if (randOut < sumWeights)
                        {
                            noteVal = notearray[j];
                            break;
                        }

                    }
                    //Raise and lower leading tones in dominant functions of minor modes in melodic lines
                    if (mode == 1)
                    {
                        if (thisChord == 5 || thisChord == 7)
                        {
                            if (noteVal.Equals(notearray[6]) || noteVal.Equals(notearray[13]))
                            {
                                if (noteVal[1] == '#')
                                {
                                    octave = int.Parse(noteVal[2].ToString());
                                    String sub = noteVal.Substring(0, 2);
                                    int index = Array.IndexOf(notes, sub);
                                    noteVal = notes[(index + 1)%12] + octave.ToString();
                                }
                                else
                                {
                                    octave = int.Parse(noteVal[1].ToString());
                                    String sub = noteVal.Substring(0, 1);
                                    int index = Array.IndexOf(notes, sub);
                                    if (sub.Equals("B"))
                                    {
                                        octave++;
                                    }
                                    noteVal = notes[(index + 1) % 12] + octave.ToString();
                                }
                            }
                        }

                    }
                    thisSection.melodies[indexer].melodicLine.Add(new Song.Note(noteVal, noteRhythm));
                    currentSum += noteRhythm;
                    totalSectionSum += noteRhythm;
                    prevNoteVal = noteVal;
                }
            }
        }

        void composeClassicalMelody(Song.SongSegment thisSection, Random randomizer, String key, int mode, String timeSigPattern, int timeSigQuant, int tempo)
        {

            
            int chordLength;
            int currentSum = 0;
            String prevNoteVal = "";
            String noteVal = "";
            int noteRhythm = 0;
            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            String[] keySig = new String[7];
            String[] notearray = new String[14];
            int keynum = Array.IndexOf(notes, key);
            thisSection.melodies.Add(new Song.Melody());

            if (mode == 0)
            {
                keySig[0] = notes[keynum];
                keySig[1] = notes[(keynum + 2) % 12];
                keySig[2] = notes[(keynum + 4) % 12];
                keySig[3] = notes[(keynum + 5) % 12];
                keySig[4] = notes[(keynum + 7) % 12];
                keySig[5] = notes[(keynum + 9) % 12];
                keySig[6] = notes[(keynum + 11) % 12];

            }
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

            int octave = 3;
            String lastNote = "";
            String nextNote = "";
            int thisChord = 0;
            int nextChord = 0;
            for (int i = 0; i < 14; i++)
            {
                nextNote = keySig[i % 7];
                if (!lastNote.Equals("") && !nextNote.Equals("") && lastNote[0] < 'C' && nextNote[0] >= 'C')
                {
                    octave++;
                }
                notearray[i] = nextNote + octave.ToString();
                lastNote = nextNote;
            }
            int totalSectionSum = 0;
            int measureLen = 0;
            if (timeSigPattern.Equals("Simple"))
            {
                measureLen += 4;
            }
            else
                measureLen += 6;
            measureLen *= timeSigQuant;
            for (int i = 0; i < thisSection.chordPattern.Count; i++)
            {
                chordLength = thisSection.chordPattern[i].chordVoice.First().length;
                currentSum = 0;
                while (currentSum < chordLength)
                {

                    //Randomly pick a length within the current measure that doesn't overlap chords
                    //int maxVal = Math.Min(measureLen - (totalSectionSum % measureLen), chordLength - currentSum);
                    int maxVal = randomizer.Next(3, 20);
                    //If a melody falls onto the last chord of a segment just make the rhythm of note #1 the length of the chord
                    if (i == thisSection.chordPattern.Count - 1)
                    {
                        noteRhythm = chordLength;
                    }
                    else
                    {
                        int[] rhythmWeight = new int[maxVal];
                        for (int j = 0; j < maxVal; j++)
                        {
                            int beatSize = measureLen / timeSigQuant;
                            if (j > beatSize)
                            {
                                rhythmWeight[j] = 4;

                            }
                            else
                            {
                                rhythmWeight[j] = 1;

                            }
                            if (maxVal < beatSize)
                            {
                                rhythmWeight[maxVal - 1] = 10;

                            }
                            if (j + 1 % beatSize == 0)
                            {
                                rhythmWeight[j] *= 4;

                            }
                            if (j + 1 == beatSize)
                            {
                                rhythmWeight[j] *= 4;

                            }

                        }
                        int sumRythWeights = 0;
                        for (int k = 0; k < maxVal; k++)
                        {
                            sumRythWeights += rhythmWeight[k];
                        }

                        int randOutput = randomizer.Next(sumRythWeights);
                        sumRythWeights = 0;
                        for (int k = 0; k < maxVal; k++)
                        {
                            sumRythWeights += rhythmWeight[k];
                            if (randOutput < sumRythWeights)
                            {
                                noteRhythm = k + 1;
                                break;
                            }

                        }
                    }



                    //Define noteValue for each note
                    int[] noteWeights = new int[14];
                    if (prevNoteVal.Equals(""))
                    {
                        for (int j = 0; j < 14; j++)
                        {
                            noteWeights[j] = 1;

                        }

                    }
                    else
                    {
                        int index = Array.IndexOf(notearray, prevNoteVal);
                        int difference = 0;
                        for (int j = 0; j < 14; j++)
                        {
                            difference = index - j;
                            difference = Math.Abs(difference);
                            switch (difference)
                            {
                                case 0:
                                    noteWeights[j] = 16;
                                    break;
                                case 1:
                                    noteWeights[j] = 14;
                                    break;
                                case 2:
                                    noteWeights[j] = 10;
                                    break;
                                case 3:
                                    noteWeights[j] = 3;
                                    break;
                                case 4:
                                    noteWeights[j] = 4;
                                    break;
                                case 5:
                                    noteWeights[j] = 3;
                                    break;
                                case 6:
                                    noteWeights[j] = 2;
                                    break;
                                case 7:
                                    noteWeights[j] = 4;
                                    break;
                                case 8:
                                    noteWeights[j] = 1;
                                    break;
                                case 9:
                                    noteWeights[j] = 2;
                                    break;
                                case 10:
                                    noteWeights[j] = 1;
                                    break;
                                case 11:
                                    noteWeights[j] = 1;
                                    break;
                                case 12:
                                    noteWeights[j] = 1;
                                    break;
                                case 13:
                                    noteWeights[j] = 1;
                                    break;


                            }


                        }

                    }

                    //Defines the current chord and the upcoming chord for usage below
                    thisChord = thisSection.chordPattern[i].chordVal;
                    if (i < thisSection.chordPattern.Count - 1)
                    {
                        nextChord = thisSection.chordPattern[i + 1].chordVal;

                    }
                    else
                    {
                        nextChord = 0;

                    }

                    if (currentSum == 0)
                    {
                        //If first and last note of the chord
                        if (currentSum + noteRhythm == chordLength)
                        {
                            //if chord is last chord weight towards chord
                            if (nextChord == 0)
                            {
                                int indexerForChordVals = thisChord - 1;
                                for (int j = 0; j < 6; j++)
                                {
                                    noteWeights[indexerForChordVals] *= 2;
                                    if (j == 2)
                                        indexerForChordVals += 3;
                                    else
                                        indexerForChordVals += 2;
                                    indexerForChordVals %= 14;
                                }

                            }
                            //Otherwise weight towards notes that lead into the next chord
                            if (nextChord == 1)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[6] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[13] *= 3;
                            }
                            if (nextChord == 2)
                            {
                                noteWeights[0] *= 3;
                                noteWeights[2] *= 3;
                                noteWeights[4] *= 3;
                                noteWeights[7] *= 3;
                                noteWeights[9] *= 3;
                                noteWeights[11] *= 3;
                            }
                            if (nextChord == 3)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[5] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[12] *= 3;
                                noteWeights[0] *= 3;
                                noteWeights[7] *= 3;
                            }
                            if (nextChord == 4)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[2] *= 3;
                                noteWeights[4] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[9] *= 3;
                                noteWeights[11] *= 3;
                            }
                            if (nextChord == 5)
                            {
                                noteWeights[0] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[5] *= 3;
                                noteWeights[7] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[12] *= 3;
                            }
                            if (nextChord == 6)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[6] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[13] *= 3;
                            }
                            if (nextChord == 7)
                            {
                                noteWeights[0] *= 3;
                                noteWeights[2] *= 3;
                                noteWeights[5] *= 3;
                                noteWeights[7] *= 3;
                                noteWeights[9] *= 3;
                                noteWeights[12] *= 3;
                            }


                        }
                        if (thisChord == 1)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[2] *= 2;
                            noteWeights[4] *= 1;
                            noteWeights[7] *= 3;
                            noteWeights[9] *= 2;
                            noteWeights[11] *= 1;
                        }
                        if (thisChord == 2)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 1;
                            noteWeights[5] *= 2;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 1;
                            noteWeights[12] *= 2;
                        }
                        if (thisChord == 3)
                        {
                            noteWeights[2] *= 3;
                            noteWeights[4] *= 2;
                            noteWeights[6] *= 1;
                            noteWeights[9] *= 3;
                            noteWeights[11] *= 2;
                            noteWeights[13] *= 1;
                        }
                        if (thisChord == 4)
                        {
                            noteWeights[3] *= 3;
                            noteWeights[5] *= 1;
                            noteWeights[7] *= 2;
                            noteWeights[10] *= 3;
                            noteWeights[12] *= 1;
                            noteWeights[0] *= 2;
                        }
                        if (thisChord == 5)
                        {
                            noteWeights[4] *= 1;
                            noteWeights[6] *= 2;
                            noteWeights[8] *= 3;
                            noteWeights[11] *= 1;
                            noteWeights[13] *= 2;
                            noteWeights[1] *= 3;
                        }
                        if (thisChord == 6)
                        {
                            noteWeights[5] *= 2;
                            noteWeights[7] *= 1;
                            noteWeights[9] *= 3;
                            noteWeights[12] *= 2;
                            noteWeights[0] *= 1;
                            noteWeights[2] *= 3;
                        }
                        if (thisChord == 7)
                        {
                            noteWeights[6] *= 2;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 1;
                            noteWeights[13] *= 2;
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 1;
                        }
                        //if the note is not a chord tone, set the weight to be 0
                        for (int j = 0; j < 14; j++)
                        {
                            int thisChordIndex = thisChord - 1;
                            if (j != thisChordIndex && j != thisChordIndex + 2 && j != thisChordIndex + 4 && j != thisChordIndex + 7 && j != (thisChordIndex + 9) % 13 && j != (thisChordIndex + 11) % 13)
                            {
                                noteWeights[j] = 0;
                            }
                        }


                    }
                    else if (currentSum + noteRhythm == chordLength)
                    {

                        //weight towards notes that lead into the next chord
                        if (nextChord == 1)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[6] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[13] *= 3;
                        }
                        if (nextChord == 2)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[2] *= 3;
                            noteWeights[4] *= 3;
                            noteWeights[7] *= 3;
                            noteWeights[9] *= 3;
                            noteWeights[11] *= 3;
                        }
                        if (nextChord == 3)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[5] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[12] *= 3;
                            noteWeights[0] *= 3;
                            noteWeights[7] *= 3;
                        }
                        if (nextChord == 4)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[2] *= 3;
                            noteWeights[4] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[9] *= 3;
                            noteWeights[11] *= 3;
                        }
                        if (nextChord == 5)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[5] *= 3;
                            noteWeights[7] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[12] *= 3;
                        }
                        if (nextChord == 6)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[6] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[13] *= 3;
                        }
                        if (nextChord == 7)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[2] *= 3;
                            noteWeights[5] *= 3;
                            noteWeights[7] *= 3;
                            noteWeights[9] *= 3;
                            noteWeights[12] *= 3;
                        }
                    }
                    //If a note isn't first or last of a chord. Double it's weighting towards chord tones.
                    else
                    {
                        int indexerForChordVals = thisChord - 1;
                        for (int j = 0; j < 6; j++)
                        {
                            noteWeights[indexerForChordVals] *= 2;
                            if (j == 2)
                                indexerForChordVals += 3;
                            else
                                indexerForChordVals += 2;
                            indexerForChordVals %= 14;
                        }

                    }
                    //Selects a note randomly based upon weighting
                    int sumWeights = 0;
                    for (int j = 0; j < 14; j++)
                    {
                        sumWeights += noteWeights[j];
                    }

                    int randOut = randomizer.Next(sumWeights);
                    sumWeights = 0;
                    for (int j = 0; j < 14; j++)
                    {
                        sumWeights += noteWeights[j];
                        if (randOut < sumWeights)
                        {
                            noteVal = notearray[j];
                            break;
                        }

                    }
                    //Raise and lower leading tones in dominant functions of minor modes in melodic lines
                    if (mode == 1)
                    {
                        if (thisChord == 5 || thisChord == 7)
                        {
                            if (noteVal.Equals(notearray[6]) || noteVal.Equals(notearray[13]))
                            {
                                if (noteVal[1] == '#')
                                {
                                    octave = int.Parse(noteVal[2].ToString());
                                    String sub = noteVal.Substring(0, 2);
                                    int index = Array.IndexOf(notes, sub);
                                    noteVal = notes[(index + 1) % 12] + octave.ToString();
                                }
                                else
                                {
                                    octave = int.Parse(noteVal[1].ToString());
                                    String sub = noteVal.Substring(0, 1);
                                    int index = Array.IndexOf(notes, sub);
                                    if (sub.Equals("B"))
                                    {
                                        octave++;
                                    }
                                    noteVal = notes[(index + 1) % 12] + octave.ToString();
                                }
                            }
                        }

                    }
                    thisSection.melodies[0].melodicLine.Add(new Song.Note(noteVal, noteRhythm));
                    currentSum += noteRhythm;
                    totalSectionSum += noteRhythm;
                    prevNoteVal = noteVal;
                }
            }
        }

        void composeJazzMelody(Song.SongSegment thisSection, Random randomizer, String key, int mode, String timeSigPattern, int timeSigQuant, int tempo)
        {
            //TODO check for bad input
            int chordLength;
            int currentSum = 0;
            String prevNoteVal = "";
            String noteVal = "";
            int noteRhythm = 0;
            String[] notes = { "A", "A#", "B", "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#" };
            String[] keySig = new String[7];
            String[] notearray = new String[14];
            int keynum = Array.IndexOf(notes, key);
            thisSection.melodies.Add(new Song.Melody());

            if (mode == 0)
            {
                keySig[0] = notes[keynum];
                keySig[1] = notes[(keynum + 2) % 12];
                keySig[2] = notes[(keynum + 4) % 12];
                keySig[3] = notes[(keynum + 5) % 12];
                keySig[4] = notes[(keynum + 7) % 12];
                keySig[5] = notes[(keynum + 9) % 12];
                keySig[6] = notes[(keynum + 11) % 12];

            }
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

            int octave = 3;
            String lastNote = "";
            String nextNote = "";
            int thisChord = 0;
            int nextChord = 0;
            for (int i = 0; i < 14; i++)
            {
                nextNote = keySig[i % 7];
                if (!lastNote.Equals("") && !nextNote.Equals("") && lastNote[0] < 'C' && nextNote[0] >= 'C')
                {
                    octave++;
                }
                notearray[i] = nextNote + octave.ToString();
                lastNote = nextNote;
            }
            int totalSectionSum = 0;
            int measureLen = 0;
            if (timeSigPattern.Equals("Simple"))
            {
                measureLen += 4;
            }
            else
                measureLen += 6;
            measureLen *= timeSigQuant;
            for (int i = 0; i < thisSection.chordPattern.Count; i++)
            {
                chordLength = thisSection.chordPattern[i].chordVoice.First().length;
                currentSum = 0;
                while (currentSum < chordLength)
                {

                    //Randomly pick a length within the current measure that doesn't overlap chords
                    //int maxVal = Math.Min(measureLen - (totalSectionSum % measureLen), chordLength - currentSum);
                    int maxVal;
                    if (tempo >= 130)
                    {
                        maxVal = Math.Min((measureLen - (totalSectionSum % measureLen)) / 2, (chordLength - currentSum) / 2);
                    }
                    else
                        maxVal = Math.Min(measureLen - (totalSectionSum % measureLen), chordLength - currentSum);
                    //If a melody falls onto the last chord of a segment just make the rhythm of note #1 the length of the chord
                    if (i == thisSection.chordPattern.Count - 1)
                    {
                        noteRhythm = randomizer.Next(0, chordLength);
                    }
                    else
                    {
                        int[] rhythmWeight = new int[maxVal];
                        for (int j = 0; j < maxVal; j++)
                        {
                            int beatSize = measureLen / timeSigQuant;
                            if (j > beatSize)
                            {
                                rhythmWeight[j] = 4;

                            }
                            else
                            {
                                rhythmWeight[j] = 1;

                            }
                            if (maxVal < beatSize)
                            {
                                rhythmWeight[maxVal - 1] = 10;

                            }
                            if (j + 1 % beatSize == 0)
                            {
                                rhythmWeight[j] *= 4;

                            }
                            if (j + 1 == beatSize)
                            {
                                rhythmWeight[j] *= 4;

                            }

                        }
                        int sumRythWeights = 0;
                        for (int k = 0; k < maxVal; k++)
                        {
                            sumRythWeights += rhythmWeight[k];
                        }

                        int randOutput = randomizer.Next(sumRythWeights);
                        sumRythWeights = 0;
                        for (int k = 0; k < maxVal; k++)
                        {
                            sumRythWeights += rhythmWeight[k];
                            if (randOutput < sumRythWeights)
                            {
                                noteRhythm = k + 1;
                                break;
                            }

                        }
                    }



                    //Define noteValue for each note
                    int[] noteWeights = new int[14];
                    if (prevNoteVal.Equals(""))
                    {
                        for (int j = 0; j < 14; j++)
                        {
                            noteWeights[j] = 1;

                        }

                    }
                    else
                    {
                        int index = Array.IndexOf(notearray, prevNoteVal);
                        int difference = 0;
                        for (int j = 0; j < 14; j++)
                        {
                            difference = index - j;
                            difference = Math.Abs(difference);
                            switch (difference)
                            {
                                case 0:
                                    noteWeights[j] = 16;
                                    break;
                                case 1:
                                    noteWeights[j] = 14;
                                    break;
                                case 2:
                                    noteWeights[j] = 10;
                                    break;
                                case 3:
                                    noteWeights[j] = 3;
                                    break;
                                case 4:
                                    noteWeights[j] = 4;
                                    break;
                                case 5:
                                    noteWeights[j] = 3;
                                    break;
                                case 6:
                                    noteWeights[j] = 2;
                                    break;
                                case 7:
                                    noteWeights[j] = 4;
                                    break;
                                case 8:
                                    noteWeights[j] = 1;
                                    break;
                                case 9:
                                    noteWeights[j] = 2;
                                    break;
                                case 10:
                                    noteWeights[j] = 1;
                                    break;
                                case 11:
                                    noteWeights[j] = 1;
                                    break;
                                case 12:
                                    noteWeights[j] = 1;
                                    break;
                                case 13:
                                    noteWeights[j] = 1;
                                    break;


                            }


                        }

                    }

                    //Defines the current chord and the upcoming chord for usage below
                    thisChord = thisSection.chordPattern[i].chordVal;
                    if (i < thisSection.chordPattern.Count - 1)
                    {
                        nextChord = thisSection.chordPattern[i + 1].chordVal;

                    }
                    else
                    {
                        nextChord = 0;

                    }

                    if (currentSum == 0)
                    {
                        //If first and last note of the chord
                        if (currentSum + noteRhythm == chordLength)
                        {
                            //if chord is last chord weight towards chord
                            if (nextChord == 0)
                            {
                                int indexerForChordVals = thisChord - 1;
                                for (int j = 0; j < 6; j++)
                                {
                                    noteWeights[indexerForChordVals] *= 2;
                                    if (j == 2)
                                        indexerForChordVals += 3;
                                    else
                                        indexerForChordVals += 2;
                                    indexerForChordVals %= 14;
                                }

                            }
                            //Otherwise weight towards notes that lead into the next chord
                            if (nextChord == 1)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[6] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[13] *= 3;
                            }
                            if (nextChord == 2)
                            {
                                noteWeights[0] *= 3;
                                noteWeights[2] *= 3;
                                noteWeights[4] *= 3;
                                noteWeights[7] *= 3;
                                noteWeights[9] *= 3;
                                noteWeights[11] *= 3;
                            }
                            if (nextChord == 3)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[5] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[12] *= 3;
                                noteWeights[0] *= 3;
                                noteWeights[7] *= 3;
                            }
                            if (nextChord == 4)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[2] *= 3;
                                noteWeights[4] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[9] *= 3;
                                noteWeights[11] *= 3;
                            }
                            if (nextChord == 5)
                            {
                                noteWeights[0] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[5] *= 3;
                                noteWeights[7] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[12] *= 3;
                            }
                            if (nextChord == 6)
                            {
                                noteWeights[1] *= 3;
                                noteWeights[3] *= 3;
                                noteWeights[6] *= 3;
                                noteWeights[8] *= 3;
                                noteWeights[10] *= 3;
                                noteWeights[13] *= 3;
                            }
                            if (nextChord == 7)
                            {
                                noteWeights[0] *= 3;
                                noteWeights[2] *= 3;
                                noteWeights[5] *= 3;
                                noteWeights[7] *= 3;
                                noteWeights[9] *= 3;
                                noteWeights[12] *= 3;
                            }


                        }
                        if (thisChord == 1)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[2] *= 2;
                            noteWeights[4] *= 1;
                            noteWeights[7] *= 3;
                            noteWeights[9] *= 2;
                            noteWeights[11] *= 1;
                        }
                        if (thisChord == 2)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 1;
                            noteWeights[5] *= 2;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 1;
                            noteWeights[12] *= 2;
                        }
                        if (thisChord == 3)
                        {
                            noteWeights[2] *= 3;
                            noteWeights[4] *= 2;
                            noteWeights[6] *= 1;
                            noteWeights[9] *= 3;
                            noteWeights[11] *= 2;
                            noteWeights[13] *= 1;
                        }
                        if (thisChord == 4)
                        {
                            noteWeights[3] *= 3;
                            noteWeights[5] *= 1;
                            noteWeights[7] *= 2;
                            noteWeights[10] *= 3;
                            noteWeights[12] *= 1;
                            noteWeights[0] *= 2;
                        }
                        if (thisChord == 5)
                        {
                            noteWeights[4] *= 1;
                            noteWeights[6] *= 2;
                            noteWeights[8] *= 3;
                            noteWeights[11] *= 1;
                            noteWeights[13] *= 2;
                            noteWeights[1] *= 3;
                        }
                        if (thisChord == 6)
                        {
                            noteWeights[5] *= 2;
                            noteWeights[7] *= 1;
                            noteWeights[9] *= 3;
                            noteWeights[12] *= 2;
                            noteWeights[0] *= 1;
                            noteWeights[2] *= 3;
                        }
                        if (thisChord == 7)
                        {
                            noteWeights[6] *= 2;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 1;
                            noteWeights[13] *= 2;
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 1;
                        }
                        //if the note is not a chord tone, set the weight to be 0
                        for (int j = 0; j < 14; j++)
                        {
                            int thisChordIndex = thisChord - 1;
                            if (j != thisChordIndex && j != thisChordIndex + 2 && j != thisChordIndex + 4 && j != thisChordIndex + 7 && j != (thisChordIndex + 9) % 13 && j != (thisChordIndex + 11) % 13)
                            {
                                noteWeights[j] = 0;
                            }
                        }


                    }
                    else if (currentSum + noteRhythm == chordLength)
                    {

                        //weight towards notes that lead into the next chord
                        if (nextChord == 1)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[6] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[13] *= 3;
                        }
                        if (nextChord == 2)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[2] *= 3;
                            noteWeights[4] *= 3;
                            noteWeights[7] *= 3;
                            noteWeights[9] *= 3;
                            noteWeights[11] *= 3;
                        }
                        if (nextChord == 3)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[5] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[12] *= 3;
                            noteWeights[0] *= 3;
                            noteWeights[7] *= 3;
                        }
                        if (nextChord == 4)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[2] *= 3;
                            noteWeights[4] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[9] *= 3;
                            noteWeights[11] *= 3;
                        }
                        if (nextChord == 5)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[5] *= 3;
                            noteWeights[7] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[12] *= 3;
                        }
                        if (nextChord == 6)
                        {
                            noteWeights[1] *= 3;
                            noteWeights[3] *= 3;
                            noteWeights[6] *= 3;
                            noteWeights[8] *= 3;
                            noteWeights[10] *= 3;
                            noteWeights[13] *= 3;
                        }
                        if (nextChord == 7)
                        {
                            noteWeights[0] *= 3;
                            noteWeights[2] *= 3;
                            noteWeights[5] *= 3;
                            noteWeights[7] *= 3;
                            noteWeights[9] *= 3;
                            noteWeights[12] *= 3;
                        }
                    }
                    //If a note isn't first or last of a chord. Double it's weighting towards chord tones.
                    else
                    {
                        int indexerForChordVals = thisChord - 1;
                        for (int j = 0; j < 6; j++)
                        {
                            noteWeights[indexerForChordVals] *= 2;
                            if (j == 2)
                                indexerForChordVals += 3;
                            else
                                indexerForChordVals += 2;
                            indexerForChordVals %= 14;
                        }

                    }
                    //Selects a note randomly based upon weighting
                    int sumWeights = 0;
                    for (int j = 0; j < 14; j++)
                    {
                        sumWeights += noteWeights[j];
                    }

                    int randOut = randomizer.Next(sumWeights);
                    sumWeights = 0;
                    for (int j = 0; j < 14; j++)
                    {
                        sumWeights += noteWeights[j];
                        if (randOut < sumWeights)
                        {
                            noteVal = notearray[j];
                            break;
                        }

                    }
                    //Raise and lower leading tones in dominant functions of minor modes in melodic lines
                    if (mode == 1)
                    {
                        if (thisChord == 5 || thisChord == 7)
                        {
                            if (noteVal.Equals(notearray[6]) || noteVal.Equals(notearray[13]))
                            {
                                if (noteVal[1] == '#')
                                {
                                    octave = int.Parse(noteVal[2].ToString());
                                    String sub = noteVal.Substring(0, 2);
                                    int index = Array.IndexOf(notes, sub);
                                    noteVal = notes[(index + 1)%12] + octave.ToString();
                                }
                                else
                                {
                                    octave = int.Parse(noteVal[1].ToString());
                                    String sub = noteVal.Substring(0, 1);
                                    int index = Array.IndexOf(notes, sub);
                                    if (sub.Equals("B"))
                                    {
                                        octave++;
                                    }
                                    noteVal = notes[(index + 1) % 12] + octave.ToString();
                                }
                            }
                        }

                    }
                    thisSection.melodies[0].melodicLine.Add(new Song.Note(noteVal, noteRhythm));
                    currentSum += noteRhythm;
                    totalSectionSum += noteRhythm;
                    prevNoteVal = noteVal;
                }
            }
        }
    }
}
