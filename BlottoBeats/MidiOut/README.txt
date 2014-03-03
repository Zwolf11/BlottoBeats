Right now, MidiOut is currently modeled based on your example. Although I could not get the output to play piano 
sound, based on the website documentation, this way is closer and probably better than what I had previously.

What I had previously included having separate ChannelMessages for separate notes and separating the durations 
with Thread.Sleep().

I also say your way may be more correct because the toolkit should be designed where playback is all on the 
Sequencer class. Look at the comment for it in the .cs and the website for more detail.

There's also an error when trying to access the HashSet of notes, saying that 'chordPattern' is not defined. I tried to reference according to how the Song class in SongData is structured. If I am not making the correct references, 
please let me know so I can try and fix it.

The way I see it, the only real issue in terms of outputting actual sound is how to handle the MIDI ticks, since
each MIDI message is separated by ticks and won't be processed until certain number delta ticks has been reached.
This is what I was spending most of the time trying to figure out during our meeting on Sunday. 
If we can figure out how to convert bpm/note length to ticks, I think we should be able to get the output up and 
running.

As for the switch cases for the notes, I've covered everything from middle C to the highest G. I'll get the
rest of it (everything below) covered hopefully by tomorrow night.

Please take a look at it and get back to me asap. I'm usually not on Skype unless I have to be, so you can probably
reach me better through text.