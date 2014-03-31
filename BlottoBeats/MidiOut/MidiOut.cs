using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sanford.Collections;
using Sanford.Multimedia;
using Sanford.Multimedia.Midi;
using Sanford.Multimedia.Timers;
using Sanford.Threading;
using SongData;
using System.Media;
using System.IO;

namespace MidiOut
{
    public class MidiOut
    {
        //Key
        KeySignatureBuilder keybuilder = new KeySignatureBuilder();
        //Note(s)
        ChannelMessageBuilder builder = new ChannelMessageBuilder();
        //Tempo
        TempoChangeBuilder tempoBuilder = new TempoChangeBuilder();
        //Sequencer (according to the C# MIDI Toolkit documentation, we need this to playback the sequence of MIDI messages). If you don't feel it's necessary, feel free to remove it.
        Sequencer s = new Sequencer();
        //Sequence
        Sequence sequence = new Sequence();

        public double outputToMidi(Song output)
        {
            //5 Tracks (Just in case; so far MIDIOut is currently utilizing 2: 1 for instrument and another for notes). Each track added to sequence right after
            Track[] track = new Track[5];
            double songLen = 0;
            for (int i = 0; i < 5; i++)
            {
                track[i] = new Track();
                sequence.Add(track[i]);
            }
            //Set the tempo (assuming Song.Tempo is in bpm)
            tempoBuilder.Tempo = (int)(1.0 / output.Tempo * 60000000);
            tempoBuilder.Build();
            track[0].Insert(0, tempoBuilder.Result);
            //Set instrument
            /*builder.Command = ChannelCommand.ProgramChange;
            //builder.Data1 = (int)GeneralMidiInstrument.AcousticGrandPiano;
            builder.Data2 = 0;
            builder.Build();
            track[1].Insert(0, builder.Result);*/
            //Tick position
            int pos = 0;
            //Note length
            int len = 0;
            //MidiChannel number
            int c = 1;
            //Set MidiChannel number to 1
            builder.MidiChannel = c;
            //Iterate through the chord voice and turn them on; Each iteration will save the note and length values
            for (int i=0; i<output.songData.Count; i++)
            {
                for (int j=0; j<output.songData[i].chordPattern.Count; j++)
                {
                    //Reset channel values to stop outOfBoundsException : Austin
                    c = 1;
                    builder.MidiChannel = c;
                    foreach (var item in (output.songData[i].chordPattern[j].chordVoice))
                    {
                        String note = item.noteValue; //C, C#, D, D#, E, F, F#, G, G#, A, A#
                        len = item.length; //in 16th notes
                        //Switch note on
                        builder.Command = ChannelCommand.ProgramChange;
                        if (output.Genre == "Chord Progression")
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.AcousticGrandPiano;
                        }
                        else if (output.Genre == "Classical")
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.Violin;
                        }
                        builder.Data2 = 0;
                        builder.Build();
                        track[1].Insert(pos, builder.Result);
                        builder.Command = ChannelCommand.NoteOn;
                        switch (note)
                        {
                            case "C0":
                                builder.Data1 = 0;
                                break;
                            case "C#0":
                                builder.Data1 = 1;
                                break;
                            case "D0":
                                builder.Data1 = 2;
                                break;
                            case "D#0":
                                builder.Data1 = 3;
                                break;
                            case "E0":
                                builder.Data1 = 4;
                                break;
                            case "F0":
                                builder.Data1 = 5;
                                break;
                            case "F#0":
                                builder.Data1 = 6;
                                break;
                            case "G0":
                                builder.Data1 = 7;
                                break;
                            case "G#0":
                                builder.Data1 = 8;
                                break;
                            case "A0":
                                builder.Data1 = 9;
                                break;
                            case "A#0":
                                builder.Data1 = 10;
                                break;
                            case "B0":
                                builder.Data1 = 11;
                                break;
                            case "C1":
                                builder.Data1 = 12;
                                break;
                            case "C#1":
                                builder.Data1 = 13;
                                break;
                            case "D1":
                                builder.Data1 = 14;
                                break;
                            case "D#1":
                                builder.Data1 = 15;
                                break;
                            case "E1":
                                builder.Data1 = 16;
                                break;
                            case "F1":
                                builder.Data1 = 17;
                                break;
                            case "F#1":
                                builder.Data1 = 18;
                                break;
                            case "G1":
                                builder.Data1 = 19;
                                break;
                            case "G#1":
                                builder.Data1 = 20;
                                break;
                            case "A1":
                                builder.Data1 = 21;
                                break;
                            case "A#1":
                                builder.Data1 = 22;
                                break;
                            case "B1":
                                builder.Data1 = 23;
                                break;
                            case "C2":
                                builder.Data1 = 24;
                                break;
                            case "C#2":
                                builder.Data1 = 25;
                                break;
                            case "D2":
                                builder.Data1 = 26;
                                break;
                            case "D#2":
                                builder.Data1 = 27;
                                break;
                            case "E2":
                                builder.Data1 = 28;
                                break;
                            case "F2":
                                builder.Data1 = 29;
                                break;
                            case "F#2":
                                builder.Data1 = 30;
                                break;
                            case "G2":
                                builder.Data1 = 31;
                                break;
                            case "G#2":
                                builder.Data1 = 32;
                                break;
                            case "A2":
                                builder.Data1 = 33;
                                break;
                            case "A#2":
                                builder.Data1 = 34;
                                break;
                            case "B2":
                                builder.Data1 = 35;
                                break;
                            case "C3":
                                builder.Data1 = 36;
                                break;
                            case "C#3":
                                builder.Data1 = 37;
                                break;
                            case "D3":
                                builder.Data1 = 38;
                                break;
                            case "D#3":
                                builder.Data1 = 39;
                                break;
                            case "E3":
                                builder.Data1 = 40;
                                break;
                            case "F3":
                                builder.Data1 = 41;
                                break;
                            case "F#3":
                                builder.Data1 = 42;
                                break;
                            case "G3":
                                builder.Data1 = 43;
                                break;
                            case "G#3":
                                builder.Data1 = 44;
                                break;
                            case "A3":
                                builder.Data1 = 45;
                                break;
                            case "A#3":
                                builder.Data1 = 46;
                                break;
                            case "B3":
                                builder.Data1 = 47;
                                break;
                            case "C4":
                                builder.Data1 = 48;
                                break;
                            case "C#4":
                                builder.Data1 = 49;
                                break;
                            case "D4":
                                builder.Data1 = 50;
                                break;
                            case "D#4":
                                builder.Data1 = 51;
                                break;
                            case "E4":
                                builder.Data1 = 52;
                                break;
                            case "F4":
                                builder.Data1 = 53;
                                break;
                            case "F#4":
                                builder.Data1 = 54;
                                break;
                            case "G4":
                                builder.Data1 = 55;
                                break;
                            case "G#4":
                                builder.Data1 = 56;
                                break;
                            case "A4":
                                builder.Data1 = 57;
                                break;
                            case "A#4":
                                builder.Data1 = 58;
                                break;
                            case "B4":
                                builder.Data1 = 59;
                                break;
                            case "C5":
                                builder.Data1 = 60;
                                break;
                            case "C#5":
                                builder.Data1 = 61;
                                break;
                            case "D5":
                                builder.Data1 = 62;
                                break;
                            case "D#5":
                                builder.Data1 = 63;
                                break;
                            case "E5":
                                builder.Data1 = 64;
                                break;
                            case "F5":
                                builder.Data1 = 65;
                                break;
                            case "F#5":
                                builder.Data1 = 66;
                                break;
                            case "G5":
                                builder.Data1 = 67;
                                break;
                            case "G#5":
                                builder.Data1 = 68;
                                break;
                            case "A5":
                                builder.Data1 = 69;
                                break;
                            case "A#5":
                                builder.Data1 = 70;
                                break;
                            case "B5":
                                builder.Data1 = 71;
                                break;
                            case "C6":
                                builder.Data1 = 72;
                                break;
                            case "C#6":
                                builder.Data1 = 73;
                                break;
                            case "D6":
                                builder.Data1 = 74;
                                break;
                            case "D#6":
                                builder.Data1 = 75;
                                break;
                            case "E6":
                                builder.Data1 = 76;
                                break;
                            case "F6":
                                builder.Data1 = 77;
                                break;
                            case "F#6":
                                builder.Data1 = 78;
                                break;
                            case "G6":
                                builder.Data1 = 79;
                                break;
                            case "G#6":
                                builder.Data1 = 80;
                                break;
                            case "A6":
                                builder.Data1 = 81;
                                break;
                            case "A#6":
                                builder.Data1 = 82;
                                break;
                            case "B6":
                                builder.Data1 = 83;
                                break;
                            case "C7":
                                builder.Data1 = 84;
                                break;
                            case "C#7":
                                builder.Data1 = 85;
                                break;
                            case "D7":
                                builder.Data1 = 86;
                                break;
                            case "D#7":
                                builder.Data1 = 87;
                                break;
                            case "E7":
                                builder.Data1 = 88;
                                break;
                            case "F7":
                                builder.Data1 = 89;
                                break;
                            case "F#7":
                                builder.Data1 = 90;
                                break;
                            case "G7":
                                builder.Data1 = 91;
                                break;
                            case "G#7":
                                builder.Data1 = 92;
                                break;
                            case "A7":
                                builder.Data1 = 93;
                                break;
                            case "A#7":
                                builder.Data1 = 94;
                                break;
                            case "B7":
                                builder.Data1 = 95;
                                break;
                            case "C8":
                                builder.Data1 = 96;
                                break;
                            case "C#8":
                                builder.Data1 = 97;
                                break;
                            case "D8":
                                builder.Data1 = 98;
                                break;
                            case "D#8":
                                builder.Data1 = 99;
                                break;
                            case "E8":
                                builder.Data1 = 100;
                                break;
                            case "F8":
                                builder.Data1 = 101;
                                break;
                            case "F#8":
                                builder.Data1 = 102;
                                break;
                            case "G8":
                                builder.Data1 = 103;
                                break;
                            case "G#8":
                                builder.Data1 = 104;
                                break;
                            case "A8":
                                builder.Data1 = 105;
                                break;
                            case "A#8":
                                builder.Data1 = 106;
                                break;
                            case "B8":
                                builder.Data1 = 107;
                                break;
                            case "C9":
                                builder.Data1 = 108;
                                break;
                            case "C#9":
                                builder.Data1 = 109;
                                break;
                            case "D9":
                                builder.Data1 = 110;
                                break;
                            case "D#9":
                                builder.Data1 = 111;
                                break;
                            case "E9":
                                builder.Data1 = 112;
                                break;
                            case "F9":
                                builder.Data1 = 113;
                                break;
                            case "F#9":
                                builder.Data1 = 114;
                                break;
                            case "G9":
                                builder.Data1 = 115;
                                break;
                            case "G#9":
                                builder.Data1 = 116;
                                break;
                            case "A9":
                                builder.Data1 = 117;
                                break;
                            case "A#9":
                                builder.Data1 = 118;
                                break;
                            case "B9":
                                builder.Data1 = 119;
                                break;
                            case "C10":
                                builder.Data1 = 120;
                                break;
                            case "C#10":
                                builder.Data1 = 121;
                                break;
                            case "D10":
                                builder.Data1 = 122;
                                break;
                            case "D#10":
                                builder.Data1 = 123;
                                break;
                            case "E10":
                                builder.Data1 = 124;
                                break;
                            case "F10":
                                builder.Data1 = 125;
                                break;
                            case "F#10":
                                builder.Data1 = 126;
                                break;
                            case "G10":
                                builder.Data1 = 127;
                                break;
                        }//endswitch
                        builder.Data2 = 127; //Set volume to max
                        builder.Build(); //Build the message
                        track[1].Insert(pos, builder.Result); //Insert into Track 1 at tick position 'pos'
                        //Increment MIDI channel by 1
                        c += 1;
                        builder.MidiChannel = c;
                        
                    }//endforeach
                    /*Tick position increment; This will be based on the last note of the previous chord, but I think it's safe to assume that its length will be the same as the rest of the chord tones of that chord.
                     PpqnClock.PpqnMinValue is the minimum PPQ value (24) set by the class library PpqnClock.*/
                    songLen += len / 4;
                    pos += (PpqnClock.PpqnMinValue / 4 * len);
                    c = 1;
                    builder.MidiChannel = c;

                    foreach (var item in (output.songData[i].chordPattern[j].chordVoice))
                    {
                        String note = item.noteValue;
                        len = item.length;
                        //Set Note Off
                        builder.Command = ChannelCommand.NoteOff;
                        switch (note)
                        {
                            case "C0":
                                builder.Data1 = 0;
                                break;
                            case "C#0":
                                builder.Data1 = 1;
                                break;
                            case "D0":
                                builder.Data1 = 2;
                                break;
                            case "D#0":
                                builder.Data1 = 3;
                                break;
                            case "E0":
                                builder.Data1 = 4;
                                break;
                            case "F0":
                                builder.Data1 = 5;
                                break;
                            case "F#0":
                                builder.Data1 = 6;
                                break;
                            case "G0":
                                builder.Data1 = 7;
                                break;
                            case "G#0":
                                builder.Data1 = 8;
                                break;
                            case "A0":
                                builder.Data1 = 9;
                                break;
                            case "A#0":
                                builder.Data1 = 10;
                                break;
                            case "B0":
                                builder.Data1 = 11;
                                break;
                            case "C1":
                                builder.Data1 = 12;
                                break;
                            case "C#1":
                                builder.Data1 = 13;
                                break;
                            case "D1":
                                builder.Data1 = 14;
                                break;
                            case "D#1":
                                builder.Data1 = 15;
                                break;
                            case "E1":
                                builder.Data1 = 16;
                                break;
                            case "F1":
                                builder.Data1 = 17;
                                break;
                            case "F#1":
                                builder.Data1 = 18;
                                break;
                            case "G1":
                                builder.Data1 = 19;
                                break;
                            case "G#1":
                                builder.Data1 = 20;
                                break;
                            case "A1":
                                builder.Data1 = 21;
                                break;
                            case "A#1":
                                builder.Data1 = 22;
                                break;
                            case "B1":
                                builder.Data1 = 23;
                                break;
                            case "C2":
                                builder.Data1 = 24;
                                break;
                            case "C#2":
                                builder.Data1 = 25;
                                break;
                            case "D2":
                                builder.Data1 = 26;
                                break;
                            case "D#2":
                                builder.Data1 = 27;
                                break;
                            case "E2":
                                builder.Data1 = 28;
                                break;
                            case "F2":
                                builder.Data1 = 29;
                                break;
                            case "F#2":
                                builder.Data1 = 30;
                                break;
                            case "G2":
                                builder.Data1 = 31;
                                break;
                            case "G#2":
                                builder.Data1 = 32;
                                break;
                            case "A2":
                                builder.Data1 = 33;
                                break;
                            case "A#2":
                                builder.Data1 = 34;
                                break;
                            case "B2":
                                builder.Data1 = 35;
                                break;
                            case "C3":
                                builder.Data1 = 36;
                                break;
                            case "C#3":
                                builder.Data1 = 37;
                                break;
                            case "D3":
                                builder.Data1 = 38;
                                break;
                            case "D#3":
                                builder.Data1 = 39;
                                break;
                            case "E3":
                                builder.Data1 = 40;
                                break;
                            case "F3":
                                builder.Data1 = 41;
                                break;
                            case "F#3":
                                builder.Data1 = 42;
                                break;
                            case "G3":
                                builder.Data1 = 43;
                                break;
                            case "G#3":
                                builder.Data1 = 44;
                                break;
                            case "A3":
                                builder.Data1 = 45;
                                break;
                            case "A#3":
                                builder.Data1 = 46;
                                break;
                            case "B3":
                                builder.Data1 = 47;
                                break;
                            case "C4":
                                builder.Data1 = 48;
                                break;
                            case "C#4":
                                builder.Data1 = 49;
                                break;
                            case "D4":
                                builder.Data1 = 50;
                                break;
                            case "D#4":
                                builder.Data1 = 51;
                                break;
                            case "E4":
                                builder.Data1 = 52;
                                break;
                            case "F4":
                                builder.Data1 = 53;
                                break;
                            case "F#4":
                                builder.Data1 = 54;
                                break;
                            case "G4":
                                builder.Data1 = 55;
                                break;
                            case "G#4":
                                builder.Data1 = 56;
                                break;
                            case "A4":
                                builder.Data1 = 57;
                                break;
                            case "A#4":
                                builder.Data1 = 58;
                                break;
                            case "B4":
                                builder.Data1 = 59;
                                break;
                            case "C5":
                                builder.Data1 = 60;
                                break;
                            case "C#5":
                                builder.Data1 = 61;
                                break;
                            case "D5":
                                builder.Data1 = 62;
                                break;
                            case "D#5":
                                builder.Data1 = 63;
                                break;
                            case "E5":
                                builder.Data1 = 64;
                                break;
                            case "F5":
                                builder.Data1 = 65;
                                break;
                            case "F#5":
                                builder.Data1 = 66;
                                break;
                            case "G5":
                                builder.Data1 = 67;
                                break;
                            case "G#5":
                                builder.Data1 = 68;
                                break;
                            case "A5":
                                builder.Data1 = 69;
                                break;
                            case "A#5":
                                builder.Data1 = 70;
                                break;
                            case "B5":
                                builder.Data1 = 71;
                                break;
                            case "C6":
                                builder.Data1 = 72;
                                break;
                            case "C#6":
                                builder.Data1 = 73;
                                break;
                            case "D6":
                                builder.Data1 = 74;
                                break;
                            case "D#6":
                                builder.Data1 = 75;
                                break;
                            case "E6":
                                builder.Data1 = 76;
                                break;
                            case "F6":
                                builder.Data1 = 77;
                                break;
                            case "F#6":
                                builder.Data1 = 78;
                                break;
                            case "G6":
                                builder.Data1 = 79;
                                break;
                            case "G#6":
                                builder.Data1 = 80;
                                break;
                            case "A6":
                                builder.Data1 = 81;
                                break;
                            case "A#6":
                                builder.Data1 = 82;
                                break;
                            case "B6":
                                builder.Data1 = 83;
                                break;
                            case "C7":
                                builder.Data1 = 84;
                                break;
                            case "C#7":
                                builder.Data1 = 85;
                                break;
                            case "D7":
                                builder.Data1 = 86;
                                break;
                            case "D#7":
                                builder.Data1 = 87;
                                break;
                            case "E7":
                                builder.Data1 = 88;
                                break;
                            case "F7":
                                builder.Data1 = 89;
                                break;
                            case "F#7":
                                builder.Data1 = 90;
                                break;
                            case "G7":
                                builder.Data1 = 91;
                                break;
                            case "G#7":
                                builder.Data1 = 92;
                                break;
                            case "A7":
                                builder.Data1 = 93;
                                break;
                            case "A#7":
                                builder.Data1 = 94;
                                break;
                            case "B7":
                                builder.Data1 = 95;
                                break;
                            case "C8":
                                builder.Data1 = 96;
                                break;
                            case "C#8":
                                builder.Data1 = 97;
                                break;
                            case "D8":
                                builder.Data1 = 98;
                                break;
                            case "D#8":
                                builder.Data1 = 99;
                                break;
                            case "E8":
                                builder.Data1 = 100;
                                break;
                            case "F8":
                                builder.Data1 = 101;
                                break;
                            case "F#8":
                                builder.Data1 = 102;
                                break;
                            case "G8":
                                builder.Data1 = 103;
                                break;
                            case "G#8":
                                builder.Data1 = 104;
                                break;
                            case "A8":
                                builder.Data1 = 105;
                                break;
                            case "A#8":
                                builder.Data1 = 106;
                                break;
                            case "B8":
                                builder.Data1 = 107;
                                break;
                            case "C9":
                                builder.Data1 = 108;
                                break;
                            case "C#9":
                                builder.Data1 = 109;
                                break;
                            case "D9":
                                builder.Data1 = 110;
                                break;
                            case "D#9":
                                builder.Data1 = 111;
                                break;
                            case "E9":
                                builder.Data1 = 112;
                                break;
                            case "F9":
                                builder.Data1 = 113;
                                break;
                            case "F#9":
                                builder.Data1 = 114;
                                break;
                            case "G9":
                                builder.Data1 = 115;
                                break;
                            case "G#9":
                                builder.Data1 = 116;
                                break;
                            case "A9":
                                builder.Data1 = 117;
                                break;
                            case "A#9":
                                builder.Data1 = 118;
                                break;
                            case "B9":
                                builder.Data1 = 119;
                                break;
                            case "C10":
                                builder.Data1 = 120;
                                break;
                            case "C#10":
                                builder.Data1 = 121;
                                break;
                            case "D10":
                                builder.Data1 = 122;
                                break;
                            case "D#10":
                                builder.Data1 = 123;
                                break;
                            case "E10":
                                builder.Data1 = 124;
                                break;
                            case "F10":
                                builder.Data1 = 125;
                                break;
                            case "F#10":
                                builder.Data1 = 126;
                                break;
                            case "G10":
                                builder.Data1 = 127;
                                break;
                        }//endswitch
                        builder.Data2 = 0; //Set volume to mute
                        builder.Build(); //Build the message
                        track[1].Insert(pos, builder.Result); //Insert into Track 1 at tick position 'pos'
                        //Increment MIDI channel by 1
                        c += 1;
                        builder.MidiChannel = c;
                        //songLen += item.length / 16;
                    }//endforeach
                    
                }//endfor
            }//endfor
            //Submits file to the C:\BlottoBeats Folder where it is stored until another song is generated
            if (!Directory.Exists(@"C:\BlottoBeats"))
            {
                Directory.CreateDirectory(@"C:\BlottoBeats");

            }
            if(File.Exists(@"C:\BlottoBeats\temp.mid")){
                File.Delete(@"C:\BlottoBeats\temp.mid");

            }

            sequence.Save(@"C:\BlottoBeats\temp.mid");
            return ((double) pos)/(PpqnClock.PpqnMinValue / 4)/4 / output.Tempo * 60;

            //Code to play sequence in client. Currently not functioning
            //s.Sequence = sequence;  
           //s.Start();
        }
    }

}
