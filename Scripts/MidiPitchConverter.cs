using UnityEngine;


public static class MidiPitchConverter
{
    private const float MinPitch = 0.03125f;
    private const float MaxPitch = 15.10199f;
    private const int BaseC = 60; // C5
    private const int NotesPerOctave = 12;
    
    public static float MidiNoteToPitch(int midiNote)
    {
        float semitoneOffset = midiNote - BaseC;
        float pitch = Mathf.Pow(2, semitoneOffset / NotesPerOctave);
        return Mathf.Clamp(pitch, MinPitch, MaxPitch);
    }
    
    public static int PitchToMidiNote(float pitch)
    {
        float clampedPitch = Mathf.Clamp(pitch, MinPitch, MaxPitch);
        float semitoneOffset = Mathf.Log(clampedPitch, 2) * NotesPerOctave;
        return Mathf.RoundToInt(BaseC + semitoneOffset);
    }
}