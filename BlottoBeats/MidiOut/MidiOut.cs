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
using Generator;

namespace MidiOut
{
    public class MidiOut
    {
        //Note(s)
        ChannelMessageBuilder builder = new ChannelMessageBuilder();
        //Tempo
        TempoChangeBuilder tempoBuilder = new TempoChangeBuilder();
        //Sequencer (according to the C# MIDI Toolkit documentation, we need this to playback the sequence of MIDI messages). If you don't feel it's necessary, feel free to remove it.
        Sequencer s = new Sequencer();
        //Sequence
        Sequence sequence = new Sequence();

        public void outputToMidi(Song output)
        {
            //Tracks
            Track track0 = new Track();
            Track track1 = new Track();
            Track track2 = new Track();
            //Add tracks to the sequence
            sequence.Add(track0);
            sequence.Add(track1);
            sequence.Add(track2);
            //Set the tempo (assuming Song.Tempo is in bpm)
            tempoBuilder.Tempo = (int)(1 / output.Tempo * 60000000);
            tempoBuilder.Build();
            track0.Insert(0, tempoBuilder.Result);
            //Set instrument
            builder.Command = ChannelCommand.ProgramChange;
            builder.Data1 = (int)GeneralMidiInstrument.AcousticGrandPiano;
            builder.Data2 = 0;
            builder.Build();
            track1.Insert(0, builder.Result);
            //MidiChannel number
            int i = 1;

            builder.MidiChannel = i;
            //Iterate through the chord voice and turn them on; Each iteration will save the note and length values
            foreach (var item in (output.songData.chordPattern.chordVoice))
            {
                String note = output.songData.chordPattern.chordVoice.noteValue;
                int len = output.songData.chordPattern.chordVoice.length;

                channelBuilder.Command = ChannelCommand.NoteOn;
                switch (note)
                {
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
                }
                builder.Data2 = 127; // velocity 127
                builder.Build();
                track1.Insert(0, builder.Result);
                i += 1;
                builder.MidiChannel = i;
            }

            //Iterate through the chord voice again to turn the notes off
            i = 1;
            builder.MidiChannel = i;
            foreach (var item in (output.songData.chordPattern.chordVoice))
            {
                String note = output.songData.chordPattern.chordVoice.noteValue;
                int len = output.songData.chordPattern.chordVoice.length;

                builder.Command = ChannelCommand.NoteOff;
                switch (note)
                {
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
                }

                builder.Data2 = 0; // velocity 127
                builder.Build();
                track1.Insert(len, builder.Result);
                i += 1;
                builder.MidiChannel = i;
            }
            //Judging from the documentation from the toolkit website, this should output the sequence
            s.Sequence = sequence;
            s.Start();
        }
    }

}
