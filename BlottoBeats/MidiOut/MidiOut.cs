using BlottoBeats.Library.SongData;
using Sanford.Multimedia.Midi;
using System;
using System.IO;
using System.Linq;

namespace BlottoBeats.MidiOut
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
            int startOfSegment = 0;
            for (int i = 0; i < output.songData.Count; i++)
            {
                startOfSegment = pos;
                for (int j = 0; j < output.songData[i].chordPattern.Count; j++)
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
                        if (output.Genre == "Generic" || output.Genre == "Jazz")
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.AcousticGrandPiano;
                        }
                        else if (output.Genre == "Classical")
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.Violin;
                        }
                        else if (output.Genre == "4-Chord Pop/Rock")
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.ElectricGuitarMuted;
                        }
                        builder.Data2 = 0;
                        builder.Build();
                        track[1].Insert(pos, builder.Result);
                        builder.Command = ChannelCommand.NoteOn;
                        builder.Data1 = midiValOfNote(note);
                        if (output.Genre == "Generic")
                        {
                            builder.Data2 = 100;
                        }
                        else if (output.Genre == "Classical")
                        {
                            builder.Data2 = 80;
                        }
                        else if (output.Genre == "Jazz")
                        {
                            builder.Data2 = 110;
                        }
                        else if (output.Genre == "4-Chord Pop/Rock")
                        {
                            builder.Data2 = 120;
                        }
                        
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
                        builder.Data1 = midiValOfNote(note);
                        builder.Data2 = 0; //Set volume to mute
                        builder.Build(); //Build the message
                        track[1].Insert(pos, builder.Result); //Insert into Track 1 at tick position 'pos'
                        //Increment MIDI channel by 1
                        c += 1;
                        builder.MidiChannel = c;
                        //songLen += item.length / 16;
                    }//endforeach

                }//endfor

                for (int q = 0; q < output.songData[i].melodies.Count(); q++)
                {
                    pos = startOfSegment;
                    for (int j = 0; j < output.songData[i].melodies[q].melodicLine.Count(); j++)
                    {
                        Song.Note outputNote = output.songData[i].melodies[q].melodicLine[j];
                        String note = outputNote.noteValue;
                        int noteLength = outputNote.length; //in 16th notes
                        //Switch note on
                        builder.Command = ChannelCommand.ProgramChange;
                        if (output.Genre == "Generic")
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.ElectricGuitarJazz;
                        }
                        else if (output.Genre == "Classical")
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.Violin;
                        }
                        else if (output.Genre == "Twelve-tone")
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.AcousticGrandPiano;

                        }
                        else if (output.Genre == "Jazz")
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.AltoSax;
                        }
                        else if (output.Genre == "4-Chord Pop/Rock" && q==0)
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.ChoirAahs;
                        }
                        else if (output.Genre == "4-Chord Pop/Rock" && q==1)
                        {
                            builder.Data1 = (int)GeneralMidiInstrument.ElectricBassPick;
                        }
                        builder.Data2 = 0;
                        builder.Build();
                        track[q+2].Insert(pos, builder.Result);
                        builder.Command = ChannelCommand.NoteOn;
                        builder.Data1 = midiValOfNote(note);
                        builder.Data2 = 127; //Set volume to max
                        builder.Build(); //Build the message
                        track[q+2].Insert(pos, builder.Result); //Insert into Track 1 at tick position 'pos'
                        //Increment MIDI channel by 1
                        pos += (PpqnClock.PpqnMinValue / 4 * noteLength);
                        //Set Note Off
                        builder.Command = ChannelCommand.NoteOff;
                        builder.Data1 = midiValOfNote(note);
                        builder.Data2 = 0; //Set volume to mute
                        builder.Build(); //Build the message
                        track[q+2].Insert(pos, builder.Result);

                    }
                }
            }//endfor
            //Submits file to the C:\BlottoBeats Folder where it is stored until another song is generated
            /*if (!Directory.Exists(@"C:\BlottoBeats"))
            {
                Directory.CreateDirectory(@"C:\BlottoBeats");

            }*/
            if (File.Exists("temp.mid"))
            {
                File.Delete("temp.mid");

            }

            sequence.Save("temp.mid");

            return ((double)pos) / (PpqnClock.PpqnMinValue / 4) / 4 / output.Tempo * 60;

            //Code to play sequence in client. Currently not functioning
            //s.Sequence = sequence;  
            //s.Start();
        }
        private int midiValOfNote(String note)
        {
            int retval = 0;
            switch (note)
            {
                case "C0":
                    retval = 0;
                    break;
                case "C#0":
                    retval = 1;
                    break;
                case "D0":
                    retval = 2;
                    break;
                case "D#0":
                    retval = 3;
                    break;
                case "E0":
                    retval = 4;
                    break;
                case "F0":
                    retval = 5;
                    break;
                case "F#0":
                    retval = 6;
                    break;
                case "G0":
                    retval = 7;
                    break;
                case "G#0":
                    retval = 8;
                    break;
                case "A0":
                    retval = 9;
                    break;
                case "A#0":
                    retval = 10;
                    break;
                case "B0":
                    retval = 11;
                    break;
                case "C1":
                    retval = 12;
                    break;
                case "C#1":
                    retval = 13;
                    break;
                case "D1":
                    retval = 14;
                    break;
                case "D#1":
                    retval = 15;
                    break;
                case "E1":
                    retval = 16;
                    break;
                case "F1":
                    retval = 17;
                    break;
                case "F#1":
                    retval = 18;
                    break;
                case "G1":
                    retval = 19;
                    break;
                case "G#1":
                    retval = 20;
                    break;
                case "A1":
                    retval = 21;
                    break;
                case "A#1":
                    retval = 22;
                    break;
                case "B1":
                    retval = 23;
                    break;
                case "C2":
                    retval = 24;
                    break;
                case "C#2":
                    retval = 25;
                    break;
                case "D2":
                    retval = 26;
                    break;
                case "D#2":
                    retval = 27;
                    break;
                case "E2":
                    retval = 28;
                    break;
                case "F2":
                    retval = 29;
                    break;
                case "F#2":
                    retval = 30;
                    break;
                case "G2":
                    retval = 31;
                    break;
                case "G#2":
                    retval = 32;
                    break;
                case "A2":
                    retval = 33;
                    break;
                case "A#2":
                    retval = 34;
                    break;
                case "B2":
                    retval = 35;
                    break;
                case "C3":
                    retval = 36;
                    break;
                case "C#3":
                    retval = 37;
                    break;
                case "D3":
                    retval = 38;
                    break;
                case "D#3":
                    retval = 39;
                    break;
                case "E3":
                    retval = 40;
                    break;
                case "F3":
                    retval = 41;
                    break;
                case "F#3":
                    retval = 42;
                    break;
                case "G3":
                    retval = 43;
                    break;
                case "G#3":
                    retval = 44;
                    break;
                case "A3":
                    retval = 45;
                    break;
                case "A#3":
                    retval = 46;
                    break;
                case "B3":
                    retval = 47;
                    break;
                case "C4":
                    retval = 48;
                    break;
                case "C#4":
                    retval = 49;
                    break;
                case "D4":
                    retval = 50;
                    break;
                case "D#4":
                    retval = 51;
                    break;
                case "E4":
                    retval = 52;
                    break;
                case "F4":
                    retval = 53;
                    break;
                case "F#4":
                    retval = 54;
                    break;
                case "G4":
                    retval = 55;
                    break;
                case "G#4":
                    retval = 56;
                    break;
                case "A4":
                    retval = 57;
                    break;
                case "A#4":
                    retval = 58;
                    break;
                case "B4":
                    retval = 59;
                    break;
                case "C5":
                    retval = 60;
                    break;
                case "C#5":
                    retval = 61;
                    break;
                case "D5":
                    retval = 62;
                    break;
                case "D#5":
                    retval = 63;
                    break;
                case "E5":
                    retval = 64;
                    break;
                case "F5":
                    retval = 65;
                    break;
                case "F#5":
                    retval = 66;
                    break;
                case "G5":
                    retval = 67;
                    break;
                case "G#5":
                    retval = 68;
                    break;
                case "A5":
                    retval = 69;
                    break;
                case "A#5":
                    retval = 70;
                    break;
                case "B5":
                    retval = 71;
                    break;
                case "C6":
                    retval = 72;
                    break;
                case "C#6":
                    retval = 73;
                    break;
                case "D6":
                    retval = 74;
                    break;
                case "D#6":
                    retval = 75;
                    break;
                case "E6":
                    retval = 76;
                    break;
                case "F6":
                    retval = 77;
                    break;
                case "F#6":
                    retval = 78;
                    break;
                case "G6":
                    retval = 79;
                    break;
                case "G#6":
                    retval = 80;
                    break;
                case "A6":
                    retval = 81;
                    break;
                case "A#6":
                    retval = 82;
                    break;
                case "B6":
                    retval = 83;
                    break;
                case "C7":
                    retval = 84;
                    break;
                case "C#7":
                    retval = 85;
                    break;
                case "D7":
                    retval = 86;
                    break;
                case "D#7":
                    retval = 87;
                    break;
                case "E7":
                    retval = 88;
                    break;
                case "F7":
                    retval = 89;
                    break;
                case "F#7":
                    retval = 90;
                    break;
                case "G7":
                    retval = 91;
                    break;
                case "G#7":
                    retval = 92;
                    break;
                case "A7":
                    retval = 93;
                    break;
                case "A#7":
                    retval = 94;
                    break;
                case "B7":
                    retval = 95;
                    break;
                case "C8":
                    retval = 96;
                    break;
                case "C#8":
                    retval = 97;
                    break;
                case "D8":
                    retval = 98;
                    break;
                case "D#8":
                    retval = 99;
                    break;
                case "E8":
                    retval = 100;
                    break;
                case "F8":
                    retval = 101;
                    break;
                case "F#8":
                    retval = 102;
                    break;
                case "G8":
                    retval = 103;
                    break;
                case "G#8":
                    retval = 104;
                    break;
                case "A8":
                    retval = 105;
                    break;
                case "A#8":
                    retval = 106;
                    break;
                case "B8":
                    retval = 107;
                    break;
                case "C9":
                    retval = 108;
                    break;
                case "C#9":
                    retval = 109;
                    break;
                case "D9":
                    retval = 110;
                    break;
                case "D#9":
                    retval = 111;
                    break;
                case "E9":
                    retval = 112;
                    break;
                case "F9":
                    retval = 113;
                    break;
                case "F#9":
                    retval = 114;
                    break;
                case "G9":
                    retval = 115;
                    break;
                case "G#9":
                    retval = 116;
                    break;
                case "A9":
                    retval = 117;
                    break;
                case "A#9":
                    retval = 118;
                    break;
                case "B9":
                    retval = 119;
                    break;
                case "C10":
                    retval = 120;
                    break;
                case "C#10":
                    retval = 121;
                    break;
                case "D10":
                    retval = 122;
                    break;
                case "D#10":
                    retval = 123;
                    break;
                case "E10":
                    retval = 124;
                    break;
                case "F10":
                    retval = 125;
                    break;
                case "F#10":
                    retval = 126;
                    break;
                case "G10":
                    retval = 127;
                    break;
            }//endswitch
            return retval;

        }
    }


}
